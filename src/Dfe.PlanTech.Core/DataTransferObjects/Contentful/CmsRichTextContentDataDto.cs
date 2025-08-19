using Contentful.Core.Models;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsRichTextContentDataDto : CmsEntryDto, IHasUri
{
    public string Id { get; set; } = null!;
    public string InternalName { get; init; } = null!;
    public string Slug { get; init; } = null!;
    public string? Title { get; init; }
    public Asset? Asset { get; init; }
    public IReadOnlyList<CmsRichTextContentDataDto> Content { get; init; } = [];
    public string SummaryLine { get; init; } = null!;
    public string? Uri { get; init; } = null!;
    public CmsRichTextContentDto? RichText { get; init; } = null!;

    public CmsRichTextContentDataDto(RichTextContentDataEntry richTextField)
    {
        Id = richTextField.Id;
        InternalName = richTextField.InternalName;
        Slug = richTextField.Slug;
        Title = richTextField.Title;
        Asset = richTextField.Asset;
        Content = richTextField.Content.Select(c => c.AsDto()).ToList();
        SummaryLine = richTextField.SummaryLine;
        Uri = richTextField.Uri;
        RichText = richTextField.RichText?.AsDto();
    }
}
