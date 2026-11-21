using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.QuestionsViewBuilderTests;

public class RouteToInterstitialPageTests : QuestionsViewBuilderTestBase
{
    private const string CategoryName = "Category name";
    private const string ExpectedViewName = "~/Views/Pages/Page.cshtml";

    private readonly SubmissionStatus[] _expectedSubmissionStatuses =
    [
        SubmissionStatus.InProgress,
        SubmissionStatus.CompleteNotReviewed,
        SubmissionStatus.CompleteReviewed,
    ];

    private readonly ComponentButtonWithEntryReferenceEntry _button = new();
    private readonly MissingComponentEntry _otherContent = new();
    private readonly PageEntry _interstitialPage;

    public RouteToInterstitialPageTests()
    {
        _interstitialPage = new PageEntry
        {
            Slug = SectionSlug,
            SectionTitle = "Interstitial",
            Content = [_button, _otherContent],
        };

        _contentfulService.GetPageBySlugAsync(SectionSlug).Returns(_interstitialPage);
        _contentfulService
            .GetCategoryBySlugAsync(CategorySlug)
            .Returns(
                new QuestionnaireCategoryEntry
                {
                    Header = new ComponentHeaderEntry { Text = CategoryName },
                }
            );
    }

    [Fact]
    public async Task RouteToInterstitialPage_WhenInterstitialPageIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService.GetPageBySlugAsync(SectionSlug).Returns((PageEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToInterstitialPage(_controller, CategorySlug, SectionSlug)
        );
        Assert.Contains(SectionSlug, exception.Message);
    }

    [Fact]
    public async Task RouteToInterstitialPage_WhenCategoryIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetCategoryBySlugAsync(CategorySlug)
            .Returns((QuestionnaireCategoryEntry?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToInterstitialPage(_controller, CategorySlug, SectionSlug)
        );
        Assert.Contains(CategorySlug, exception.Message);
    }

