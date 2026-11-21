using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.QuestionsViewBuilderTests;

public class RestartSelfAssessmentTests : QuestionsViewBuilderTestBase
{
    [Fact]
    public async Task RestartSelfAssessment_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync("missing-section")
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RestartSelfAssessment(
                _controller,
                CategorySlug,
                "missing-section",
                false
            )
        );
        Assert.Contains("missing-section", exception.Message);
    }

    [Fact]
    public async Task RestartSelfAssessment_WhenNotObsoleteFlow_MarksSubmissionInaccessible()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RestartSelfAssessment(_controller, CategorySlug, SectionSlug, false);

        // Assert
        await _submissionService
            .Received(1)
            .SetSubmissionInaccessibleAsync(FocusedEstablishmentId, SectionId);
    }

    [Fact]
    public async Task RestartSelfAssessment_WhenObsoleteFlow_DoesNotMarkSubmissionInaccessible()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RestartSelfAssessment(_controller, CategorySlug, SectionSlug, true);

        // Assert
        await _submissionService
            .DidNotReceiveWithAnyArgs()
            .SetSubmissionInaccessibleAsync(default, default!);
    }

    [Fact]
    public async Task RestartSelfAssessment_WhenCalled_RedirectsToInterstitialPage()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RestartSelfAssessment(
            _controller,
            CategorySlug,
            SectionSlug,
            false
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.GetInterstitialPage), redirect.ActionName);
        Assert.Equal(QuestionsController.Controller, redirect.ControllerName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(CategorySlug, redirect.RouteValues["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
    }
}
