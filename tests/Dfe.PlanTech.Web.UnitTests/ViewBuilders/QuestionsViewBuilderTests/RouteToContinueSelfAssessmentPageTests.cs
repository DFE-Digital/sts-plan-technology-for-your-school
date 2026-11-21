using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.QuestionsViewBuilderTests;

public class RouteToContinueSelfAssessmentPageTests : QuestionsViewBuilderTestBase
{
    private const string FocusedSchoolName = "Focused School";

    private static readonly DateTime DateCreated = new(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime DateLastUpdated = new(2026, 1, 2, 10, 30, 0, DateTimeKind.Utc);

    private readonly List<QuestionWithAnswerModel> _responses =
    [
        new()
        {
            QuestionSysId = FirstQuestionId,
            AnswerSysId = "answer-1-id",
            QuestionText = "Question one",
            AnswerText = "Answer one",
        },
    ];

    private readonly SqlEstablishmentDto _focusedEstablishmentDto = new SqlEstablishmentDtoBuilder()
        .WithId(FocusedEstablishmentId)
        .WithOrgName(FocusedSchoolName)
        .Build();

    [Fact]
    public async Task RouteToContinueSelfAssessmentPage_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync("missing-section")
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToContinueSelfAssessmentPage(
                _controller,
                CategorySlug,
                "missing-section"
            )
        );
        Assert.Contains("missing-section", exception.Message);
    }

    [Fact]
    public async Task RouteToContinueSelfAssessmentPage_WhenNoSubmissionExists_RedirectsToInterstitialPage()
    {
        // Arrange
        _submissionService
            .GetLatestSubmissionResponsesModel(
                FocusedEstablishmentId,
                _section,
                (SubmissionStatus?)null
            )
            .Returns((SubmissionResponsesModel?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToContinueSelfAssessmentPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PagesController.GetByRoute), redirect.ActionName);
        Assert.Equal(nameof(PagesController).GetControllerNameSlug(), redirect.ControllerName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(SectionSlug, redirect.RouteValues["route"]);
    }

    [Fact]
    public async Task RouteToContinueSelfAssessmentPage_WhenSubmissionHasNoResponses_RedirectsToInterstitialPage()
    {
        // Arrange
        _submissionService
            .GetLatestSubmissionResponsesModel(
                FocusedEstablishmentId,
                _section,
                (SubmissionStatus?)null
            )
            .Returns(new SubmissionResponsesModel(1, []) { Status = SubmissionStatus.InProgress });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToContinueSelfAssessmentPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PagesController.GetByRoute), redirect.ActionName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(SectionSlug, redirect.RouteValues["route"]);
    }

    [Fact]
    public async Task RouteToContinueSelfAssessmentPage_WhenSubmissionIsObsolete_ReturnsRestartObsoleteAssessmentView()
    {
        // Arrange
        _submissionService
            .GetLatestSubmissionResponsesModel(
                FocusedEstablishmentId,
                _section,
                (SubmissionStatus?)null
            )
            .Returns(
                new SubmissionResponsesModel(1, _responses) { Status = SubmissionStatus.Obsolete }
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToContinueSelfAssessmentPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("RestartObsoleteAssessment", view.ViewName);
        var model = Assert.IsType<RestartObsoleteAssessmentViewModel>(view.Model);
        Assert.Equal(SectionName, model.TopicName);
        Assert.Equal(CategorySlug, model.CategorySlug);
        Assert.Equal(SectionSlug, model.SectionSlug);
    }

    [Fact]
    public async Task RouteToContinueSelfAssessmentPage_WhenSubmissionHasResponses_ReturnsContinueSelfAssessmentView()
    {
        // Arrange
        _submissionService
            .GetLatestSubmissionResponsesModel(
                FocusedEstablishmentId,
                _section,
                (SubmissionStatus?)null
            )
            .Returns(
                new SubmissionResponsesModel(1, _responses)
                {
                    Status = SubmissionStatus.InProgress,
                    DateCreated = DateCreated,
                    DateLastUpdated = DateLastUpdated,
                }
            );
        _establishmentService
            .GetEstablishmentByIdAsync(FocusedEstablishmentId)
            .Returns(_focusedEstablishmentDto);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToContinueSelfAssessmentPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("ContinueSelfAssessment", view.ViewName);
        var model = Assert.IsType<ContinueSelfAssessmentViewModel>(view.Model);
        Assert.Equal(DateCreated, model.AssessmentStartDate);
        Assert.Equal(DateLastUpdated, model.AssessmentUpdatedDate);
        Assert.Equal(1, model.AnsweredCount);
        Assert.Equal(2, model.QuestionsCount);
        Assert.Equal(SectionName, model.TopicName);
        Assert.Same(_responses, model.Responses);
        Assert.Equal(CategorySlug, model.CategorySlug);
        Assert.Equal(SectionSlug, model.SectionSlug);
        Assert.Equal(FocusedSchoolName, model.SchoolName);
    }

    [Fact]
    public async Task RouteToContinueSelfAssessmentPage_WhenEstablishmentHasNoName_ReturnsViewWithEmptySchoolName()
    {
        // Arrange
        _submissionService
            .GetLatestSubmissionResponsesModel(
                FocusedEstablishmentId,
                _section,
                (SubmissionStatus?)null
            )
            .Returns(
                new SubmissionResponsesModel(1, _responses)
                {
                    Status = SubmissionStatus.InProgress,
                    DateCreated = DateCreated,
                    DateLastUpdated = DateLastUpdated,
                }
            );
        _establishmentService
            .GetEstablishmentByIdAsync(FocusedEstablishmentId)
            .Returns(
                new SqlEstablishmentDtoBuilder()
                    .WithId(FocusedEstablishmentId)
                    .WithOrgName(null)
                    .Build()
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToContinueSelfAssessmentPage(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ContinueSelfAssessmentViewModel>(view.Model);
        Assert.Equal(string.Empty, model.SchoolName);
    }
}
