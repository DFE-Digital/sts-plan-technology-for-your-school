using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Services;

public class ContentfulService(
    IContentfulWorkflow contentfulWorkflow
) : IContentfulService
{
    private readonly IContentfulWorkflow _contentfulWorkflow = contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));

    public Task<IEnumerable<QuestionnaireSectionEntry>> GetAllSectionsAsync()
    {
        return _contentfulWorkflow.GetAllSectionsAsync();
    }

    public Task<string?> GetCategoryHeaderTextBySlugAsync(string slug)
    {
        return _contentfulWorkflow.GetCategoryHeaderTextBySlugAsync(slug);
    }

    public Task<QuestionnaireCategoryEntry?> GetCategoryBySlugAsync(string slug, int? includeLevel = null)
    {
        return _contentfulWorkflow.GetCategoryBySlugAsync(slug, includeLevel);
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

    public Task<QuestionnaireSectionEntry> GetSectionBySlugAsync(string slug, int? includeLevel = null)
    {
        return _contentfulWorkflow.GetSectionBySlugAsync(slug, includeLevel);
    }

    public Task<SubtopicRecommendationEntry?> GetSubtopicRecommendationByIdAsync(string subtopicId, int? includeLevel = null)
    {
        return _contentfulWorkflow.GetSubtopicRecommendationByIdAsync(subtopicId, includeLevel);
    }

    public Task<RecommendationIntroEntry?> GetSubtopicRecommendationIntroAsync(string subtopicId, string maturity)
    {
        return _contentfulWorkflow.GetSubtopicRecommendationIntroByIdAndMaturityAsync(subtopicId, maturity);
    }
}
