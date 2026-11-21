using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.GroupsViewBuilderTests;

public class RouteToMatStandardsListAsyncTests : GroupsViewBuilderTestBase
{
    private const string GroupName = "Springfield Academy Trust";

    private readonly QuestionnaireCategoryEntry _cyberSecurityCategory = new()
    {
        Sys = new SystemDetails("category-cyber"),
        Header = new ComponentHeaderEntry { Text = "Cyber security" },
    };

    public RouteToMatStandardsListAsyncTests()
    {
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithOrgName(GroupName).Build());
        _contentfulService
            .GetAllCategoriesAsync()
            .Returns(new List<QuestionnaireCategoryEntry> { _cyberSecurityCategory });
        _contentfulService.GetPageBySlugAsync("home").Returns(new PageEntry());
    }

    [Fact]
    public async Task RouteToMatStandardsListAsync_WhenCalled_ClearsSelectedSchools()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.RouteToMatStandardsListAsync(new TestController());

        // Assert
        _currentUserProvider.Received(1).ClearSelectedSchools();
    }

    [Fact]
    public async Task RouteToMatStandardsListAsync_WhenNoCategoriesExist_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService.GetAllCategoriesAsync().Returns(new List<QuestionnaireCategoryEntry>());
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToMatStandardsListAsync(new TestController())
        );
    }

    [Fact]
    public async Task RouteToMatStandardsListAsync_WhenCalled_ReturnsMatStandardsListView()
    {
        // Arrange
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToMatStandardsListAsync(controller);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("MatStandardsList", view.ViewName);
        var viewModel = Assert.IsType<MatStandardsListViewModel>(view.Model);
        Assert.Equal(GroupName, viewModel.GroupName);
        Assert.Same(_cyberSecurityCategory, Assert.Single(viewModel.Categories));
        Assert.Equal(
            "Select a standard",
            controller.ViewData[StatePassingMechanismConstants.Title]
        );
    }

    [Fact]
    public async Task RouteToMatStandardsListAsync_WhenOrganisationHasNoName_UsesDefaultGroupName()
    {
        // Arrange
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithOrgName(null).Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToMatStandardsListAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<MatStandardsListViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Equal("Your organisation", viewModel.GroupName);
    }

    [Fact]
    public async Task RouteToMatStandardsListAsync_WhenHomePageOrdersCategories_ListsHomePageCategoriesBeforeTheRest()
    {
        // Arrange
        var governanceCategory = new QuestionnaireCategoryEntry
        {
            Sys = new SystemDetails("category-governance"),
            Header = new ComponentHeaderEntry { Text = "Leadership and governance" },
        };
        _contentfulService
            .GetAllCategoriesAsync()
            .Returns(
                new List<QuestionnaireCategoryEntry> { _cyberSecurityCategory, governanceCategory }
            );
        _contentfulService
            .GetPageBySlugAsync("home")
            .Returns(new PageEntry { Content = [governanceCategory] });
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToMatStandardsListAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<MatStandardsListViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Equal([governanceCategory, _cyberSecurityCategory], viewModel.Categories);
    }

    [Fact]
    public async Task RouteToMatStandardsListAsync_WhenHomePageIsMissing_UsesAllCategories()
    {
        // Arrange
        _contentfulService.GetPageBySlugAsync("home").Returns((PageEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToMatStandardsListAsync(new TestController());

        // Assert
        var viewModel = Assert.IsType<MatStandardsListViewModel>(
            Assert.IsType<ViewResult>(result).Model
        );
        Assert.Same(_cyberSecurityCategory, Assert.Single(viewModel.Categories));
    }
}
