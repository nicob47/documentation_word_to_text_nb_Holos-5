namespace H.Core.Mappers;

/// <summary>
/// Generic mapper interface for mapping from TSource to TDest.
/// Replaces AutoMapper's IMapper with strongly-typed, DI-friendly contracts.
/// </summary>
public interface IModelMapper<in TSource, out TDest>
{
    TDest Map(TSource source);
}
