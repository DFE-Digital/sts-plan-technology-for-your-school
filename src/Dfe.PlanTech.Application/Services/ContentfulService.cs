using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Services;

public class ContentfulService(IContentfulWorkflow contentfulWorkflow) : IContentfulService
{
    private readonly IContentfulWorkflow _contentfulWorkflow =
        contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));

    public Task<QuestionnaireCategoryEntry?> GetCategoryBySlugAsync(
        string slug,
        int? includeLevel = null
    )
    {
        return _contentfulWorkflow.GetCategoryBySlugAsync(slug, includeLevel);
    }

    public Task<IEnumerable<QuestionnaireSectionEntry>> GetAllSectionsAsync()
    {
        return _contentfulWorkflow.GetAllSectionsAsync();
    }

    public Task<IEnumerable<QuestionnaireCategoryEntry>> GetAllCategoriesAsync()
    {
        return _contentfulWorkflow.GetAllCategoriesAsync();
    }

    public Task<QuestionnaireSectionEntry> GetSectionBySlugAsync(
        string slug,
        int? includeLevel = null
    )
    {
        return _contentfulWorkflow.GetSectionBySlugAsync(slug, includeLevel);
    }

    public Task<NavigationLinkEntry> GetLinkByIdAsync(string contentId)
    {
        return _contentfulWorkflow.GetEntryByIdAsync<NavigationLinkEntry>(contentId);
    }

    public Task<List<MicrocopyEntry>> GetMicrocopyEntriesAsync()
    {
        return contentfulWorkflow.GetEntriesAsync<MicrocopyEntry>();
    }

    public Task<List<NavigationLinkEntry>> GetNavigationLinksAsync()
    {
        return contentfulWorkflow.GetEntriesAsync<NavigationLinkEntry>();
    }

    public Task<PageEntry> GetPageByIdAsync(string pageId)
    {
        return _contentfulWorkflow.GetEntryByIdAsync<PageEntry>(pageId);
    }

    public Task<PageEntry> GetPageBySlugAsync(string slug)
    {
        return _contentfulWorkflow.GetPageBySlugAsync(slug);
    }

    public Task<QuestionnaireQuestionEntry> GetQuestionByIdAsync(string questionId)
    {
        return _contentfulWorkflow.GetEntryByIdAsync<QuestionnaireQuestionEntry>(questionId);
    }

    public Task<IEnumerable<RecommendationChunkEntry>> GetPaginatedRecommendationEntriesAsync(
        int page
    )
    {
        return _contentfulWorkflow.GetPaginatedRecommendationEntriesAsync(page);
    }

    public Task<int> GetRecommendationChunkCountAsync(int page)
    {
        return _contentfulWorkflow.GetRecommendationChunkCountAsync();
    }

    public Task<List<RedirectEntry>> GetRedirectsAsync()
    {
        return contentfulWorkflow.GetEntriesAsync<RedirectEntry>();
    }

    public Task<ComponentTextBodyEntry> GetTextBodyByIdAsync(string id)
    {
        return _contentfulWorkflow.GetEntryByIdAsync<ComponentTextBodyEntry>(id);
    }
}
