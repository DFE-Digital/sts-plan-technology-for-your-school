using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.PagesViewBuilderTests;

public class ConstructorTests : PagesViewBuilderTestBase
{
    private readonly IOptions<ContactOptionsConfiguration> _contactOptions = Options.Create(
        new ContactOptionsConfiguration { LinkId = ContactLinkId }
    );
    private readonly IOptions<ErrorPagesConfiguration> _errorPages = Options.Create(
        new ErrorPagesConfiguration { InternalErrorPageId = InternalErrorPageId }
    );

    [Fact]
    public void Constructor_WhenContactOptionsIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new PagesViewBuilder(
                _logger,
                null!,
                _errorPages,
                _contentfulService,
                _currentUser,
                _notifyService,
                _submissionService,
                _recommendationService
            )
        );
        Assert.Equal("contactOptions", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenErrorPagesIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new PagesViewBuilder(
                _logger,
                _contactOptions,
                null!,
                _contentfulService,
                _currentUser,
                _notifyService,
                _submissionService,
                _recommendationService
            )
        );
        Assert.Equal("errorPages", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenContentfulServiceIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new PagesViewBuilder(
                _logger,
                _contactOptions,
                _errorPages,
                null!,
                _currentUser,
                _notifyService,
                _submissionService,
                _recommendationService
            )
        );
        Assert.Equal("contentfulService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenCurrentUserIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new PagesViewBuilder(
                _logger,
                _contactOptions,
                _errorPages,
                _contentfulService,
                null!,
                _notifyService,
                _submissionService,
                _recommendationService
            )
        );
        Assert.Equal("currentUser", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenNotifyServiceIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new PagesViewBuilder(
                _logger,
                _contactOptions,
                _errorPages,
                _contentfulService,
                _currentUser,
                null!,
                _submissionService,
                _recommendationService
            )
        );
        Assert.Equal("notifyService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenSubmissionServiceIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new PagesViewBuilder(
                _logger,
                _contactOptions,
                _errorPages,
                _contentfulService,
                _currentUser,
                _notifyService,
                null!,
                _recommendationService
            )
        );
        Assert.Equal("submissionService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenRecommendationServiceIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new PagesViewBuilder(
                _logger,
                _contactOptions,
                _errorPages,
                _contentfulService,
                _currentUser,
                _notifyService,
                _submissionService,
                null!
            )
        );
        Assert.Equal("recommendationService", exception.ParamName);
    }
}
