using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Domain.Caching.Models;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests;

public class QuestionnaireCacherTests
{
    [Fact]
    public async Task Should_Create_New_Cache_When_Not_Cached_Yet()
    {
        var cacherMock = new Mock<ICacher>();
        cacherMock.Setup(cacher => cacher.GetAsync(It.IsAny<string>(), It.IsAny<Func<QuestionnaireCache>>(), It.IsAny<TimeSpan?>()))
                    .ReturnsAsync((string key, Func<QuestionnaireCache> creator, TimeSpan? timeSpan) => creator());

        var questionnaireCacher = new QuestionnaireCacher(cacherMock.Object);

        var cache = await questionnaireCacher.Cached;

        Assert.NotNull(cache);
        Assert.Null(cache.CurrentSectionTitle);
    }

    [Fact]
    public async Task Should_Save_Cache()
    {
        var cacherMock = new Mock<ICacher>();
        cacherMock.Setup(cacher => cacher.GetAsync(It.IsAny<string>(), It.IsAny<Func<QuestionnaireCache>>(), It.IsAny<TimeSpan?>())).Verifiable();

        var questionnaireCacher = new QuestionnaireCacher(cacherMock.Object);

        await questionnaireCacher.SaveCache(new QuestionnaireCache());

        cacherMock.Verify(cacher => cacher.SetAsync(It.IsAny<string>(), It.IsAny<QuestionnaireCache>(), It.IsAny<TimeSpan?>()));
    }
}