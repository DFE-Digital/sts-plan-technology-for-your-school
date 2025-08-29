using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces
{
    public interface ISubtopicRecommendationWorkflow
    {
        Task<SubtopicRecommendationEntry?> GetFirstSubtopicRecommendationAsync(string subtopicId);
        Task<RecommendationIntroEntry?> GetIntroForMaturityAsync(string subtopicId, string maturity);
    }
}
