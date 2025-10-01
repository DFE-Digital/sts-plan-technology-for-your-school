using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces
{
    public interface IContentfulService
    {
        Task<IEnumerable<QuestionnaireSectionEntry>> GetAllSectionsAsync();
        Task<QuestionnaireCategoryEntry?> GetCategoryBySlugAsync(string slug, int? includeLevel = null);
        Task<string?> GetCategoryHeaderTextBySlugAsync(string slug);
        Task<NavigationLinkEntry> GetLinkByIdAsync(string contentId);
        Task<List<NavigationLinkEntry>> GetNavigationLinksAsync();
        Task<PageEntry> GetPageByIdAsync(string pageId);
        Task<PageEntry> GetPageBySlugAsync(string slug);
        Task<QuestionnaireQuestionEntry> GetQuestionByIdAsync(string questionId);
        Task<QuestionnaireSectionEntry> GetSectionBySlugAsync(string slug, int? includeLevel = null);
    }
}
