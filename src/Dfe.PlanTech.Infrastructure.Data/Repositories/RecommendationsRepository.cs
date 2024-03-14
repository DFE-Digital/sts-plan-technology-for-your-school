using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data.Repositories;

public class RecommendationsRepository(ICmsDbContext db) : IRecommendationsRepository
{
  private readonly ICmsDbContext _db = db;

  public async Task<SubtopicRecommendationDbEntity?> GetRecommendationsForSubtopic(string subtopicId, CancellationToken cancellationToken)
  {
    try
    {
      var recommendation = await _db.SubtopicRecommendations.Include(rec => rec.Intros)
                                                            .ThenInclude(intro => intro.Content)
                                                            .Include(rec => rec.Section)
                                                            .FirstOrDefaultAsync(subtopicRecommendation => subtopicRecommendation.SubtopicId == subtopicId, cancellationToken: cancellationToken);

      if (recommendation == null)
      {
        return null;
      }

      return recommendation;
    }
    catch (Exception ex)
    {
      //_logger.LogError(ex, "Error retrieving recommendation for {Subtopic} from DB", subtopicId);
      throw;
    }
  }

}