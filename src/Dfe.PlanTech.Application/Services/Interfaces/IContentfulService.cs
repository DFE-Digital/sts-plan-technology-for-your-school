using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IContentfulService
{
    Task<QuestionnaireCategoryEntry?> GetCategoryBySlugAsync(string slug, int? includeLevel = null);
    Task<string?> GetCategoryHeaderTextBySlugAsync(string slug);
    Task<IEnumerable<QuestionnaireSectionEntry>> GetAllSectionsAsync();
    Task<QuestionnaireSectionEntry> GetSectionBySlugAsync(string slug, int? includeLevel = null);
    Task<NavigationLinkEntry> GetLinkByIdAsync(string contentId);
    Task<List<MicrocopyEntry>> GetMicrocopyEntriesAsync();
    Task<List<NavigationLinkEntry>> GetNavigationLinksAsync();
    Task<PageEntry> GetPageByIdAsync(string pageId);
    Task<PageEntry> GetPageBySlugAsync(string slug);
    Task<QuestionnaireQuestionEntry> GetQuestionByIdAsync(string questionId);
    Task<int> GetRecommendationChunkCountAsync(int page);
    Task<IEnumerable<RecommendationChunkEntry>> GetPaginatedRecommendationEntriesAsync(int page);
    Task<ComponentTextBodyEntry> GetTextBodyByIdAsync(string id);
}
