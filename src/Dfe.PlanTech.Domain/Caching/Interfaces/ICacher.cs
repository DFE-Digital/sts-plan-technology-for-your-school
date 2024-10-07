namespace Dfe.PlanTech.Domain.Caching.Interfaces;

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
    public T? Get<T>(string key, Func<T> getFromService, TimeSpan timeToLive);

    /// <summary>
    /// Tries to get from Cache, if not found gets from service and sets cache to match. Uses default time to live from options.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="getFromService"></param>
    /// <param name="timeToLive"></param>
    /// <returns></returns>
    public T? Get<T>(string key, Func<T> getFromService);

    /// <summary>
    /// Tries to get from Cache only.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="getFromService"></param>
    /// <param name="timeToLive"></param>
    /// <returns></returns>
    public T? Get<T>(string key);

    /// <summary>
    /// Tries to get from Cache, if not found gets from service and sets cache to match.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="getFromService"></param>
    /// <param name="timeToLive"></param>
    /// <returns></returns>
    public Task<T?> GetAsync<T>(string key, Func<Task<T>> getFromService, TimeSpan timeToLive);

    /// <summary>
    /// Tries to get from Cache, if not found gets from service and sets cache to match. Uses default time to live from options.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="getFromService"></param>
    /// <param name="timeToLive"></param>
    /// <returns></returns>
    public Task<T?> GetAsync<T>(string key, Func<Task<T>> getFromService);

    /// <summary>
    /// Set value in cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="getFromService"></param>
    /// <param name="timeToLive"></param>
    /// <returns></returns>
    public void Set<T>(string key, TimeSpan timeToLive, T? value);

    /// <summary>
    /// Sets value in cache using default time to live options
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    public void Set<T>(string key, T? value);
}
