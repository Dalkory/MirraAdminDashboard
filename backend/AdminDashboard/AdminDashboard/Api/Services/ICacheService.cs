using Microsoft.Extensions.Caching.Distributed;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options);
    Task RemoveAsync(string key);
}