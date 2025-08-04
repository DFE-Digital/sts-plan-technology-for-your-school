using System.ComponentModel.DataAnnotations.Schema;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsRecommendationDto : CmsEntryDto
{
    public string Slug { get; set; } = null!;

    public string Maturity { get; set; } = null!;

    [NotMapped]
    public List<CmsEntryDto> Content { get; set; } = [];

    public string HeaderText { get; set; } = null!;

    public string LinkText { get; set; } = null!;

    public string SlugifiedLinkText { get; set; } = null!;
}
