using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces
{
    public interface IContentfulWorkflow
    {
        Task<IEnumerable<QuestionnaireSectionEntry>> GetAllSectionsAsync();
        Task<QuestionnaireCategoryEntry?> GetCategoryBySlugAsync(string slug, int? includeLevel = null);
        Task<string?> GetCategoryHeaderTextBySlugAsync(string slug);
        Task<List<TEntry>> GetEntries<TEntry>() where TEntry : ContentfulEntry;
        Task<TEntry> GetEntryById<TEntry>(string entryId) where TEntry : ContentfulEntry;
        Task<RecommendationIntroEntry?> GetIntroForMaturityAsync(string subtopicId, string maturity);
        Task<PageEntry> GetPageBySlugAsync(string slug);
        Task<QuestionnaireSectionEntry> GetSectionBySlugAsync(string sectionSlug, int? includeLevel = null);
        Task<SubtopicRecommendationEntry?> GetSubtopicRecommendationByIdAsync(string subtopicId, int? includeLevel = null);
        Task<RecommendationIntroEntry?> GetSubtopicRecommendationIntroByIdAndMaturityAsync(string subtopicId, string maturity);
    }
}
