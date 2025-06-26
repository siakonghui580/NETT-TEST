using Application.Shared.RedisCache;
using StackExchange.Redis;
using System.Text.Json;

namespace Application.RedisCache
{
    public class RedisCacheService : IRedisCacheService
    {
        #region Declarations
        private readonly IDatabase _redisDB;
        #endregion

        #region Constructor
        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redisDB = redis.GetDatabase();
        }
        #endregion

        #region Methods

        #region C
        public async Task SetAsync<T>(string key, T? valueToCache, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(valueToCache);
            await _redisDB.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(10));
        }
        #endregion

        #region R
        public async Task<T?> GetAsync<T>(string key)
        {
            var cached = await _redisDB.StringGetAsync(key);

            if (!cached.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(cached);
        }
        #endregion

        #region D
        public Task DeleteAsync(string key)
        {
            return _redisDB.KeyDeleteAsync(key);
        }
        #endregion

        #endregion
    }
}
