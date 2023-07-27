

namespace Dfe.PlanTech.Domain.Caching.Models;

public class CachedItem<T>
{
    public T? Item { get; init; }

    public CachedItem() {}

    public CachedItem(T? item){
        Item = item;
    }
}