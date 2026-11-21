using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.QuestionsViewBuilderTests;

public class ContinuePreviousAssessmentTests : QuestionsViewBuilderTestBase
{
    [Fact]
    public async Task ContinuePreviousAssessment_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync("missing-section")
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.ContinuePreviousAssessment(_controller, CategorySlug, "missing-section")
        );
        Assert.Contains("missing-section", exception.Message);
    }

    [Fact]
    public async Task ContinuePreviousAssessment_WhenCalled_RestoresInaccessibleSubmission()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.ContinuePreviousAssessment(_controller, CategorySlug, SectionSlug);

        // Assert
        await _submissionService
            .Received(1)
            .RestoreInaccessibleSubmissionAsync(FocusedEstablishmentId, SectionId);
    }

    [Fact]
    public async Task ContinuePreviousAssessment_WhenCalled_RedirectsToNextUnansweredQuestion()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.ContinuePreviousAssessment(
            _controller,
            CategorySlug,
            SectionSlug
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.GetNextUnansweredQuestion), redirect.ActionName);
        Assert.Equal(QuestionsController.Controller, redirect.ControllerName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(CategorySlug, redirect.RouteValues["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
    }
}
