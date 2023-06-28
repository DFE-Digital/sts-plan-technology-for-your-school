using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Domain.Caching.Models;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests;

public class QuestionnaireCacherTests
{
    [Fact]
    public void Should_Create_New_Cache_When_Not_Cached_Yet()
    {
        var cacherMock = new Mock<ICacher>();
        cacherMock.Setup(cacher => cacher.Get(It.IsAny<string>(), It.IsAny<Func<QuestionnaireCache>>()))
                    .Returns((string key, Func<QuestionnaireCache> creator) => creator());

        var questionnaireCacher = new QuestionnaireCacher(cacherMock.Object);

        var cache = questionnaireCacher.Cached;

        Assert.NotNull(cache);
        Assert.Null(cache.CurrentSectionTitle);
    }

    [Fact]
    public void Should_Save_Cache()
    {
        var cacherMock = new Mock<ICacher>();
        cacherMock.Setup(cacher => cacher.Set(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<QuestionnaireCache>())).Verifiable();

        var questionnaireCacher = new QuestionnaireCacher(cacherMock.Object);

        questionnaireCacher.SaveCache(new QuestionnaireCache());

        cacherMock.Verify(cacher => cacher.Set(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<QuestionnaireCache>()));
    }

}