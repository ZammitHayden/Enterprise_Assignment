using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Enterprise_Assignment.Data.Repositories
{
    public class ItemsInMemoryRepository : IItemsRepository
    {
        private readonly IMemoryCache _memoryCache;
        private const string CacheKeyPrefix = "ImportItems_";

        public ItemsInMemoryRepository(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public List<IItemValidating> GetItems(string sessionId)
        {
            var cacheKey = $"{CacheKeyPrefix}{sessionId}";
            if (_memoryCache.TryGetValue(cacheKey, out List<IItemValidating> items))
            {
                return items;
            }
            return new List<IItemValidating>();
        }

        public void SaveItems(string sessionId, List<IItemValidating> items)
        {
            var cacheKey = $"{CacheKeyPrefix}{sessionId}";
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30));

            _memoryCache.Set(cacheKey, items, cacheOptions);
        }

        public void ClearItems(string sessionId)
        {
            var cacheKey = $"{CacheKeyPrefix}{sessionId}";
            _memoryCache.Remove(cacheKey);
        }
    }
}