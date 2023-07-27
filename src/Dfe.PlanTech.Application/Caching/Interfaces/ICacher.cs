namespace Dfe.PlanTech.Application.Caching.Interfaces;

/// <summary>
/// Get/set values in cache.
/// </summary>
public interface ICacher
{
    /// <summary>
    /// Tries to get from Cache, if not found gets from service and sets cache to match
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="getFromService"></param>
    /// <param name="timeToLive"></param>
    /// <returns></returns>
    public Task<T?> GetAsync<T>(string key, Func<Task<T>> getFromService, TimeSpan? timeToLive = null);

    /// Tries to get from Cache, if not found gets from service and sets cache to match
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="getFromService"></param>
    /// <param name="timeToLive"></param>
    /// <returns></returns>
    public Task<T?> GetAsync<T>(string key, Func<T> getFromService, TimeSpan? timeToLive = null);

    /// Tries to get from Cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="timeToLive"></param>
    /// <returns></returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Set value in cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value">Value to set (if null, removes from cache)</param>
    /// <param name="timeToLive"></param>
    /// <returns></returns>
    public Task SetAsync<T>(string key, T? value, TimeSpan? timeToLive = null);
}
