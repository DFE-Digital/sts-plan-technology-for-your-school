namespace Dfe.PlanTech.Domain.Persistence.Interfaces;

public interface IRelationshipJoinTable<TConcrete> : IComparableDbEntity<TConcrete, long>
  where TConcrete : IRelationshipJoinTable<TConcrete>
{

}
