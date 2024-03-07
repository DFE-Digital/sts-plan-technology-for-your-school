using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class ContentComponentDbEntity : IContentComponentDbEntity
{
    public string Id { get; set; } = null!;

    public bool Published { get; set; }

    public bool Archived { get; set; }

    public bool Deleted { get; set; }

    [DontCopyValue]
    public long? Order { get; set; }

    public List<PageDbEntity> BeforeTitleContentPages { get; set; } = [];

    /// <summary>
    /// Joins for <see cref="BeforeTitleContentPages"/> 
    /// </summary>
    public List<PageContentDbEntity> BeforeTitleContentPagesJoins { get; set; } = [];

    public List<PageDbEntity> ContentPages { get; set; } = [];

    /// <summary>
    /// Joins for <see cref="ContentPages"/> 
    /// </summary>
    public List<PageContentDbEntity> ContentPagesJoins { get; set; } = [];

}