using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IHasRichText
{
    public RichTextContentDbEntity? RichText { get; set; }
    public long? RichTextId { get; }
}
