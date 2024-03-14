using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data.Repositories;

public class RecommendationsRepository(ICmsDbContext db) : IRecommendationsRepository
{
  private readonly ICmsDbContext _db = db;

  public Task<SubtopicRecommendationDbEntity?> GetCompleteRecommendationsForSubtopic(string subtopicId, CancellationToken cancellationToken)
  => _db.SubtopicRecommendations.Include(rec => rec.Intros)
                                                            .ThenInclude(intro => intro.Content)
                                                            .Include(rec => rec.Section)
                                                            .ThenInclude(section => section.Chunks)
                                                            .ThenInclude(chunk => chunk.Content)
                                                            .FirstOrDefaultAsync(subtopicRecommendation => subtopicRecommendation.SubtopicId == subtopicId, cancellationToken: cancellationToken);

  public Task<RecommendationsViewDto?> GetRecommenationsViewDtoForSubtopicAndMaturity(string subtopicId, string maturity, CancellationToken cancellationToken)
  => _db.SubtopicRecommendations.Where(subtopicRecommendation => subtopicRecommendation.SubtopicId == subtopicId)
                                                          .Select(subtopicRecommendation => subtopicRecommendation.Intros.FirstOrDefault(intro => intro.Maturity == maturity))
                                                          .Select(intro => new RecommendationsViewDto(intro!.Slug, intro.Header.Text))
                                                          .FirstOrDefaultAsync(cancellationToken: cancellationToken);
}