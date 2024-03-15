using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data.Repositories;

public class RecommendationsRepository(ICmsDbContext db) : IRecommendationsRepository
{
  private readonly ICmsDbContext _db = db;

  public async Task<SubtopicRecommendationDbEntity?> GetCompleteRecommendationsForSubtopic(string subtopicId, CancellationToken cancellationToken)
  {
    var recommendation = await _db.Sections.Where(section => section.Id == subtopicId)
                                          .Select(section => new
                                          {
                                            name = section.Name,
                                            recommendation = section.SubtopicRecommendation
                                          })
                                          .Select(recWithName => new SubtopicRecommendationDbEntity()
                                          {
                                            Subtopic = new SectionDbEntity()
                                            {
                                              Name = recWithName.name
                                            },
                                            Section = new RecommendationSectionDbEntity()
                                            {
                                              Id = recWithName.recommendation!.Section.Id,
                                              Answers = recWithName.recommendation!.Section.Answers.Select(answer => new AnswerDbEntity() { Id = answer.Id }).ToList(),
                                            },
                                            SectionId = recWithName.recommendation!.SectionId,
                                            Id = recWithName.recommendation.Id
                                          })
                                          .FirstOrDefaultAsync(cancellationToken);

    //var recommendation = await _db.SubtopicRecommendations.Include(subtopicRec => subtopicRec.Section).FirstOrDefaultAsync(subtopicRec => subtopicRec.SubtopicId == subtopicId, cancellationToken);

    if (recommendation == null)
    {
      return null;
    }

    var intros = await _db.RecommendationIntros.Where(intro => intro.SubtopicRecommendations.Any(subtopicRec => subtopicRec.Id == recommendation.Id))
                                              .Include(intro => intro.Header)
                                              .ToListAsync(cancellationToken);

    var chunks = await _db.RecommendationChunks.Where(chunk => chunk.RecommendationSections.Any(section => section.Id == recommendation.SectionId))
                                                .Select(chunk => new RecommendationChunkDbEntity()
                                                {
                                                  Header = new HeaderDbEntity() { Text = chunk.Header.Text, Size = chunk.Header.Size, Tag = chunk.Header.Tag },
                                                  Title = chunk.Title,
                                                  Answers = chunk.Answers.Select(answer => new AnswerDbEntity() { Id = answer.Id }).ToList(),
                                                  Id = chunk.Id,
                                                })
                                                .ToListAsync(cancellationToken);

    var contents = await _db.ContentComponents.Where(cc => cc.RecommendationChunk.Any(chunk => chunk.RecommendationSections.Any(section => section.Id == recommendation.SectionId)) ||
                                                            cc.RecommendationIntro.Any(intro => intro.SubtopicRecommendations.Any(subtopicRecommendation => subtopicRecommendation.Id == recommendation.Id)))
                                              .ToListAsync(cancellationToken);

    return new SubtopicRecommendationDbEntity()
    {
      Intros = intros,
      Section = new RecommendationSectionDbEntity()
      {
        Chunks = chunks,
        Answers = recommendation.Section.Answers
      },
      Subtopic = recommendation.Subtopic,
      SubtopicId = recommendation.SubtopicId
    };
  }

  public Task<RecommendationsViewDto?> GetRecommenationsViewDtoForSubtopicAndMaturity(string subtopicId, string maturity, CancellationToken cancellationToken)
  => _db.SubtopicRecommendations.Where(subtopicRecommendation => subtopicRecommendation.SubtopicId == subtopicId)
                                .Select(subtopicRecommendation => subtopicRecommendation.Intros.FirstOrDefault(intro => intro.Maturity == maturity))
                                .Select(intro => intro != null ? new RecommendationsViewDto(intro.Slug, intro.Header.Text) : null)
                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
}