using Microsoft.Extensions.Caching.Memory;

namespace Dotnet8LineBotLab.Services;

public class CacheService
{
    private readonly IMemoryCache _memoryCache;

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    // 儲存資料到快取
    public void SetCache(string key, string value, int slidingExpiration = 30)
    {
        var cacheExpirationOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(slidingExpiration)
        };

        _memoryCache.Set(key, value, cacheExpirationOptions);
    }

    // 讀取快取資料
    public string GetCache(string key)
    {
        if (_memoryCache.TryGetValue(key, out var value))
        {
            return value.ToString();
        }
        else
        {
            return string.Empty;
        };
    }

    // 清除快取資料
    public void RemoveCache(string key)
    {
        _memoryCache.Remove(key);
    }
}