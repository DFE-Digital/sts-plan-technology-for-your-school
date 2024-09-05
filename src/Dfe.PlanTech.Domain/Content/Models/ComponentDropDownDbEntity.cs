using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Model for DropDown type.
/// </summary>
public class ComponentDropDownDbEntity : ContentComponentDbEntity, IComponentDropDown<RichTextContentDbEntity>
{
    /// <summary>
    /// The title to display.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// The Content to display.
    /// </summary>
    [ForeignKey("RichTextContentId")]
    public RichTextContentDbEntity? Content { get; set; }

    public long? RichTextContentId { get; set; }
}
