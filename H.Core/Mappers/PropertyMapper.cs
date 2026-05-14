using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace H.Core.Mappers;

/// <summary>
/// Convention-based property mapper that copies public properties from source to destination
/// where property names and types match. Replaces AutoMapper's default behavior.
///
/// Per (TSource, TDest) pair, the reflection-driven plan (which source property feeds which
/// destination property, plus compiled getter/setter delegates) is built once and cached. This
/// keeps subsequent <see cref="Map{TSource, TDest}"/> calls allocation-light and avoids the
/// ~236 µs per-call cost of repeated <see cref="Type.GetProperties()"/> + reflective
/// GetValue/SetValue seen on the hot path inside MapDetailsScreenViewItemFromComponentScreenViewItem.
/// </summary>
public static class PropertyMapper
{
    /// <summary>
    /// One compiled copy step: take TSource + TDest, read the source property via a compiled
    /// delegate and write it onto the destination via another. Built once per pair of types and
    /// then replayed N times — no further reflection metadata walks at call time.
    /// </summary>
    private delegate void CopyStep(object source, object dest);

    private static readonly ConcurrentDictionary<(Type Source, Type Dest), CopyStep[]> _planCache = new();

    /// <summary>
    /// Creates a new TDest and copies all matching properties from source.
    /// </summary>
    public static TDest Map<TSource, TDest>(TSource source) where TDest : new()
    {
        var dest = new TDest();
        CopyTo(source, dest);
        return dest;
    }

    /// <summary>
    /// Copies matching properties from source onto an existing dest instance.
    /// Properties match by name, and the source property type must be assignable to the dest property type.
    /// Collection properties (IEnumerable, but not string) are skipped to avoid sharing references;
    /// callers are expected to copy collection items manually.
    /// Guid properties are skipped by default to preserve object identity; each object should keep
    /// its own unique Guid. Nullable&lt;T&gt; source properties are unwrapped to match non-nullable T
    /// destination properties.
    /// </summary>
    public static void CopyTo<TSource, TDest>(TSource source, TDest dest)
    {
        if (source is null || dest is null)
        {
            return;
        }

        var plan = _planCache.GetOrAdd((typeof(TSource), typeof(TDest)), key => BuildPlan(key.Source, key.Dest));
        for (var i = 0; i < plan.Length; i++)
        {
            plan[i](source!, dest!);
        }
    }

    /// <summary>
    /// One-time plan build for a (source, dest) type pair: walks public instance properties,
    /// applies the same Guid / collection / Nullable&lt;T&gt; rules as the original reflective
    /// version, and compiles a Get → Set lambda for each surviving pair.
    /// </summary>
    private static CopyStep[] BuildPlan(Type sourceType, Type destType)
    {
        var srcProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destProps = destType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name);

        var steps = new List<CopyStep>(srcProps.Length);
        foreach (var srcProp in srcProps)
        {
            if (!destProps.TryGetValue(srcProp.Name, out var destProp))
                continue;

            // Preserve identity Guid (mirrors original semantics).
            if (srcProp.Name == "Guid")
                continue;

            var srcPropertyType = srcProp.PropertyType;
            var destPropertyType = destProp.PropertyType;

            // Skip collection properties to avoid sharing mutable references. Strings excluded
            // because string implements IEnumerable but is value-like for copy purposes.
            if (typeof(IEnumerable).IsAssignableFrom(destPropertyType) && destPropertyType != typeof(string))
                continue;

            if (destPropertyType.IsAssignableFrom(srcPropertyType))
            {
                steps.Add(BuildDirectStep(sourceType, destType, srcProp, destProp));
                continue;
            }

            var srcUnderlying = Nullable.GetUnderlyingType(srcPropertyType);
            if (srcUnderlying != null && destPropertyType.IsAssignableFrom(srcUnderlying))
            {
                steps.Add(BuildNullableUnwrapStep(sourceType, destType, srcProp, destProp));
            }
        }

        return steps.ToArray();
    }

    /// <summary>
    /// Compiles: (object s, object d) => ((TDest)d).Prop = ((TSource)s).Prop;
    /// </summary>
    private static CopyStep BuildDirectStep(Type sourceType, Type destType, PropertyInfo srcProp, PropertyInfo destProp)
    {
        var srcParam = Expression.Parameter(typeof(object), "s");
        var destParam = Expression.Parameter(typeof(object), "d");

        var srcCast = Expression.Convert(srcParam, sourceType);
        var destCast = Expression.Convert(destParam, destType);

        var getter = Expression.Property(srcCast, srcProp);
        Expression valueExpr = getter;
        if (destProp.PropertyType != srcProp.PropertyType)
        {
            valueExpr = Expression.Convert(getter, destProp.PropertyType);
        }

        var assign = Expression.Assign(Expression.Property(destCast, destProp), valueExpr);
        return Expression.Lambda<CopyStep>(assign, srcParam, destParam).Compile();
    }

    /// <summary>
    /// Compiles: (s, d) => { var v = src.Prop; if (v.HasValue) dest.Prop = v.Value; }
    /// Preserves the original "leave dest at default when source is null" behaviour.
    /// </summary>
    private static CopyStep BuildNullableUnwrapStep(Type sourceType, Type destType, PropertyInfo srcProp, PropertyInfo destProp)
    {
        var srcParam = Expression.Parameter(typeof(object), "s");
        var destParam = Expression.Parameter(typeof(object), "d");

        var srcCast = Expression.Convert(srcParam, sourceType);
        var destCast = Expression.Convert(destParam, destType);

        var nullableValue = Expression.Property(srcCast, srcProp);
        var hasValue = Expression.Property(nullableValue, "HasValue");
        var value = Expression.Property(nullableValue, "Value");

        Expression valueExpr = value;
        if (destProp.PropertyType != value.Type)
        {
            valueExpr = Expression.Convert(value, destProp.PropertyType);
        }

        var assign = Expression.IfThen(hasValue,
            Expression.Assign(Expression.Property(destCast, destProp), valueExpr));

        return Expression.Lambda<CopyStep>(assign, srcParam, destParam).Compile();
    }
}
