namespace NewFace.Services;

public interface IMemoryManagementService
{
    Task InvalidateCache(string key);
    Task<T> GetOrSetCache<T>(string key, Func<Task<T>> getItemCallback, TimeSpan? expiration = null);
}
