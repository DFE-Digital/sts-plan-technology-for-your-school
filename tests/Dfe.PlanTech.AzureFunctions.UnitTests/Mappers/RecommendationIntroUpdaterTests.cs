using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class RecommendationIntroUpdaterTests
{
    private readonly List<RecommendationIntroContentDbEntity> _introContents =
    [
        new RecommendationIntroContentDbEntity()
        {
            RecommendationIntroId = "1",
            ContentComponentId = "A"
        },
        new RecommendationIntroContentDbEntity()
        {
            RecommendationIntroId = "2",
            ContentComponentId = "B"
        },
        new RecommendationIntroContentDbEntity()
        {
            RecommendationIntroId = "3",
            ContentComponentId = "C"
        },
        new RecommendationIntroContentDbEntity()
        {
            RecommendationIntroId = "4",
            ContentComponentId = "D"
        }
    ];


    [Fact]
    public void UpdateEntityConcrete_EntityExists_RemovesAssociatedIntro()
    {
        var logger = Substitute.For<ILogger<RecommendationIntroUpdater>>();
        var db = Substitute.For<CmsDbContext>();

        IQueryable<RecommendationIntroContentDbEntity> queryableIntroContents = _introContents.AsQueryable();

        var asyncProviderAnswers = new AsyncQueryProvider<RecommendationIntroContentDbEntity>(queryableIntroContents.Provider);

        var mockRecommendationIntroContents = Substitute.For<DbSet<RecommendationIntroContentDbEntity>, IQueryable<RecommendationIntroContentDbEntity>>();
        ((IQueryable<RecommendationIntroContentDbEntity>)mockRecommendationIntroContents).Provider.Returns(asyncProviderAnswers);
        ((IQueryable<RecommendationIntroContentDbEntity>)mockRecommendationIntroContents).Expression.Returns(queryableIntroContents.Expression);
        ((IQueryable<RecommendationIntroContentDbEntity>)mockRecommendationIntroContents).ElementType.Returns(queryableIntroContents.ElementType);
        ((IQueryable<RecommendationIntroContentDbEntity>)mockRecommendationIntroContents).GetEnumerator().Returns(queryableIntroContents.GetEnumerator());
        db.RecommendationIntroContents = mockRecommendationIntroContents;

        db.RecommendationIntroContents.When(x => x.RemoveRange(Arg.Any<IEnumerable<RecommendationIntroContentDbEntity>>())).Do(callInfo =>
        {
            var entitiesToRemove = (IEnumerable<RecommendationIntroContentDbEntity>)callInfo[0];
            _introContents.RemoveAll(entity => entitiesToRemove.Contains(entity));
        });


        var updater = new RecommendationIntroUpdater(logger, db);

        var existingIntro = new RecommendationIntroDbEntity { Id = "1" };
        var newIntro = new RecommendationIntroDbEntity { Id = "1" };

        var mappedEntity = new MappedEntity
        {
            ExistingEntity = existingIntro,
            IncomingEntity = newIntro,
            CmsEvent = CmsEvent.SAVE
        };

        var result = updater.UpdateEntityConcrete(mappedEntity);

        Assert.Equal(3, _introContents.Count);
    }

}