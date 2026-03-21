namespace Verendar.Common.Caching
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);

        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// Atomically sets the key only if it does not already exist. Returns true if the key was set.
        /// </summary>
        Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null);

        Task RemoveAsync(string key);
    }
}
