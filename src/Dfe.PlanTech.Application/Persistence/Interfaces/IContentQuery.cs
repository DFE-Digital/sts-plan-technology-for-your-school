namespace Dfe.PlanTech.Application.Persistence.Interfaces;

/// <summary>
/// Something that is used for filtering content query results by field
/// </summary>
public interface IContentQuery
{
    public string Field { get; }
}
