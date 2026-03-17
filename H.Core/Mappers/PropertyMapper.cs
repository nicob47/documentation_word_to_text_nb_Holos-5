using System.Collections;
using System.Reflection;

namespace H.Core.Mappers;

/// <summary>
/// Convention-based property mapper that copies public properties from source to destination
/// where property names and types match. Replaces AutoMapper's default behavior.
/// </summary>
public static class PropertyMapper
{
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
        var srcProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destProps = typeof(TDest).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name);

        foreach (var srcProp in srcProps)
        {
            if (!destProps.TryGetValue(srcProp.Name, out var destProp))
                continue;

            var srcType = srcProp.PropertyType;
            var destType = destProp.PropertyType;

            // Skip the identity Guid property to preserve object identity.
            // Other Guid-typed properties (e.g. FieldSystemComponentGuid, DomainObjectGuid) are copied normally.
            if (srcProp.Name == "Guid")
                continue;

            // Skip collection properties to avoid sharing mutable collection references.
            // String implements IEnumerable but should be copied normally.
            if (typeof(IEnumerable).IsAssignableFrom(destType) && destType != typeof(string))
                continue;

            // Direct assignability (same type, or base/interface assignment)
            if (destType.IsAssignableFrom(srcType))
            {
                destProp.SetValue(dest, srcProp.GetValue(source));
                continue;
            }

            // Handle Nullable<T> source -> T destination
            var srcUnderlying = Nullable.GetUnderlyingType(srcType);
            if (srcUnderlying != null && destType.IsAssignableFrom(srcUnderlying))
            {
                var value = srcProp.GetValue(source);
                if (value != null)
                {
                    destProp.SetValue(dest, value);
                }
                // If null, leave dest at its default value
            }
        }
    }
}
