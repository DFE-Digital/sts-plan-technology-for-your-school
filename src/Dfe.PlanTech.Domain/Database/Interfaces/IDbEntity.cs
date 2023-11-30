namespace Dfe.PlanTech.Domain.Database.Interfaces;

public interface IDbEntity
{
  public long Id { get; }

  public string ContentfulId { get; }
}