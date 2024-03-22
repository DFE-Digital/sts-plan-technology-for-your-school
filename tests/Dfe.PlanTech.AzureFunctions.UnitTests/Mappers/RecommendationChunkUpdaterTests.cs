using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class RecommendationChunkUpdaterTests
{
    private readonly List<RecommendationChunkAnswerDbEntity> _chunkAnswers =
[
    new RecommendationChunkAnswerDbEntity()
        {
            RecommendationChunkId = "1",
            AnswerId = "E"
        },
        new RecommendationChunkAnswerDbEntity()
        {
            RecommendationChunkId = "B",
            AnswerId = "F"
        },
        new RecommendationChunkAnswerDbEntity()
        {
            RecommendationChunkId = "C",
            AnswerId = "G"
        },
        new RecommendationChunkAnswerDbEntity()
        {
            RecommendationChunkId = "D",
            AnswerId = "H"
        }
];


    private readonly List<RecommendationChunkContentDbEntity> _chunksContents =
    [
        new RecommendationChunkContentDbEntity()
        {
            ContentComponentId= "I",
            RecommendationChunkId= "M"
        },
        new RecommendationChunkContentDbEntity()
        {
            ContentComponentId = "J",
            RecommendationChunkId = "N"
        },
        new RecommendationChunkContentDbEntity()
        {
            ContentComponentId = "K",
            RecommendationChunkId = "O"
        },
        new RecommendationChunkContentDbEntity()
        {
            ContentComponentId = "1",
            RecommendationChunkId = "1"
        }
    ];

    [Fact]
    public void UpdateEntityConcrete_EntityExists_RemovesAssociatedContentAndAnswers()
    {
        var logger = Substitute.For<ILogger<RecommendationChunkUpdater>>();
        var db = Substitute.For<CmsDbContext>();

        IQueryable<RecommendationChunkAnswerDbEntity> queryableAnswers = _chunkAnswers.AsQueryable();

        var asyncProviderAnswers = new AsyncQueryProvider<RecommendationSectionAnswerDbEntity>(queryableAnswers.Provider);

        var mockRecommendationChunkAnswers = Substitute.For<DbSet<RecommendationChunkAnswerDbEntity>, IQueryable<RecommendationChunkAnswerDbEntity>>();
        ((IQueryable<RecommendationChunkAnswerDbEntity>)mockRecommendationChunkAnswers).Provider.Returns(asyncProviderAnswers);
        ((IQueryable<RecommendationChunkAnswerDbEntity>)mockRecommendationChunkAnswers).Expression.Returns(queryableAnswers.Expression);
        ((IQueryable<RecommendationChunkAnswerDbEntity>)mockRecommendationChunkAnswers).ElementType.Returns(queryableAnswers.ElementType);
        ((IQueryable<RecommendationChunkAnswerDbEntity>)mockRecommendationChunkAnswers).GetEnumerator().Returns(queryableAnswers.GetEnumerator());
        db.RecommendationChunkAnswers = mockRecommendationChunkAnswers;

        IQueryable<RecommendationChunkContentDbEntity> queryable = _chunksContents.AsQueryable();

        var asyncProvider = new AsyncQueryProvider<RecommendationSectionChunkDbEntity>(queryable.Provider);

        var mockRecommendationChunkContent = Substitute.For<DbSet<RecommendationChunkContentDbEntity>, IQueryable<RecommendationChunkContentDbEntity>>();
        ((IQueryable<RecommendationChunkContentDbEntity>)mockRecommendationChunkContent).Provider.Returns(asyncProvider);
        ((IQueryable<RecommendationChunkContentDbEntity>)mockRecommendationChunkContent).Expression.Returns(queryable.Expression);
        ((IQueryable<RecommendationChunkContentDbEntity>)mockRecommendationChunkContent).ElementType.Returns(queryable.ElementType);
        ((IQueryable<RecommendationChunkContentDbEntity>)mockRecommendationChunkContent).GetEnumerator().Returns(queryable.GetEnumerator());
        db.RecommendationChunkContents = mockRecommendationChunkContent;

        db.RecommendationChunkAnswers.When(x => x.RemoveRange(Arg.Any<IEnumerable<RecommendationChunkAnswerDbEntity>>())).Do(callInfo =>
        {
            var entitiesToRemove = (IEnumerable<RecommendationChunkAnswerDbEntity>)callInfo[0];
            _chunkAnswers.RemoveAll(entity => entitiesToRemove.Contains(entity));
        });


        db.RecommendationChunkContents.When(x => x.RemoveRange(Arg.Any<IEnumerable<RecommendationChunkContentDbEntity>>())).Do(callInfo =>
        {
            var entitiesToRemove = (IEnumerable<RecommendationChunkContentDbEntity>)callInfo[0];
            _chunksContents.RemoveAll(entity => entitiesToRemove.Contains(entity));
        });

        var updater = new RecommendationChunkUpdater(logger, db);

        var existingSection = new RecommendationChunkDbEntity() { Id = "1" };
        var newSection = new RecommendationChunkDbEntity { Id = "1" };

        var mappedEntity = new MappedEntity
        {
            ExistingEntity = existingSection,
            IncomingEntity = newSection,
            CmsEvent = CmsEvent.SAVE
        };

        var result = updater.UpdateEntityConcrete(mappedEntity);

        Assert.Equal(3, _chunkAnswers.Count);
        Assert.Equal(3, _chunksContents.Count);
    }
}