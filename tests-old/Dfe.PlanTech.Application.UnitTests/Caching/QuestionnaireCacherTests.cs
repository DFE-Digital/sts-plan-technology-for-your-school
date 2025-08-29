using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Core.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests;

public class QuestionnaireCacherTests
{
    [Fact]
    public void Should_Create_New_Cache_When_Not_Cached_Yet()
    {
        var cacherSubstitute = Substitute.For<ICacher>();
        cacherSubstitute.Get(Arg.Any<string>(), Arg.Any<Func<QuestionnaireCache>>())
            .Returns((callInfo) =>
            {
                Func<QuestionnaireCache> creator = (Func<QuestionnaireCache>)callInfo[1];
                return creator();
            });

        var questionnaireCacher = new QuestionnaireCacher(cacherSubstitute);

        var cache = questionnaireCacher.Cached;

        Assert.NotNull(cache);
        Assert.Null(cache.CurrentSectionTitle);
    }

    [Fact]
    public void Should_Save_Cache()
    {
        var cacherSubstitute = Substitute.For<ICacher>();
        cacherSubstitute.Set(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<QuestionnaireCache>());

        var questionnaireCacher = new QuestionnaireCacher(cacherSubstitute);

        questionnaireCacher.SaveCache(new QuestionnaireCache());

        cacherSubstitute.Received().Set(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<QuestionnaireCache>());
    }
}
