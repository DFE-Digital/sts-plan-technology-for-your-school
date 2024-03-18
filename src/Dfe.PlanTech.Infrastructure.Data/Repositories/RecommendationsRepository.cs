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

    if (recommendation == null)
    {
      return null;
    }

    var intros = await _db.RecommendationIntros.Where(intro => intro.SubtopicRecommendations.Any(subtopicRec => subtopicRec.Id == recommendation.Id))
                                              .Include(intro => intro.Header)
                                              .ToListAsync(cancellationToken);

    var test = await _db.RecommendationChunks.Include(chunk => chunk.RecommendationSections).ToListAsync(cancellationToken: cancellationToken);

    var chunks = await _db.RecommendationChunks.Where(chunk => chunk.RecommendationSections.Any(section => section.Id == recommendation.SectionId))
                                                .Select(chunk => new RecommendationChunkDbEntity()
                                                {
                                                  Header = new HeaderDbEntity() { Text = chunk.Header.Text, Size = chunk.Header.Size, Tag = chunk.Header.Tag },
                                                  Title = chunk.Title,
                                                  Answers = chunk.Answers.Select(answer => new AnswerDbEntity() { Id = answer.Id }).ToList(),
                                                  Id = chunk.Id,
                                                })
                                                .ToListAsync(cancellationToken);

    var introContent = await _db.RecommendationIntroContents.Where(introContent => introContent.RecommendationIntro.SubtopicRecommendations.Any(rec => rec.Id == recommendation.Id))
                                                            .Select(introContent => new
                                                            {
                                                              intro = introContent.RecommendationIntroId,
                                                              content = introContent.ContentComponent
                                                            })
                                                            .ToListAsync(cancellationToken);

    var chunkContent = await _db.RecommendationChunkContents.Where(chunkContent => chunkContent.RecommendationChunk.RecommendationSections.Any(section => section.Id == recommendation.SectionId))
                                                            .Select(chunkContent => new
                                                            {
                                                              chunk = chunkContent.RecommendationChunkId,
                                                              content = chunkContent.ContentComponent
                                                            })
                                                            .ToListAsync(cancellationToken);

    await _db.RichTextContentWithSubtopicRecommendationIds
              .Where(rt => rt.SubtopicRecommendationId == recommendation.Id)
              .ToListAsync(cancellationToken);

    return new SubtopicRecommendationDbEntity()
    {
      Id = recommendation.Id,
      Intros = intros.Select(intro => new RecommendationIntroDbEntity()
      {
        Id = intro.Id,
        Header = intro.Header,
        HeaderId = intro.HeaderId,
        Maturity = intro.Maturity,
        Content = introContent.Where(content => content.intro == intro.Id).Select(content => content.content!).ToList()
      }).ToList(),
      Section = new RecommendationSectionDbEntity()
      {
        Chunks = chunks.Select(chunk => new RecommendationChunkDbEntity()
        {
          Id = chunk.Id,
          Header = chunk.Header,
          HeaderId = chunk.HeaderId,
          Answers = chunk.Answers,
          Content = chunkContent.Where(content => content.chunk == chunk.Id).Select(content => content.content!).ToList()
        }).ToList(),
        Answers = recommendation.Section.Answers,
        Id = recommendation.Section.Id,
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