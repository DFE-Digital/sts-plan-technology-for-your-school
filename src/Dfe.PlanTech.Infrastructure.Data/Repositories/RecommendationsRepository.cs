using Dfe.PlanTech.Application.Extensions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Data.Repositories;

public class RecommendationsRepository(ICmsDbContext db, ILogger<IRecommendationsRepository> logger) : IRecommendationsRepository
{
    private readonly ICmsDbContext _db = db;

    public async Task<SubtopicRecommendationDbEntity?> GetCompleteRecommendationsForSubtopic(string subtopicId, CancellationToken cancellationToken)
    {
        var recommendation = await _db.SubtopicRecommendations.Where(subtopicRecommendation => subtopicRecommendation.SubtopicId == subtopicId)
                                              .Select(subtopicRecommendation => new SubtopicRecommendationDbEntity()
                                              {
                                                  Subtopic = new SectionDbEntity()
                                                  {
                                                      Name = subtopicRecommendation.Subtopic.Name
                                                  },
                                                  Section = new RecommendationSectionDbEntity()
                                                  {
                                                      Id = subtopicRecommendation.Section.Id,
                                                      Answers = subtopicRecommendation!.Section.Answers.Select(answer => new AnswerDbEntity() { Id = answer.Id }).ToList(),
                                                  },
                                                  SectionId = subtopicRecommendation!.SectionId,
                                                  Id = subtopicRecommendation.Id
                                              })
                                              .FirstOrDefaultAsyncWithCache(cancellationToken);

        if (recommendation == null)
        {
            return null;
        }

        var intros = await _db.RecommendationIntros.Where(intro => intro.SubtopicRecommendations.Any(subtopicRec => subtopicRec.Id == recommendation.Id))
                                                  .Include(intro => intro.Header)
                                                  .ToListAsyncWithCache(cancellationToken);

        var chunks = await _db.RecommendationChunks.Where(chunk => chunk.RecommendationSections.Any(section => section.Id == recommendation.SectionId))
                                                    .Select(chunk => new RecommendationChunkDbEntity()
                                                    {
                                                        Header = chunk.Header,
                                                        Answers = chunk.Answers.Select(answer => new AnswerDbEntity() { Id = answer.Id }).ToList(),
                                                        Id = chunk.Id,
                                                        Order = chunk.Order,
                                                        CSLink = chunk.CSLink
                                                    })
                                                    .OrderBy(chunk => chunk.Order)
                                                    .ToListAsyncWithCache(cancellationToken);

        var introContent = await _db.RecommendationIntroContents.Where(introContent => introContent.RecommendationIntro != null &&
                                                                                       introContent.RecommendationIntro.SubtopicRecommendations.Any(rec => rec.Id == recommendation.Id))
                                                                .Select(introContent => new RecommendationIntroContentDbEntity()
                                                                {
                                                                    RecommendationIntroId = introContent.RecommendationIntroId,
                                                                    ContentComponent = introContent.ContentComponent,
                                                                    ContentComponentId = introContent.ContentComponentId,
                                                                    Id = introContent.Id
                                                                })
                                                                .ToListAsyncWithCache(cancellationToken);

        var chunkContent = await _db.RecommendationChunkContents.Where(chunkContent => chunkContent.RecommendationChunk != null &&
                                                                                       chunkContent.RecommendationChunk.RecommendationSections.Any(section => section.Id == recommendation.SectionId))
                                                                .Select(chunkContent => new RecommendationChunkContentDbEntity()
                                                                {
                                                                    RecommendationChunkId = chunkContent.RecommendationChunkId,
                                                                    ContentComponent = chunkContent.ContentComponent,
                                                                    ContentComponentId = chunkContent.ContentComponentId,
                                                                    Id = chunkContent.Id
                                                                })
                                                                .ToListAsyncWithCache(cancellationToken);

        LogInvalidJoinRows(introContent);
        LogInvalidJoinRows(chunkContent);

        await _db.RichTextContentWithSubtopicRecommendationIds
          .Where(rt => rt.SubtopicRecommendationId == recommendation.Id)
          .ToListAsyncWithCache(cancellationToken);

        return new SubtopicRecommendationDbEntity()
        {
            Id = recommendation.Id,
            Intros = intros.Select(intro => new RecommendationIntroDbEntity()
            {
                Id = intro.Id,
                Header = intro.Header,
                HeaderId = intro.HeaderId,
                Maturity = intro.Maturity,
                Content = [.. introContent.Where(content => content.RecommendationIntroId == intro.Id && content.ContentComponent != null)
                                            .Select(content => content.ContentComponent)
                                            .OrderBy(content => content?.Order)]
            }).ToList(),
            Section = new RecommendationSectionDbEntity()
            {
                Chunks = chunks.Select(chunk => new RecommendationChunkDbEntity()
                {
                    Id = chunk.Id,
                    Header = chunk.Header,
                    Answers = chunk.Answers,
                    Content = [.. chunkContent.Where(content => content.RecommendationChunkId == chunk.Id && content.ContentComponent != null)
                                            .Select(content => content.ContentComponent)
                                            .OrderBy(content => content?.Order)],
                    CSLink = chunk.CSLink
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
                                  .FirstOrDefaultAsyncWithCache(cancellationToken: cancellationToken);

    /// <summary>
    /// Check for invalid join rows, and log any errored rows.
    /// </summary>
    /// <typeparam name="TContentComponentJoin"></typeparam>
    /// <param name="contentJoins"></param>
    private void LogInvalidJoinRows<TContentComponentJoin>(List<TContentComponentJoin> contentJoins)
    where TContentComponentJoin : class, IHasContentComponent
    {
        var invalidJoins = contentJoins.Where(join => join.ContentComponent == null).ToArray();

        if (invalidJoins.Length == 0)
        {
            return;
        }

        logger.LogError("{ContentJoinType} has {InvalidRowsCount} rows missing ContentComponentId: {ErroredRowIds}", typeof(TContentComponentJoin).Name, invalidJoins.Length, invalidJoins.Select(join => join.Id));
    }
}
