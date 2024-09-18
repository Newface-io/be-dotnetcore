using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace NewFace.Services;

public class MemoryManagementService : IMemoryManagementService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<MemoryManagementService> _logger;

    public MemoryManagementService(IDistributedCache distributedCache, ILogger<MemoryManagementService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T> GetOrSetCache<T>(string key, Func<Task<T>> getItemCallback, TimeSpan? expiration = null)
    {
        var cachedValue = await _distributedCache.GetStringAsync(key);

        if (cachedValue != null)
        {
            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        var item = await getItemCallback();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
        };

        await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(item), options);

        return item;
    }

    // ActorImage.IsMianImage 가 바뀔때마다 호출해서 cache를 지운다.
    public async Task InvalidateCache(string key)
    {
        await _distributedCache.RemoveAsync(key);
    }
}