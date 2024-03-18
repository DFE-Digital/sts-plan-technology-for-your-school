namespace Dfe.PlanTech.Domain.Persistence.Interfaces;

public interface IComparableDbEntity<TConcrete, TId> : IHasId<TId>
where TConcrete : IComparableDbEntity<TConcrete, TId>
where TId : IComparable, IComparable<TId>, IEquatable<TId>
{
  public bool Matches(TConcrete other);
}