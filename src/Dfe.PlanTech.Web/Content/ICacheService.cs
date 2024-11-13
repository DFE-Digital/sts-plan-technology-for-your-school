namespace Dfe.PlanTech.Web.Content;

public interface ICacheService<T>
{
    void AddToCache(string key, T item);
    T? GetFromCache(string key);
    void ClearCache();
}
