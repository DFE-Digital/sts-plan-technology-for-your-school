namespace Dfe.PlanTech.Domain.Caching.Models;

public class LockOwner
{
    public string Id { get; private set; }
    public LockOwner() => Id = Guid.NewGuid().ToString("N");
    public override string ToString() => Id;
    public static LockOwner Create() => new LockOwner();
}
