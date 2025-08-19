using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Services;

public class ContentfulService(
    ContentfulWorkflow contentfulWorkflow
)
{
    private readonly ContentfulWorkflow _contentfulWorkflow = contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));

    public Task<IEnumerable<QuestionnaireSectionEntry>> GetAllSectionsAsync()
    {
        return _contentfulWorkflow.GetAllSectionsAsync();
    }

    public Task<QuestionnaireCategoryEntry?> GetCategoryBySlugAsync(string slug)
    {
        return _contentfulWorkflow.GetCategoryBySlugAsync(slug);
    }

    public Task<NavigationLinkEntry> GetLinkByIdAsync(string contentId)
    {
        return _contentfulWorkflow.GetEntryById<NavigationLinkEntry>(contentId);
    }

    public Task<List<NavigationLinkEntry>> GetNavigationLinksAsync()
    {
        return contentfulWorkflow.GetEntries<NavigationLinkEntry>();
    }

    public Task<PageEntry> GetPageByIdAsync(string pageId)
    {
        return _contentfulWorkflow.GetEntryById<PageEntry>(pageId);
    }

    public Task<PageEntry> GetPageBySlugAsync(string slug)
    {
        return _contentfulWorkflow.GetPageBySlugAsync(slug);
    }

    public Task<QuestionnaireQuestionEntry> GetQuestionByIdAsync(string questionId)
    {
        return _contentfulWorkflow.GetEntryById<QuestionnaireQuestionEntry>(questionId);
    }

    public Task<QuestionnaireSectionEntry> GetSectionBySlugAsync(string slug)
    {
        return _contentfulWorkflow.GetSectionBySlugAsync(slug);
    }

    public Task<SubtopicRecommendationEntry?> GetSubtopicRecommendationByIdAsync(string subtopicId)
    {
        return _contentfulWorkflow.GetSubtopicRecommendationByIdAsync(subtopicId);
    }

    public Task<RecommendationIntroEntry?> GetSubtopicRecommendationIntroAsync(string subtopicId, string maturity)
    {
        return _contentfulWorkflow.GetSubtopicRecommendationIntroByIdAndMaturityAsync(subtopicId, maturity);
    }
}
