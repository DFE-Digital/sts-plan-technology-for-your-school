using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.QuestionsViewBuilderTests;

public class ConstructorTests : QuestionsViewBuilderTestBase
{
    private readonly IOptions<ContactOptionsConfiguration> _contactOptions = Options.Create(
        new ContactOptionsConfiguration { LinkId = ContactLinkId }
    );
    private readonly IOptions<ErrorMessagesConfiguration> _errorMessages = Options.Create(
        new ErrorMessagesConfiguration()
    );
    private readonly ContentfulOptionsConfiguration _contentfulOptions = new();

    // ConstructorGuardAssert cannot be used: the logger parameter is passed to BaseViewBuilder, which does not guard it.
    [Fact]
    public void Constructor_WhenAnyGuardedDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Equal(
            "contactOptions",
            Assert
                .Throws<ArgumentNullException>(() =>
                    new QuestionsViewBuilder(
                        _logger,
                        null!,
                        _errorMessages,
                        _contentfulOptions,
                        _contentfulService,
                        _currentUserProvider,
                        _establishmentService,
                        _questionService,
                        _recommendationService,
                        _submissionService
                    )
                )
                .ParamName
        );
        Assert.Equal(
            "errorMessages",
            Assert
                .Throws<ArgumentNullException>(() =>
                    new QuestionsViewBuilder(
                        _logger,
                        _contactOptions,
                        null!,
                        _contentfulOptions,
                        _contentfulService,
                        _currentUserProvider,
                        _establishmentService,
                        _questionService,
                        _recommendationService,
                        _submissionService
                    )
                )
                .ParamName
        );
        Assert.Equal(
            "contentfulOptions",
            Assert
                .Throws<ArgumentNullException>(() =>
                    new QuestionsViewBuilder(
                        _logger,
                        _contactOptions,
                        _errorMessages,
                        null!,
                        _contentfulService,
                        _currentUserProvider,
                        _establishmentService,
                        _questionService,
                        _recommendationService,
                        _submissionService
                    )
                )
                .ParamName
        );
        Assert.Equal(
            "contentfulService",
            Assert
                .Throws<ArgumentNullException>(() =>
                    new QuestionsViewBuilder(
                        _logger,
                        _contactOptions,
                        _errorMessages,
                        _contentfulOptions,
                        null!,
                        _currentUserProvider,
                        _establishmentService,
                        _questionService,
                        _recommendationService,
                        _submissionService
                    )
                )
                .ParamName
        );
        Assert.Equal(
            "currentUser",
            Assert
                .Throws<ArgumentNullException>(() =>
                    new QuestionsViewBuilder(
                        _logger,
                        _contactOptions,
                        _errorMessages,
                        _contentfulOptions,
                        _contentfulService,
                        null!,
                        _establishmentService,
                        _questionService,
                        _recommendationService,
                        _submissionService
                    )
                )
                .ParamName
        );
        Assert.Equal(
            "establishmentService",
            Assert
                .Throws<ArgumentNullException>(() =>
                    new QuestionsViewBuilder(
                        _logger,
                        _contactOptions,
                        _errorMessages,
                        _contentfulOptions,
                        _contentfulService,
                        _currentUserProvider,
                        null!,
                        _questionService,
                        _recommendationService,
                        _submissionService
                    )
                )
                .ParamName
        );
        Assert.Equal(
            "questionService",
            Assert
                .Throws<ArgumentNullException>(() =>
                    new QuestionsViewBuilder(
                        _logger,
                        _contactOptions,
                        _errorMessages,
                        _contentfulOptions,
                        _contentfulService,
                        _currentUserProvider,
                        _establishmentService,
                        null!,
                        _recommendationService,
                        _submissionService
                    )
                )
                .ParamName
        );
        Assert.Equal(
            "recommendationService",
            Assert
                .Throws<ArgumentNullException>(() =>
                    new QuestionsViewBuilder(
                        _logger,
                        _contactOptions,
                        _errorMessages,
                        _contentfulOptions,
                        _contentfulService,
                        _currentUserProvider,
                        _establishmentService,
                        _questionService,
                        null!,
                        _submissionService
                    )
                )
                .ParamName
        );
        Assert.Equal(
            "submissionService",
            Assert
                .Throws<ArgumentNullException>(() =>
                    new QuestionsViewBuilder(
                        _logger,
                        _contactOptions,
                        _errorMessages,
                        _contentfulOptions,
                        _contentfulService,
                        _currentUserProvider,
                        _establishmentService,
                        _questionService,
                        _recommendationService,
                        null!
                    )
                )
                .ParamName
        );
    }
}
