using Dfe.PlanTech.Core.Configuration;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.QuestionsViewBuilderTests;

public class RouteByQuestionIdTests : QuestionsViewBuilderTestBase
{
    [Fact]
    public async Task RouteByQuestionId_WhenPreviewApiIsDisabled_RedirectsToHomePage()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest(
            new ContentfulOptionsConfiguration { UsePreviewApi = false }
        );

        // Act
        var result = await systemUnderTest.RouteByQuestionId(_controller, FirstQuestionId);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(UrlConstants.HomePage, redirect.Url);
    }

    [Fact]
    public async Task RouteByQuestionId_WhenPreviewApiIsEnabled_ReturnsQuestionViewForQuestion()
    {
        // Arrange
        _contentfulService.GetQuestionByIdAsync(FirstQuestionId).Returns(_firstQuestion);
        var systemUnderTest = CreateSystemUnderTest(
            new ContentfulOptionsConfiguration { UsePreviewApi = true }
        );

        // Act
        var result = await systemUnderTest.RouteByQuestionId(_controller, FirstQuestionId);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Question", view.ViewName);
        var model = Assert.IsType<QuestionViewModel>(view.Model);
        Assert.Same(_firstQuestion, model.Question);
        Assert.Null(model.SectionId);
        Assert.Null(model.CategorySlug);
        Assert.Null(model.SectionSlug);
        Assert.Equal(
            _firstQuestion.Text,
            _controller.ViewData[StatePassingMechanismConstants.Title]
        );
    }

    [Fact]
    public async Task RouteByQuestionId_WhenUserCannotSelectSchools_DoesNotSetSelectedSchoolNames()
    {
        // Arrange
        _contentfulService.GetQuestionByIdAsync(FirstQuestionId).Returns(_firstQuestion);
        _currentUserProvider.CanSelectSchools.Returns(false);
        var systemUnderTest = CreateSystemUnderTest(
            new ContentfulOptionsConfiguration { UsePreviewApi = true }
        );

        // Act
        var result = await systemUnderTest.RouteByQuestionId(_controller, FirstQuestionId);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(view.Model);
        Assert.Empty(model.SelectedSchoolNames);
        Assert.False(model.IsMultiSchoolAssessment);
    }

    [Fact]
    public async Task RouteByQuestionId_WhenUserCanSelectSchools_SetsSelectedSchoolNames()
    {
        // Arrange
        List<string> expectedSchoolNames = ["School One", "School Three"];
        _contentfulService.GetQuestionByIdAsync(FirstQuestionId).Returns(_firstQuestion);
        _currentUserProvider.CanSelectSchools.Returns(true);
        _currentUserProvider.GetSelectedSchoolNames().Returns(expectedSchoolNames);
        var systemUnderTest = CreateSystemUnderTest(
            new ContentfulOptionsConfiguration { UsePreviewApi = true }
        );

        // Act
        var result = await systemUnderTest.RouteByQuestionId(_controller, FirstQuestionId);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(view.Model);
        Assert.Equal(expectedSchoolNames, model.SelectedSchoolNames);
        Assert.Equal(2, model.SelectedSchoolCount);
        Assert.True(model.IsMultiSchoolAssessment);
    }
}
