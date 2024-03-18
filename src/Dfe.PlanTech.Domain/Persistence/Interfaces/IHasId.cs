namespace Dfe.PlanTech.Domain.Persistence.Interfaces;

public interface IHasId<TId>
where TId : IComparable, IComparable<TId>, IEquatable<TId>
{
  public TId Id { get; set; }
}