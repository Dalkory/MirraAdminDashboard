using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Api.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key);
            if (cachedData == null)
            {
                _logger.LogDebug("Cache miss for key {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data from cache for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options)
    {
        try
        {
            var serializedData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedData, options);
            _logger.LogDebug("Data cached for key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting data to cache for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
            _logger.LogDebug("Cache removed for key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing data from cache for key {Key}", key);
        }
    }
}