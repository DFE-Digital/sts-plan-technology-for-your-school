using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;
/// <summary>
/// Content for a 'RichText' field in Contentful
/// </summary>
/// <inheritdoc/>
public class RichTextContentDbEntity : IRichTextContent<RichTextMarkDbEntity, RichTextContentDbEntity, RichTextDataDbEntity>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Value { get; set; } = "";

    public string NodeType { get; set; } = "";

    public RichTextDataDbEntity? Data { get; set; }

    public List<RichTextMarkDbEntity> Marks { get; set; } = [];

    /// <summary>
    /// Children
    /// </summary>
    public List<RichTextContentDbEntity> Content { get; set; } = [];

    public RichTextContentDbEntity? Parent { get; set; }

    public long? ParentId { get; set; }

    public ComponentDropDownDbEntity? DropDown { get; set; }

}
