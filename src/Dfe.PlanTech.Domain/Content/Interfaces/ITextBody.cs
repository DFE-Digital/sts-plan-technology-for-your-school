namespace Dfe.PlanTech.Domain.Content.Interfaces;

/// <summary>
/// Model for TextBody content type
/// </summary>
public interface ITextBody
{
}

public interface ITextBody<out TContent> : ITextBody
where TContent : IRichTextContent
{
    public TContent? RichText { get; }
}
