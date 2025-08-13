using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsQuestionnaireCategoryDto : CmsEntryDto
{
    public string Id { get; set; }
    public string InternalName { get; set; } = "";
    public CmsComponentHeaderDto Header { get; set; } = null!;
    public List<CmsEntryDto>? Content { get; set; } = null!;
    public CmsPageDto? LandingPage { get; set; } = null!;
    public List<CmsQuestionnaireSectionDto> Sections { get; set; } = [];

    public CmsQuestionnaireCategoryDto(QuestionnaireCategoryEntry categoryEntry)
    {
        Id = categoryEntry.Id;
        InternalName = categoryEntry.InternalName;
        Header = categoryEntry.Header.AsDto();
        Content = categoryEntry.Content?.Select(BuildContentDto).ToList();
        LandingPage = categoryEntry.LandingPage?.AsDto();
        Sections = categoryEntry.Sections.Select(s => s.AsDto()).ToList();
    }
}
