using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IHasRichText
{
    public RichTextContentDbEntity RichText { get; }
    public long RichTextId { get; }
}