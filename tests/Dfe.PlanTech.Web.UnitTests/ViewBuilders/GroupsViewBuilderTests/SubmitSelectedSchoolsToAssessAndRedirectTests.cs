using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.GroupsViewBuilderTests;

public class SubmitSelectedSchoolsToAssessAndRedirectTests : GroupsViewBuilderTestBase
{
    private readonly TestController _controller = new();

    private readonly GroupsSelectSchoolsToAssessViewModel _viewModel = new()
    {
        SelectedSchoolsRefs = ["100011"],
    };

    public static IEnumerable<object?[]> NoSelectedSchoolRefs =>
        [
            [null],
            [new List<string>()],
        ];

    public SubmitSelectedSchoolsToAssessAndRedirectTests()
    {
        _controller.RouteData.Values["categorySlug"] = CategorySlug;
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(
                new List<SelectedEstablishmentDataModel> { _oakfieldPrimary, _riverbankPrimary }
            );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenSectionSlugIsMissing_ThrowsArgumentNullException(
        string? sectionSlug
    )
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
                _controller,
                sectionSlug!,
                _viewModel
            )
        );
    }

    [Theory]
    [MemberData(nameof(NoSelectedSchoolRefs))]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenNoSchoolsAreSelected_ThrowsInvalidDataException(
        List<string>? selectedSchoolRefs
    )
    {
        // Arrange
        _viewModel.SelectedSchoolsRefs = selectedSchoolRefs;
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
                _controller,
                SectionSlug,
                _viewModel
            )
        );
        Assert.Equal("No schools have been selected", exception.Message);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenCategorySlugIsMissingFromRoute_ThrowsInvalidDataException()
    {
        // Arrange
        _controller.RouteData.Values.Remove("categorySlug");
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
                _controller,
                SectionSlug,
                _viewModel
            )
        );
        Assert.Equal("Missing category slug", exception.Message);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenSectionIsMissing_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync(SectionSlug)
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
                _controller,
                SectionSlug,
                _viewModel
            )
        );
        Assert.Contains(SectionSlug, exception.Message);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenSelectedSchoolIsNotInGroup_ThrowsInvalidDataException()
    {
        // Arrange
        _viewModel.SelectedSchoolsRefs = ["999999"];
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
                _controller,
                SectionSlug,
                _viewModel
            )
        );
        await _currentUserProvider
            .DidNotReceive()
            .SetSelectedSchoolsAsync(Arg.Any<IEnumerable<SelectedEstablishmentDataModel>>());
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenSchoolsAreSelected_SetsSelectedSchoolsFromGroupSchools()
    {
        // Arrange
        _viewModel.SelectedSchoolsRefs = ["100012", "100011"];
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
            _controller,
            SectionSlug,
            _viewModel
        );

        // Assert
        await _currentUserProvider
            .Received(1)
            .SetSelectedSchoolsAsync(
                Arg.Is<IEnumerable<SelectedEstablishmentDataModel>>(schools =>
                    schools.Select(school => school.EstablishmentId).SequenceEqual(new[] { 12, 11 })
                    && schools
                        .Select(school => school.Urn)
                        .SequenceEqual(new[] { "100012", "100011" })
                    && schools
                        .Select(school => school.Name)
                        .SequenceEqual(new[] { "Riverbank Primary", "Oakfield Primary" })
                )
            );
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenAllSchoolsAreSelected_SetsPresentedSchoolsAsSelected()
    {
        // Arrange
        _viewModel.SelectedSchoolsRefs = ["all"];
        _viewModel.PresentedSchoolRefs = ["100011", "100012"];
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
            _controller,
            SectionSlug,
            _viewModel
        );

        // Assert
        await _currentUserProvider
            .Received(1)
            .SetSelectedSchoolsAsync(
                Arg.Is<IEnumerable<SelectedEstablishmentDataModel>>(schools =>
                    schools.Select(school => school.Urn).SequenceEqual(new[] { "100011", "100012" })
                )
            );
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenOneSchoolIsSelected_SetsItAsFocusedEstablishment()
    {
        // Arrange
        _currentUserProvider.GetSelectedSchools().Returns([_oakfieldPrimary]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
            _controller,
            SectionSlug,
            _viewModel
        );

        // Assert
        await _currentUserProvider
            .Received(1)
            .SetFocusedEstablishmentAsync("100011", "Oakfield Primary");
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenOneSchoolHasSubmissionInProgress_RedirectsToContinueSelfAssessment()
    {
        // Arrange
        _currentUserProvider.GetSelectedSchools().Returns([_oakfieldPrimary]);
        ArrangeLatestSubmissionStatus(11, SubmissionStatus.InProgress);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
            _controller,
            SectionSlug,
            _viewModel
        );

        // Assert
        var redirect = Assert.IsType<RedirectToRouteResult>(result);
        Assert.Equal(QuestionsController.GetContinueSelfAssessmentAction, redirect.RouteName);
        Assert.Equal(CategorySlug, redirect.RouteValues!["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
        await _submissionService
            .DidNotReceiveWithAnyArgs()
            .SetSubmissionInaccessibleAsync(default, default!);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenOneSchoolHasNoSubmissionInProgress_RedirectsToFirstQuestion()
    {
        // Arrange
        _currentUserProvider.GetSelectedSchools().Returns([_oakfieldPrimary]);
        ArrangeLatestSubmissionStatus(11, SubmissionStatus.CompleteReviewed);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
            _controller,
            SectionSlug,
            _viewModel
        );

        // Assert
        var redirect = Assert.IsType<RedirectToRouteResult>(result);
        Assert.Equal(QuestionsController.GetQuestionBySlugAction, redirect.RouteName);
        Assert.Equal(CategorySlug, redirect.RouteValues!["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
        Assert.Equal(FirstQuestionSlug, redirect.RouteValues["questionSlug"]);
        await _submissionService
            .DidNotReceiveWithAnyArgs()
            .SetSubmissionInaccessibleAsync(default, default!);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenSeveralSchoolsAreSelected_DoesNotSetFocusedEstablishment()
    {
        // Arrange
        _viewModel.SelectedSchoolsRefs = ["100011", "100012"];
        _currentUserProvider.GetSelectedSchools().Returns([_oakfieldPrimary, _riverbankPrimary]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
            _controller,
            SectionSlug,
            _viewModel
        );

        // Assert
        await _currentUserProvider
            .DidNotReceiveWithAnyArgs()
            .SetFocusedEstablishmentAsync(default!, default!);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenSeveralSchoolsAreSelected_SetsOnlyInProgressSubmissionsInaccessible()
    {
        // Arrange
        _viewModel.SelectedSchoolsRefs = ["100011", "100012"];
        _currentUserProvider.GetSelectedSchools().Returns([_oakfieldPrimary, _riverbankPrimary]);
        ArrangeLatestSubmissionStatus(11, SubmissionStatus.InProgress);
        ArrangeLatestSubmissionStatus(12, SubmissionStatus.CompleteReviewed);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
            _controller,
            SectionSlug,
            _viewModel
        );

        // Assert
        await _submissionService.Received(1).SetSubmissionInaccessibleAsync(11, SectionId);
        await _submissionService.DidNotReceive().SetSubmissionInaccessibleAsync(12, SectionId);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenSeveralSchoolsAreSelected_RedirectsToFirstQuestion()
    {
        // Arrange
        _viewModel.SelectedSchoolsRefs = ["100011", "100012"];
        _currentUserProvider.GetSelectedSchools().Returns([_oakfieldPrimary, _riverbankPrimary]);
        ArrangeLatestSubmissionStatus(11, SubmissionStatus.InProgress);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
            _controller,
            SectionSlug,
            _viewModel
        );

        // Assert
        var redirect = Assert.IsType<RedirectToRouteResult>(result);
        Assert.Equal(QuestionsController.GetQuestionBySlugAction, redirect.RouteName);
        Assert.Equal(FirstQuestionSlug, redirect.RouteValues!["questionSlug"]);
    }

    [Fact]
    public async Task SubmitSelectedSchoolsToAssessAndRedirect_WhenNoSelectedSchoolsAreStored_RedirectsToFirstQuestion()
    {
        // Arrange
        _currentUserProvider
            .GetSelectedSchools()
            .Returns((List<SelectedEstablishmentDataModel>?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.SubmitSelectedSchoolsToAssessAndRedirect(
            _controller,
            SectionSlug,
            _viewModel
        );

        // Assert
        var redirect = Assert.IsType<RedirectToRouteResult>(result);
        Assert.Equal(QuestionsController.GetQuestionBySlugAction, redirect.RouteName);
    }

    private void ArrangeLatestSubmissionStatus(int establishmentId, SubmissionStatus status) =>
        _submissionService
            .GetLatestSubmissionResponsesModel(establishmentId, _section, (SubmissionStatus?)null)
            .Returns(new SubmissionResponsesModel(1, []) { Status = status });
}
