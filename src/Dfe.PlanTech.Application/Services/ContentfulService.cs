using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

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

    public Task<CmsNavigationLinkDto> GetLinkByIdAsync(string contentId)
    {
        return _contentfulWorkflow.GetEntryById<NavigationLinkEntry, CmsNavigationLinkDto>(contentId);
    }

    public Task<List<CmsNavigationLinkDto>> GetNavigationLinks()
    {
        return contentfulWorkflow.GetEntries<NavigationLinkEntry, CmsNavigationLinkDto>();
    }

    public Task<CmsPageDto> GetPageByIdAsync(string pageId)
    {
        return _contentfulWorkflow.GetEntryById<PageEntry, CmsPageDto>(pageId);
    }

    public Task<CmsPageDto> GetPageBySlugAsync(string slug)
    {
        return _contentfulWorkflow.GetPageBySlugAsync(slug);
    }

    public Task<CmsQuestionnaireQuestionDto> GetQuestionByIdAsync(string questionId)
    {
        return _contentfulWorkflow.GetEntryById<QuestionnaireQuestionEntry, CmsQuestionnaireQuestionDto>(questionId);
    }

    public Task<CmsQuestionnaireSectionDto> GetSectionBySlugAsync(string slug)
    {
        return _contentfulWorkflow.GetSectionBySlugAsync(slug);
    }

    public Task<CmsSubtopicRecommendationDto?> GetSubtopicRecommendationByIdAsync(string subtopicId)
    {
        return _contentfulWorkflow.GetSubtopicRecommendationByIdAsync(subtopicId);
    }

    public Task<CmsRecommendationIntroDto?> GetSubtopicRecommendationIntroAsync(string subtopicId, string maturity)
    {
        return _contentfulWorkflow.GetSubtopicRecommendationIntroByIdAndMaturityAsync(subtopicId, maturity);
    }
}
