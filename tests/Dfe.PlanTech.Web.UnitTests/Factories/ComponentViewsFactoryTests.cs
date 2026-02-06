using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.Factories;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Factories;

public class ComponentViewsFactoryTests
{
    [Fact]
    public void TryGetViewForType_SupportsNestedFolders_ByReplacingUnderscores()
    {
        var logger = Substitute.For<ILogger<ComponentViewsFactory>>();
        var sut = new ComponentViewsFactory(logger);

        var model = new ComponentTextBodyEntry();

        var ok = sut.TryGetViewForType(model, out var path);

        Assert.True(ok);
        Assert.Equal("Components/TextBody", path);
    }

    [Fact]
    public void TryGetViewForType_WhenNoMatch_LogsWarning_And_ReturnsFalse()
    {
        var logger = Substitute.For<ILogger<ComponentViewsFactory>>();
        var sut = new ComponentViewsFactory(logger);

        var model = new Exception();

        var ok = sut.TryGetViewForType(model, out var path);

        Assert.False(ok);
        Assert.Null(path);

        // Verify a warning log occurred
        logger
            .ReceivedWithAnyArgs(1)
            .Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public void TryGetViewForType_ReturnsFalse_DoesNotLog_QuestionnaireCategory()
    {
        var logger = Substitute.For<ILogger<ComponentViewsFactory>>();
        var sut = new ComponentViewsFactory(logger);

        var model = new QuestionnaireCategoryEntry();

        var ok = sut.TryGetViewForType(model, out var path);

        Assert.False(ok);
        Assert.Null(path);

        // Verify no warning logged
        logger
            .ReceivedWithAnyArgs(0)
            .Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }
}
