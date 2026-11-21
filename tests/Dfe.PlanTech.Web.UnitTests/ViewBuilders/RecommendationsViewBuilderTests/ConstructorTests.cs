using Dfe.PlanTech.Web.ViewBuilders;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests;

public class ConstructorTests : RecommendationsViewBuilderTestBase
{
    // RecommendationsViewBuilder does not guard its logger, so ConstructorGuardAssert cannot be used here.
    [Fact]
    public void Constructor_WhenAnyGuardedDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var contentfulServiceException = Assert.Throws<ArgumentNullException>(() =>
            new RecommendationsViewBuilder(
                _logger,
                null!,
                _currentUserProvider,
                _notifyService,
                _recommendationService,
                _submissionService,
                _microcopyProvider,
                _groupService
            )
        );
        var currentUserProviderException = Assert.Throws<ArgumentNullException>(() =>
            new RecommendationsViewBuilder(
                _logger,
                _contentfulService,
                null!,
                _notifyService,
                _recommendationService,
                _submissionService,
                _microcopyProvider,
                _groupService
            )
        );
        var notifyServiceException = Assert.Throws<ArgumentNullException>(() =>
            new RecommendationsViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                null!,
                _recommendationService,
                _submissionService,
                _microcopyProvider,
                _groupService
            )
        );
        var recommendationServiceException = Assert.Throws<ArgumentNullException>(() =>
            new RecommendationsViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                _notifyService,
                null!,
                _submissionService,
                _microcopyProvider,
                _groupService
            )
        );
        var submissionServiceException = Assert.Throws<ArgumentNullException>(() =>
            new RecommendationsViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                _notifyService,
                _recommendationService,
                null!,
                _microcopyProvider,
                _groupService
            )
        );
        var microcopyProviderException = Assert.Throws<ArgumentNullException>(() =>
            new RecommendationsViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                _notifyService,
                _recommendationService,
                _submissionService,
                null!,
                _groupService
            )
        );
        var groupServiceException = Assert.Throws<ArgumentNullException>(() =>
            new RecommendationsViewBuilder(
                _logger,
                _contentfulService,
                _currentUserProvider,
                _notifyService,
                _recommendationService,
                _submissionService,
                _microcopyProvider,
                null!
            )
        );

        Assert.Equal("contentfulService", contentfulServiceException.ParamName);
        Assert.Equal("currentUser", currentUserProviderException.ParamName);
        Assert.Equal("notifyService", notifyServiceException.ParamName);
        Assert.Equal("recommendationService", recommendationServiceException.ParamName);
        Assert.Equal("submissionService", submissionServiceException.ParamName);
        Assert.Equal("microcopyProvider", microcopyProviderException.ParamName);
        Assert.Equal("groupService", groupServiceException.ParamName);
    }
}
