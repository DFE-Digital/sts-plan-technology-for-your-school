using Dfe.PlanTech.Domain.Content.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Database table for the RichText section
/// </summary>
/// <inheritdoc/>
public class RichTextDataDbEntity : IRichTextData
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string? Uri { get; init; }
}
