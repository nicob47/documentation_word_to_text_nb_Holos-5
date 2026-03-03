using Microsoft.Extensions.Caching.Memory;

namespace H.Infrastructure.Services;

public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null);
    void Set<T>(string key, T value, MemoryCacheEntryOptions? options);
    void Remove(string key);
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpirationRelativeToNow = null);
}