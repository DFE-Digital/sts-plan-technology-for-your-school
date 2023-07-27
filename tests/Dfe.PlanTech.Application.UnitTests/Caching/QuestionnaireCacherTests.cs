using System.Security.Claims;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Domain.Caching.Models;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests;

public class QuestionnaireCacherTests
{
    public IHttpContextAccessor _httpContextAccessor;
    
    public QuestionnaireCacherTests()
    {
        var usernameClaim = new Claim(BaseCacher.USER_CLAIM, "testing@test.com");

        var httpContextMock = new Mock<IHttpContextAccessor>();
        httpContextMock.Setup(httpContextMock => httpContextMock.HttpContext.User.Claims).Returns(() => new[] {
            usernameClaim
        });

        _httpContextAccessor = httpContextMock.Object;

    }
    [Fact]
    public async Task Should_Create_New_Cache_When_Not_Cached_Yet()
    {
        var cacherMock = new Mock<ICacher>();
        cacherMock.Setup(cacher => cacher.GetAsync(It.IsAny<string>(), It.IsAny<Func<QuestionnaireCache>>(), It.IsAny<TimeSpan?>()))
                    .ReturnsAsync((string key, Func<QuestionnaireCache> creator, TimeSpan? timeSpan) => creator());

        var questionnaireCacher = new QuestionnaireCacher(cacherMock.Object, _httpContextAccessor);

        var cache = await questionnaireCacher.Cached;

        Assert.NotNull(cache);
        Assert.Null(cache.CurrentSectionTitle);
    }

    [Fact]
    public async Task Should_Save_Cache()
    {
        var cacherMock = new Mock<ICacher>();
        cacherMock.Setup(cacher => cacher.GetAsync(It.IsAny<string>(), It.IsAny<Func<QuestionnaireCache>>(), It.IsAny<TimeSpan?>())).Verifiable();

        var questionnaireCacher = new QuestionnaireCacher(cacherMock.Object, _httpContextAccessor);

        await questionnaireCacher.SaveCache(new QuestionnaireCache());

        cacherMock.Verify(cacher => cacher.SetAsync(It.IsAny<string>(), It.IsAny<QuestionnaireCache>(), It.IsAny<TimeSpan?>()));
    }
}