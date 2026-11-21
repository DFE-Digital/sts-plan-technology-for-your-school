using Dfe.PlanTech.Web.ViewBuilders;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.ReviewAnswersViewBuilderTests;

public class ConstructorTests : ReviewAnswersViewBuilderTestBase
{
    [Fact]
    public void Constructor_WhenContentfulServiceIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ReviewAnswersViewBuilder(_logger, null!, _currentUserProvider, _submissionService)
        );
        Assert.Equal("contentfulService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenCurrentUserProviderIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ReviewAnswersViewBuilder(_logger, _contentfulService, null!, _submissionService)
        );
        Assert.Equal("currentUser", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenSubmissionServiceIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ReviewAnswersViewBuilder(_logger, _contentfulService, _currentUserProvider, null!)
        );
        Assert.Equal("submissionService", exception.ParamName);
    }
}
