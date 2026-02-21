using System.Collections.Concurrent;
using System.Text.Json;
using LibraryManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace LibraryManagement.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, byte> _cacheKeys = new();

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = _cache.Get<string>(key);
        if (value is null) return Task.FromResult<T?>(default);

        var result = JsonSerializer.Deserialize<T>(value);
        return Task.FromResult(result);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var serialized = JsonSerializer.Serialize(value);
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };

        _cache.Set(key, serialized, options);
        _cacheKeys.TryAdd(key, 0);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        _cacheKeys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        var keysToRemove = _cacheKeys.Keys.Where(k => k.StartsWith(prefixKey)).ToList();
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
        }
        return Task.CompletedTask;
    }
}
