using Microsoft.Extensions.Caching.Memory;

namespace H.Infrastructure.Services;

public class InMemoryCacheService : ICacheService
{
    #region Fields

    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _defaultExpiration;

    #endregion

    #region Constructors

    public InMemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _defaultExpiration = TimeSpan.FromMinutes(60); // Default expiration
    }

    #endregion

    #region Public Methods
    
    public T? Get<T>(string key)
    {
        return _memoryCache.Get<T>(key);
    }

    public void Set<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)
    {
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(absoluteExpirationRelativeToNow ?? _defaultExpiration);

        _memoryCache.Set(key, value, options);
    }

    public void Set<T>(string key, T value, MemoryCacheEntryOptions? options)
    {
        _memoryCache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpirationRelativeToNow = null)
    {
        return await _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? _defaultExpiration;
            return await factory();
        });
    } 

    #endregion
}