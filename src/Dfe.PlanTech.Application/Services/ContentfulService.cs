using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Exceptions;
using static System.Collections.Specialized.BitVector32;

namespace Dfe.PlanTech.Application.Services;

public class ContentfulService(
    ContentfulWorkflow contentfulWorkflow
)
{
    private readonly ContentfulWorkflow _contentfulWorkflow = contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));

    public Task<IEnumerable<CmsQuestionnaireSectionDto>> GetAllSectionsAsync()
    {
        return _contentfulWorkflow.GetAllSectionsAsync();
    }

    public Task<CmsNavigationLinkDto?> GetLinkByIdAsync(string contentId)
    {
        return _contentfulWorkflow.GetEntryById<NavigationLinkEntry, CmsNavigationLinkDto>(contentId);
    }

    public Task<CmsPageDto> GetPageBySlugAsync(string slug)
    {
        return _contentfulWorkflow.GetPageBySlugAsync(slug)
            ?? throw new ContentfulDataUnavailableException($"Could not find page for slug: {slug}");
    }

    public Task<CmsQuestionnaireSectionDto> GetSectionBySlugAsync(string slug)
    {
        return _contentfulWorkflow.GetSectionBySlugAsync(slug)
            ?? throw new ContentfulDataUnavailableException($"Could not find section for slug: {slug}");
    }

    public Task<CmsSubtopicRecommendationDto?> GetSubTopicRecommendation(string subtopicId)
    {
        return _contentfulWorkflow.GetSubTopicRecommendation(subtopicId)
            ?? throw new ContentfulDataUnavailableException($"Could not find subtopic recommendation for subtopic with ID {subtopicId}");
    }
}
