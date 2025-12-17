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

    public Task<IEnumerable<RecommendationChunkEntry>> GetPaginatedRecommendationEntriesAsync(int page)
    {
        return _contentfulWorkflow.GetPaginatedRecommendationEntriesAsync(page);
    }

    public Task<QuestionnaireQuestionEntry> GetQuestionByIdAsync(string questionId)
    {
        return _contentfulWorkflow.GetEntryById<QuestionnaireQuestionEntry>(questionId);
    }

    public Task<int> GetRecommendationChunkCountAsync(int page)
    {
        return _contentfulWorkflow.GetRecommendationChunkCountAsync(page);
    }

    public Task<QuestionnaireSectionEntry> GetSectionBySlugAsync(string slug, int? includeLevel = null)
    {
        return _contentfulWorkflow.GetSectionBySlugAsync(slug, includeLevel);
    }
}
