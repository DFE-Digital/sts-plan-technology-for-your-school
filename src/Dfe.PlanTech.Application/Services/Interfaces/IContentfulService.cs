using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IContentfulService
{
    Task<IEnumerable<QuestionnaireSectionEntry>> GetAllSectionsAsync();

    Task<string?> GetCategoryHeaderTextBySlugAsync(string slug);

    Task<QuestionnaireCategoryEntry?> GetCategoryBySlugAsync(string slug, int? includeLevel = null);

    Task<NavigationLinkEntry> GetLinkByIdAsync(string contentId);

    Task<List<NavigationLinkEntry>> GetNavigationLinksAsync();

    Task<List<MicrocopyEntry>> GetMicrocopyEntriesAsync();

    Task<PageEntry> GetPageByIdAsync(string pageId);

    Task<PageEntry> GetPageBySlugAsync(string slug);

    Task<IEnumerable<RecommendationChunkEntry>> GetPaginatedRecommendationEntriesAsync(int page);

    Task<QuestionnaireQuestionEntry> GetQuestionByIdAsync(string questionId);

    Task<int> GetRecommendationChunkCountAsync(int page);

    Task<QuestionnaireSectionEntry> GetSectionBySlugAsync(string slug, int? includeLevel = null);
}
