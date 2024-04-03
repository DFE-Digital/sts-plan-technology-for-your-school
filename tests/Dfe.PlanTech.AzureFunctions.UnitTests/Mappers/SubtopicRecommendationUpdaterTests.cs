using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class SubtopicRecommendationUpdaterTests
{
    private readonly List<SubtopicRecommendationIntroDbEntity> _subtopicIntros =
    [
        new SubtopicRecommendationIntroDbEntity()
        {
            RecommendationIntroId = "A",
            SubtopicRecommendationId = "1"
        },
        new SubtopicRecommendationIntroDbEntity()
        {
            RecommendationIntroId = "2",
            SubtopicRecommendationId = "B"
        },
        new SubtopicRecommendationIntroDbEntity()
        {
            RecommendationIntroId = "3",
            SubtopicRecommendationId = "C"
        },
        new SubtopicRecommendationIntroDbEntity()
        {
            RecommendationIntroId = "4",
            SubtopicRecommendationId = "D"
        }
    ];

    [Fact]
    public void UpdateEntityConcrete_EntityExists_RemovesAssociatedOldSubtopicIntros()
    {
        var logger = Substitute.For<ILogger<SubtopicRecommendationUpdater>>();
        var db = Substitute.For<CmsDbContext>();

        IQueryable<SubtopicRecommendationIntroDbEntity> queryableIntroContents = _subtopicIntros.AsQueryable();

        var asyncProviderAnswers = new AsyncQueryProvider<SubtopicRecommendationIntroDbEntity>(queryableIntroContents.Provider);

        var mockRecommendationIntroContents = Substitute.For<DbSet<SubtopicRecommendationIntroDbEntity>, IQueryable<SubtopicRecommendationIntroDbEntity>>();
        ((IQueryable<SubtopicRecommendationIntroDbEntity>)mockRecommendationIntroContents).Provider.Returns(asyncProviderAnswers);
        ((IQueryable<SubtopicRecommendationIntroDbEntity>)mockRecommendationIntroContents).Expression.Returns(queryableIntroContents.Expression);
        ((IQueryable<SubtopicRecommendationIntroDbEntity>)mockRecommendationIntroContents).ElementType.Returns(queryableIntroContents.ElementType);
        ((IQueryable<SubtopicRecommendationIntroDbEntity>)mockRecommendationIntroContents).GetEnumerator().Returns(queryableIntroContents.GetEnumerator());
        db.SubtopicRecommendationIntros = mockRecommendationIntroContents;

        db.SubtopicRecommendationIntros.When(x => x.RemoveRange(Arg.Any<IEnumerable<SubtopicRecommendationIntroDbEntity>>())).Do(callInfo =>
        {
            var entitiesToRemove = (IEnumerable<SubtopicRecommendationIntroDbEntity>)callInfo[0];
            _subtopicIntros.RemoveAll(entity => entitiesToRemove.Contains(entity));
        });


        var updater = new SubtopicRecommendationUpdater(logger, db);

        var existingSubtopicIntro = new SubtopicRecommendationDbEntity() { Id = "1" };
        var newSubtopicIntro = new SubtopicRecommendationDbEntity { Id = "1" };

        var mappedEntity = new MappedEntity
        {
            ExistingEntity = existingSubtopicIntro,
            IncomingEntity = newSubtopicIntro,
            CmsEvent = CmsEvent.SAVE
        };

        var result = updater.UpdateEntityConcrete(mappedEntity);

        Assert.Equal(3, _subtopicIntros.Count);
    }


}