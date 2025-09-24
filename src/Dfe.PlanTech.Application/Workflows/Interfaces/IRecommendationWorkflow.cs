using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Workflows.Interfaces
{
    public interface IRecommendationWorkflow
    {
        Task<IEnumerable<SqlRecommendationDto>> GetRecommendationsByContentfulReferencesAsync(IEnumerable<string> contentfulSysIds);
    }
}
