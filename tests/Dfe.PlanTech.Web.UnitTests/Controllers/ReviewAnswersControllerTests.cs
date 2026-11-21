using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Handlers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class ReviewAnswersControllerTests
{
    private const string CategorySlug = "category-slug";
    private const string SectionSlug = "section-slug";
    private const string SectionName = "Section Name";
    private const string ErrorMessage = "error-message";

    private readonly FakeLogger<ReviewAnswersController> _logger = new();
    private readonly IUserJourneyMissingContentExceptionHandler _userJourneyMissingContentExceptionHandler =
        Substitute.For<IUserJourneyMissingContentExceptionHandler>();
    private readonly IReviewAnswersViewBuilder _reviewAnswersViewBuilder =
        Substitute.For<IReviewAnswersViewBuilder>();
    private readonly ISelfAssessmentSummaryViewBuilder _selfAssessmentSummaryViewBuilder =
        Substitute.For<ISelfAssessmentSummaryViewBuilder>();

    private ReviewAnswersController CreateSystemUnderTest() =>
        new(
            _logger,
            _userJourneyMissingContentExceptionHandler,
            _reviewAnswersViewBuilder,
            _selfAssessmentSummaryViewBuilder
        )
        {
            TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Substitute.For<ITempDataProvider>()
            ),
        };

    [Fact]
    public void Constructor_WhenExceptionHandlerIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ReviewAnswersController(
                _logger,
                null!,
                _reviewAnswersViewBuilder,
                _selfAssessmentSummaryViewBuilder
            )
        );
        Assert.Equal("userJourneyMissingContentExceptionHandler", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenReviewAnswersViewBuilderIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ReviewAnswersController(
                _logger,
                _userJourneyMissingContentExceptionHandler,
                null!,
                _selfAssessmentSummaryViewBuilder
            )
        );
        Assert.Equal("reviewAnswersViewBuilder", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenSelfAssessmentSummaryViewBuilderIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ReviewAnswersController(
                _logger,
                _userJourneyMissingContentExceptionHandler,
                _reviewAnswersViewBuilder,
                null!
            )
        );
        Assert.Equal("selfAssessmentSummaryViewBuilder", exception.ParamName);
    }

    [Fact]
    public async Task CheckAnswers_WhenCalled_ReturnsViewBuilderResultWithTempDataErrorMessage()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        systemUnderTest.TempData[StatePassingMechanismConstants.ErrorMessage] = ErrorMessage;
        var expectedResult = new OkResult();
        _reviewAnswersViewBuilder
            .RouteToCheckAnswers(systemUnderTest, CategorySlug, SectionSlug, ErrorMessage)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.CheckAnswers(CategorySlug, SectionSlug);

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Fact]
    public async Task CheckAnswers_WhenNoErrorMessageInTempData_PassesNullErrorMessage()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var expectedResult = new OkResult();
        _reviewAnswersViewBuilder
            .RouteToCheckAnswers(systemUnderTest, CategorySlug, SectionSlug, null)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.CheckAnswers(CategorySlug, SectionSlug);

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Fact]
    public async Task CheckAnswers_WhenContentIsMissing_ReturnsExceptionHandlerResult()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var exception = new UserJourneyMissingContentException(
            "missing-content",
            new QuestionnaireSectionEntry()
        );
        var expectedResult = new BadRequestResult();
        _reviewAnswersViewBuilder
            .RouteToCheckAnswers(systemUnderTest, CategorySlug, SectionSlug, null)
            .ThrowsAsync(exception);
        _userJourneyMissingContentExceptionHandler
            .Handle(systemUnderTest, exception)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.CheckAnswers(CategorySlug, SectionSlug);

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [InlineData(null, SectionSlug)]
    [InlineData("", SectionSlug)]
    [InlineData(" ", SectionSlug)]
    [InlineData(CategorySlug, null)]
    [InlineData(CategorySlug, "")]
    [InlineData(CategorySlug, " ")]
    public async Task CheckAnswers_WhenSlugIsNullOrWhitespace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.CheckAnswers(categorySlug!, sectionSlug!)
        );
    }

    [Fact]
    public async Task ViewAnswers_WhenCalled_ReturnsViewBuilderResultWithTempDataErrorMessage()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        systemUnderTest.TempData[StatePassingMechanismConstants.ErrorMessage] = ErrorMessage;
        var expectedResult = new OkResult();
        _reviewAnswersViewBuilder
            .RouteToViewAnswers(systemUnderTest, CategorySlug, SectionSlug, ErrorMessage)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.ViewAnswers(CategorySlug, SectionSlug);

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Fact]
    public async Task ViewAnswers_WhenContentIsMissing_ReturnsExceptionHandlerResult()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var exception = new UserJourneyMissingContentException(
            "missing-content",
            new QuestionnaireSectionEntry()
        );
        var expectedResult = new BadRequestResult();
        _reviewAnswersViewBuilder
            .RouteToViewAnswers(systemUnderTest, CategorySlug, SectionSlug, null)
            .ThrowsAsync(exception);
        _userJourneyMissingContentExceptionHandler
            .Handle(systemUnderTest, exception)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.ViewAnswers(CategorySlug, SectionSlug);

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [InlineData(null, SectionSlug)]
    [InlineData("", SectionSlug)]
    [InlineData(" ", SectionSlug)]
    [InlineData(CategorySlug, null)]
    [InlineData(CategorySlug, "")]
    [InlineData(CategorySlug, " ")]
    public async Task ViewAnswers_WhenSlugIsNullOrWhitespace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.ViewAnswers(categorySlug!, sectionSlug!)
        );
    }

    [Fact]
    public async Task ConfirmCheckAnswers_WhenCalled_SetsSectionNameInTempDataAndReturnsViewBuilderResult()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var expectedResult = new OkResult();
        _reviewAnswersViewBuilder
            .ConfirmCheckAnswers(systemUnderTest, CategorySlug, SectionSlug, SectionName)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.ConfirmCheckAnswers(
            CategorySlug,
            SectionSlug,
            SectionName
        );

        // Assert
        Assert.Same(expectedResult, result);
        Assert.Equal(
            SectionName,
            systemUnderTest.TempData[StatePassingMechanismConstants.SectionName]
        );
    }

    [Theory]
    [InlineData(null, SectionSlug, SectionName)]
    [InlineData(" ", SectionSlug, SectionName)]
    [InlineData(CategorySlug, null, SectionName)]
    [InlineData(CategorySlug, " ", SectionName)]
    [InlineData(CategorySlug, SectionSlug, null)]
    [InlineData(CategorySlug, SectionSlug, " ")]
    public async Task ConfirmCheckAnswers_WhenArgumentIsNullOrWhitespace_ThrowsArgumentException(
        string? categorySlug,
        string? sectionSlug,
        string? sectionName
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.ConfirmCheckAnswers(categorySlug!, sectionSlug!, sectionName!)
        );
    }

    [Fact]
    public async Task GetSchoolSelfAssessmentSummary_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var expectedResult = new OkResult();
        _selfAssessmentSummaryViewBuilder
            .RouteToSelfAssessmentSummary(systemUnderTest, CategorySlug, SectionSlug)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.GetSchoolSelfAssessmentSummary(
            CategorySlug,
            SectionSlug
        );

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [InlineData(" ", SectionSlug)]
    [InlineData(CategorySlug, " ")]
    public async Task GetSchoolSelfAssessmentSummary_WhenSlugIsWhitespace_ThrowsArgumentException(
        string categorySlug,
        string sectionSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.GetSchoolSelfAssessmentSummary(categorySlug, sectionSlug)
        );
    }

    [Fact]
    public async Task GetTrustSelfAssessmentSummary_WhenCalled_ReturnsViewBuilderResult()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();
        var expectedResult = new OkResult();
        _selfAssessmentSummaryViewBuilder
            .RouteToSelfAssessmentSummary(systemUnderTest, CategorySlug, SectionSlug)
            .Returns(expectedResult);

        // Act
        var result = await systemUnderTest.GetTrustSelfAssessmentSummary(CategorySlug, SectionSlug);

        // Assert
        Assert.Same(expectedResult, result);
    }

    [Theory]
    [InlineData(" ", SectionSlug)]
    [InlineData(CategorySlug, " ")]
    public async Task GetTrustSelfAssessmentSummary_WhenSlugIsWhitespace_ThrowsArgumentException(
        string categorySlug,
        string sectionSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            systemUnderTest.GetTrustSelfAssessmentSummary(categorySlug, sectionSlug)
        );
    }
}
