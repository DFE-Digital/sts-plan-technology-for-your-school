namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface IGetEntitiesOptions
{
    IEnumerable<IContentQuery>? Queries { get; init; }
    int Include { get; init; }
}
