using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.GroupsViewBuilderTests;

public class RouteToViewInProgressAnswersTests : GroupsViewBuilderTestBase
{
    private const string SchoolUrn = "100011";

    [Fact]
    public async Task RouteToViewInProgressAnswers_WhenSchoolIsNotFound_RedirectsToHomePage()
    {
        // Arrange
        _establishmentService
            .GetEstablishmentByReferenceAsync(SchoolUrn)
            .Returns((SqlEstablishmentDto?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToViewInProgressAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SchoolUrn
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PagesController.GetByRoute), redirect.ActionName);
        Assert.Equal(UrlConstants.HomePage, redirect.RouteValues!["route"]);
    }

    [Fact]
    public async Task RouteToViewInProgressAnswers_WhenSectionIsMissing_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeSchool();
        _contentfulService
            .GetSectionBySlugAsync(SectionSlug)
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToViewInProgressAnswers(
                new TestController(),
                CategorySlug,
                SectionSlug,
                SchoolUrn
            )
        );
        Assert.Contains(SectionSlug, exception.Message);
    }

    [Theory]
    [InlineData(SubmissionStatus.InProgress)]
    [InlineData(SubmissionStatus.CompleteNotReviewed)]
    public async Task RouteToViewInProgressAnswers_WhenSubmissionIsInProgressOrAwaitingReview_ReturnsViewAnswersView(
        SubmissionStatus status
    )
    {
        // Arrange
        ArrangeSchool();
        ArrangeSubmission(status);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToViewInProgressAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SchoolUrn
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ReviewAnswersViewBuilder.ViewAnswersViewName, view.ViewName);
        var viewModel = Assert.IsType<ViewAnswersViewModel>(view.Model);
        Assert.True(viewModel.IsMatInProgressView);
        Assert.Equal("Oakfield Primary", viewModel.SchoolName);
        Assert.Equal(_section.Name, viewModel.TopicName);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
        Assert.Equal(SectionSlug, viewModel.SectionSlug);
    }

    [Theory]
    [InlineData(SubmissionStatus.NotStarted)]
    [InlineData(SubmissionStatus.CompleteReviewed)]
    public async Task RouteToViewInProgressAnswers_WhenSubmissionIsNotInProgress_RedirectsToHomePage(
        SubmissionStatus status
    )
    {
        // Arrange
        ArrangeSchool();
        ArrangeSubmission(status);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToViewInProgressAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SchoolUrn
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(UrlConstants.HomePage, redirect.RouteValues!["route"]);
    }

    private void ArrangeSchool() =>
        _establishmentService
            .GetEstablishmentByReferenceAsync(SchoolUrn)
            .Returns(
                new SqlEstablishmentDtoBuilder()
                    .WithId(11)
                    .WithEstablishmentRef(SchoolUrn)
                    .WithOrgName("Oakfield Primary")
                    .Build()
            );

    private void ArrangeSubmission(SubmissionStatus status)
    {
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
        _submissionService
            .GetSubmissionRoutingDataAsync(11, _section, SubmissionStatus.InProgress)
            .Returns(
                new SubmissionRoutingDataModel(
                    null,
                    _section,
                    new SubmissionResponsesModel(1, []),
                    status
                )
            );
    }
}
