namespace Dfe.PlanTech.Application.Caching.Interfaces;

public interface ICacher
{
    public T? Get<T>(string key, Func<T> getFromService, TimeSpan timeToLive);

    public T? Get<T>(string key, Func<T> getFromService);

    public T? Get<T>(string key);

    public Task<T?> GetAsync<T>(string key, Func<Task<T>> getFromService, TimeSpan timeToLive);
    
    public Task<T?> GetAsync<T>(string key, Func<Task<T>> getFromService);
    
    public void Set<T>(string key, TimeSpan timeToLive, T? value);
}
