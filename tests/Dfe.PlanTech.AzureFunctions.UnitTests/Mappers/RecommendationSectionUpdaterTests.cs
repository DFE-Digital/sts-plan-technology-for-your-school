using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class RecommendationSectionUpdaterTests
{
    private readonly List<RecommendationSectionAnswerDbEntity> _sectionAnwsers =
    [
        new RecommendationSectionAnswerDbEntity()
        {
            RecommendationSectionId = "1",
            AnswerId = "E"
        },
        new RecommendationSectionAnswerDbEntity()
        {
            RecommendationSectionId = "B",
            AnswerId = "F"
        },
        new RecommendationSectionAnswerDbEntity()
        {
            RecommendationSectionId = "C",
            AnswerId = "G"
        },
        new RecommendationSectionAnswerDbEntity()
        {
            RecommendationSectionId = "D",
            AnswerId = "H"
        }
    ];


    private readonly List<RecommendationSectionChunkDbEntity> _sectionChunks =
    [
        new RecommendationSectionChunkDbEntity()
        {
            RecommendationSectionId = "I",
            RecommendationChunkId = "M"
        },
        new RecommendationSectionChunkDbEntity()
        {
            RecommendationSectionId = "J",
            RecommendationChunkId = "N"
        },
        new RecommendationSectionChunkDbEntity()
        {
            RecommendationSectionId = "K",
            RecommendationChunkId = "O"
        },
        new RecommendationSectionChunkDbEntity()
        {
            RecommendationSectionId = "1",
            RecommendationChunkId = "P"
        }
    ];

    [Fact]
    public void UpdateEntityConcrete_EntityExists_RemovesAssociatedChunksAndAnswers()
    {
        var logger = Substitute.For<ILogger<RecommendationSectionUpdater>>();
        var db = Substitute.For<CmsDbContext>();

        IQueryable<RecommendationSectionAnswerDbEntity> queryableAnswers = _sectionAnwsers.AsQueryable();

        var asyncProviderAnswers = new AsyncQueryProvider<RecommendationSectionAnswerDbEntity>(queryableAnswers.Provider);

        var mockRecommendationSectionAnswers = Substitute.For<DbSet<RecommendationSectionAnswerDbEntity>, IQueryable<RecommendationSectionAnswerDbEntity>>();
        ((IQueryable<RecommendationSectionAnswerDbEntity>)mockRecommendationSectionAnswers).Provider.Returns(asyncProviderAnswers);
        ((IQueryable<RecommendationSectionAnswerDbEntity>)mockRecommendationSectionAnswers).Expression.Returns(queryableAnswers.Expression);
        ((IQueryable<RecommendationSectionAnswerDbEntity>)mockRecommendationSectionAnswers).ElementType.Returns(queryableAnswers.ElementType);
        ((IQueryable<RecommendationSectionAnswerDbEntity>)mockRecommendationSectionAnswers).GetEnumerator().Returns(queryableAnswers.GetEnumerator());
        db.RecommendationSectionAnswers = mockRecommendationSectionAnswers;

        IQueryable<RecommendationSectionChunkDbEntity> queryable = _sectionChunks.AsQueryable();

        var asyncProvider = new AsyncQueryProvider<RecommendationSectionChunkDbEntity>(queryable.Provider);

        var mockRecommendationSectionChunks = Substitute.For<DbSet<RecommendationSectionChunkDbEntity>, IQueryable<RecommendationSectionChunkDbEntity>>();
        ((IQueryable<RecommendationSectionChunkDbEntity>)mockRecommendationSectionChunks).Provider.Returns(asyncProvider);
        ((IQueryable<RecommendationSectionChunkDbEntity>)mockRecommendationSectionChunks).Expression.Returns(queryable.Expression);
        ((IQueryable<RecommendationSectionChunkDbEntity>)mockRecommendationSectionChunks).ElementType.Returns(queryable.ElementType);
        ((IQueryable<RecommendationSectionChunkDbEntity>)mockRecommendationSectionChunks).GetEnumerator().Returns(queryable.GetEnumerator());

        db.RecommendationSectionAnswers = mockRecommendationSectionAnswers;
        db.RecommendationSectionChunks = mockRecommendationSectionChunks;

        db.RecommendationSectionAnswers.When(x => x.RemoveRange(Arg.Any<IEnumerable<RecommendationSectionAnswerDbEntity>>())).Do(callInfo =>
        {
            var entitiesToRemove = (IEnumerable<RecommendationSectionAnswerDbEntity>)callInfo[0];
            _sectionAnwsers.RemoveAll(entity => entitiesToRemove.Contains(entity));
        });


        db.RecommendationSectionChunks.When(x => x.RemoveRange(Arg.Any<IEnumerable<RecommendationSectionChunkDbEntity>>())).Do(callInfo =>
        {
            var entitiesToRemove = (IEnumerable<RecommendationSectionChunkDbEntity>)callInfo[0];
            _sectionChunks.RemoveAll(entity => entitiesToRemove.Contains(entity));
        });

        var updater = new RecommendationSectionUpdater(logger, db);

        var existingSection = new RecommendationSectionDbEntity { Id = "1" };
        var newSection = new RecommendationSectionDbEntity { Id = "1" };

        var mappedEntity = new MappedEntity
        {
            ExistingEntity = existingSection,
            IncomingEntity = newSection
        };

        var result = updater.UpdateEntityConcrete(mappedEntity);

        Assert.Equal(3, _sectionAnwsers.Count);
        Assert.Equal(3, _sectionChunks.Count);
    }
}