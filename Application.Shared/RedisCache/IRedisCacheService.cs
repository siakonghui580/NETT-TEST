namespace Application.Shared.RedisCache
{
    public interface IRedisCacheService
    {
        #region C
        Task SetAsync<T>(string key, T? valueToCache, TimeSpan? expiry = null);
        #endregion

        #region R
        Task<T?> GetAsync<T>(string key);
        #endregion

        #region D
        Task DeleteAsync(string key);
        #endregion
    }
}
