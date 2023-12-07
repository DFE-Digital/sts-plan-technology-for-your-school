using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

/// <summary>
/// Database table for the RichText section
/// </summary>
/// <inheritdoc/>
public class RichTextDataDbEntity : IRichTextData
{
    public string? Uri { get; init; }
}
