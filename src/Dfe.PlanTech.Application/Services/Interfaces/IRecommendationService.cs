using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<IEnumerable<SqlRecommendationDto>> UpsertRecommendations(IEnumerable<RecommendationModel> recommendationModels);
    }
}
