namespace Dfe.PlanTech.Domain.Persistence.Interfaces;

/// <summary>
/// Something that is used for filtering content query results by field
/// </summary>
public interface IContentQuery
{
    public string Field { get; }
}