    [Fact]
    public async Task RouteToInterstitialPage_WhenCategoryHasNoHeader_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetCategoryBySlugAsync(CategorySlug)
            .Returns(new QuestionnaireCategoryEntry { Header = null! });
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToInterstitialPage(_controller, CategorySlug, SectionSlug)
        );
        Assert.Contains("header", exception.Message);
    }

    [Fact]
    public async Task RouteToInterstitialPage_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync(SectionSlug)
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToInterstitialPage(_controller, CategorySlug, SectionSlug)
        );
        Assert.Contains(SectionSlug, exception.Message);
    }

    [Fact]
    public async Task RouteToInterstitialPage_WhenUserIsNotMat_ReturnsPageViewWithoutTrustAssessmentTable()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(false);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToInterstitialPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ExpectedViewName, view.ViewName);
        var model = Assert.IsType<PageViewModel>(view.Model);
        Assert.Same(_interstitialPage, model.Page);
        Assert.Equal(CategoryName, model.CategoryName);
        Assert.Equal(CategorySlug, model.CategorySlug);
        Assert.False(model.ShowTrustSchoolAssessmentTable);
        Assert.Empty(model.TrustSchoolAssessments);
        Assert.Null(model.TrustSchoolAssessmentContinueHref);
        Assert.Equal([_button, _otherContent], model.Page.Content);
        await _currentUserProvider.DidNotReceive().GetGroupSchoolsAsync();
    }

    [Fact]
    public async Task RouteToInterstitialPage_WhenMatUserHasSelectedSchools_ReturnsPageViewWithoutTrustAssessmentTable()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        _currentUserProvider
            .GetSelectedSchools()
            .Returns([new SelectedEstablishmentDataModelBuilder().Build()]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToInterstitialPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageViewModel>(view.Model);
        Assert.False(model.ShowTrustSchoolAssessmentTable);
        Assert.Empty(model.TrustSchoolAssessments);
        Assert.Equal([_button, _otherContent], model.Page.Content);
        await _currentUserProvider.DidNotReceive().GetGroupSchoolsAsync();
    }

    [Fact]
    public async Task RouteToInterstitialPage_WhenMatUserHasNoSelectedSchools_ShowsTrustAssessmentTableAndRemovesButtons()
    {
        // Arrange
        _currentUserProvider.IsMat.Returns(true);
        _currentUserProvider.GetSelectedSchools().Returns([]);
        _currentUserProvider.GetGroupSchoolsAsync().Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToInterstitialPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageViewModel>(view.Model);
        Assert.True(model.ShowTrustSchoolAssessmentTable);
        Assert.Empty(model.TrustSchoolAssessments);
        Assert.Equal(
            "/trust/category-slug/section-slug/self-assessment/select-schools",
            model.TrustSchoolAssessmentContinueHref
        );
        var remainingContent = Assert.Single(model.Page.Content!);
        Assert.Same(_otherContent, remainingContent);
    }

    [Fact]
    public async Task RouteToInterstitialPage_WhenMatSchoolHasSubmissionInProgress_AddsRowWithViewAnswersLink()
    {
        // Arrange
        var school = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(101)
            .WithName("In Progress School")
            .WithUrn("900006")
            .Build();
        _currentUserProvider.IsMat.Returns(true);
        _currentUserProvider.GetSelectedSchools().Returns([]);
        _currentUserProvider.GetGroupSchoolsAsync().Returns([school]);
        _submissionService
            .GetLatestSubmissionResponsesModel(
                101,
                _section,
                Arg.Is<IEnumerable<SubmissionStatus>>(statuses =>
                    statuses.SequenceEqual(_expectedSubmissionStatuses)
                )
            )
            .Returns(new SubmissionResponsesModel(1, []) { Status = SubmissionStatus.InProgress });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToInterstitialPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageViewModel>(view.Model);
        var row = Assert.Single(model.TrustSchoolAssessments);
        Assert.Equal("In Progress School", row.SchoolName);
        Assert.Equal(SubmissionStatus.InProgress, row.Status);
        Assert.Equal(
            "/school/category-slug/section-slug/self-assessment/view-answers?schoolUrn=900006",
            row.ViewAnswersHref
        );
    }

    [Fact]
    public async Task RouteToInterstitialPage_WhenMatSchoolHasSubmissionCompleteNotReviewed_AddsRowWithViewAnswersLink()
    {
        // Arrange
        var school = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(101)
            .WithName("Completed School")
            .WithUrn("900007")
            .Build();
        _currentUserProvider.IsMat.Returns(true);
        _currentUserProvider.GetSelectedSchools().Returns([]);
        _currentUserProvider.GetGroupSchoolsAsync().Returns([school]);
        _submissionService
            .GetLatestSubmissionResponsesModel(
                101,
                _section,
                Arg.Is<IEnumerable<SubmissionStatus>>(statuses =>
                    statuses.SequenceEqual(_expectedSubmissionStatuses)
                )
            )
            .Returns(
                new SubmissionResponsesModel(1, [])
                {
                    Status = SubmissionStatus.CompleteNotReviewed,
                }
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToInterstitialPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageViewModel>(view.Model);
        var row = Assert.Single(model.TrustSchoolAssessments);
        Assert.Equal(SubmissionStatus.CompleteNotReviewed, row.Status);
        Assert.Equal(
            "/school/category-slug/section-slug/self-assessment/view-answers?schoolUrn=900007",
            row.ViewAnswersHref
        );
    }

    [Fact]
    public async Task RouteToInterstitialPage_WhenMatSchoolHasSubmissionCompleteReviewed_DoesNotAddRow()
    {
        // Arrange
        var school = new SelectedEstablishmentDataModelBuilder().WithEstablishmentId(101).Build();
        _currentUserProvider.IsMat.Returns(true);
        _currentUserProvider.GetSelectedSchools().Returns([]);
        _currentUserProvider.GetGroupSchoolsAsync().Returns([school]);
        _submissionService
            .GetLatestSubmissionResponsesModel(
                101,
                _section,
                Arg.Is<IEnumerable<SubmissionStatus>>(statuses =>
                    statuses.SequenceEqual(_expectedSubmissionStatuses)
                )
            )
            .Returns(
                new SubmissionResponsesModel(1, []) { Status = SubmissionStatus.CompleteReviewed }
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToInterstitialPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageViewModel>(view.Model);
        Assert.True(model.ShowTrustSchoolAssessmentTable);
        Assert.Empty(model.TrustSchoolAssessments);
    }

    [Fact]
    public async Task RouteToInterstitialPage_WhenMatSchoolHasNoSubmission_AddsNotStartedRowWithoutViewAnswersLink()
    {
        // Arrange
        var school = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(101)
            .WithName("Not Started School")
            .Build();
        _currentUserProvider.IsMat.Returns(true);
        _currentUserProvider.GetSelectedSchools().Returns([]);
        _currentUserProvider.GetGroupSchoolsAsync().Returns([school]);
        _submissionService
            .GetLatestSubmissionResponsesModel(
                101,
                _section,
                Arg.Is<IEnumerable<SubmissionStatus>>(statuses =>
                    statuses.SequenceEqual(_expectedSubmissionStatuses)
                )
            )
            .Returns((SubmissionResponsesModel?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToInterstitialPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageViewModel>(view.Model);
        var row = Assert.Single(model.TrustSchoolAssessments);
        Assert.Equal("Not Started School", row.SchoolName);
        Assert.Equal(SubmissionStatus.NotStarted, row.Status);
        Assert.Null(row.ViewAnswersHref);
    }

    [Fact]
    public async Task RouteToInterstitialPage_WhenMatHasSeveralGroupSchools_AddsRowsInGroupOrderSkippingReviewedSchools()
    {
        // Arrange
        var firstSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(101)
            .WithName("First School")
            .WithUrn("900001")
            .Build();
        var reviewedSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(102)
            .WithName("Reviewed School")
            .WithUrn("900002")
            .Build();
        var thirdSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(103)
            .WithName("Third School")
            .WithUrn("900003")
            .Build();
        _currentUserProvider.IsMat.Returns(true);
        _currentUserProvider.GetSelectedSchools().Returns([]);
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns([firstSchool, reviewedSchool, thirdSchool]);
        _submissionService
            .GetLatestSubmissionResponsesModel(
                101,
                _section,
                Arg.Is<IEnumerable<SubmissionStatus>>(statuses =>
                    statuses.SequenceEqual(_expectedSubmissionStatuses)
                )
            )
            .Returns(new SubmissionResponsesModel(1, []) { Status = SubmissionStatus.InProgress });
        _submissionService
            .GetLatestSubmissionResponsesModel(
                102,
                _section,
                Arg.Is<IEnumerable<SubmissionStatus>>(statuses =>
                    statuses.SequenceEqual(_expectedSubmissionStatuses)
                )
            )
            .Returns(
                new SubmissionResponsesModel(2, []) { Status = SubmissionStatus.CompleteReviewed }
            );
        _submissionService
            .GetLatestSubmissionResponsesModel(
                103,
                _section,
                Arg.Is<IEnumerable<SubmissionStatus>>(statuses =>
                    statuses.SequenceEqual(_expectedSubmissionStatuses)
                )
            )
            .Returns((SubmissionResponsesModel?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToInterstitialPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PageViewModel>(view.Model);
        Assert.Collection(
            model.TrustSchoolAssessments,
            row => Assert.Equal("First School", row.SchoolName),
            row => Assert.Equal("Third School", row.SchoolName)
        );
    }
}
