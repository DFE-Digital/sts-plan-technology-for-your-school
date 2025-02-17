using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Models;

public class CustomData : IRichTextData
{
    public string? Uri { get; init; }
    //Needs to be called Target to align with Contentful
    public RichTextContentData Target { get; init; }
}
