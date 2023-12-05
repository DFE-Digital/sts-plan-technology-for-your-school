using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Mark type for the rich text (e.g. bold, underline)
/// </summary>
public interface IRichTextMark
{
  public string Type { get; }

  public MarkType MarkType { get; }
}
