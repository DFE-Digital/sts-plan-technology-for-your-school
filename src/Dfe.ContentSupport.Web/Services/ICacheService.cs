namespace Dfe.ContentSupport.Web.Services;

public interface ICacheService<T>
{
    void AddToCache(string key, T item);
    T? GetFromCache(string key);
    void ClearCache();
}