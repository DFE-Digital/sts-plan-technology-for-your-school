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

    [ForeignKey("RichTextContentId")]
    /// <summary>
    /// The Content to display.
    /// </summary>
    public RichTextContentDbEntity Content { get; set; } = null!;

    public long RichTextContentId { get; set; }
}
