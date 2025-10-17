using System.Security.Claims;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Context;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Handlers;

public class UserJourneyMissingContentExceptionHandlerTests
{
    private static (CurrentUser currentUser, HttpContext http) MakeCurrentUser(int? establishmentId = null)
    {
        var http = new DefaultHttpContext();
        var identity = new ClaimsIdentity(authenticationType: "test");

        if (establishmentId.HasValue)
        {
            identity.AddClaim(new Claim(ClaimConstants.DB_ESTABLISHMENT_ID, establishmentId.Value.ToString()));
        }
        http.User = new ClaimsPrincipal(identity);
        var accessor = new HttpContextAccessor { HttpContext = http };
        var establishmentService = Substitute.For<IEstablishmentService>();
        var logger = Substitute.For<ILogger<CurrentUser>>();
        return (new CurrentUser(accessor, establishmentService, logger), http);
    }

    private static Controller MakeController(HttpContext http)
    {
        var controller = new TestController();
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        controller.TempData = new TempDataDictionary(http, Substitute.For<ITempDataProvider>());
        return controller;
    }

    private static UserJourneyMissingContentException MakeException(string sectionId = "sec-1", string name = "Section Name")
    {
        var section = new QuestionnaireSectionEntry
        {
            // In your project, Id is a shortcut for Sys.Id
            Sys = new SystemDetails(sectionId),
            Name = name
        };
        return new UserJourneyMissingContentException("boom", section);
    }

    private static IConfiguration MakeConfig(string value)
    {
        var dict = new Dictionary<string, string?>
        {
            { UserJourneyMissingContentExceptionHandler.ErrorMessageConfigKey, value }
        };
        return new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
    }

    private sealed class TestController : Controller { }

    [Fact]
    public async Task Handle_DeletesSubmission_SetsTempData_And_Redirects()
    {
        // Arrange
        var logger = Substitute.For<ILogger<UserJourneyMissingContentExceptionHandler>>();
        var submissionSvc = Substitute.For<ISubmissionService>();
        var (currentUser, http) = MakeCurrentUser(establishmentId: 456);
        var controller = MakeController(http);
        var configValue = "Please try again — your answers may be out of date.";
        var config = MakeConfig(configValue);

        var sut = new UserJourneyMissingContentExceptionHandler(logger, config, submissionSvc, currentUser);
        var ex = MakeException(sectionId: "sec-1");

        // Act
        var result = await sut.Handle(controller, ex);

        // Assert: deletion called with current user's establishment and section id
        await submissionSvc.Received(1).SetSubmissionInaccessibleAsync(456, "sec-1");

        // TempData set
        Assert.True(controller.TempData.ContainsKey(UserJourneyMissingContentExceptionHandler.ErrorMessageTempDataKey));
        Assert.Equal(configValue, controller.TempData[UserJourneyMissingContentExceptionHandler.ErrorMessageTempDataKey]);

        // Redirect to PagesController.GetPageByRouteAction with route = UrlConstants.Home
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(PagesController.GetPageByRouteAction, redirect.ActionName);
        Assert.Equal(PagesController.ControllerName, redirect.ControllerName);
        Assert.True(redirect.RouteValues!.ContainsKey("route"));
        Assert.Equal(UrlConstants.Home, redirect.RouteValues["route"]);
    }

    [Fact]
    public async Task Handle_Throws_When_CurrentUser_Has_No_EstablishmentId()
    {
        // Arrange
        var logger = Substitute.For<ILogger<UserJourneyMissingContentExceptionHandler>>();
        var submissionSvc = Substitute.For<ISubmissionService>();
        var (currentUser, http) = MakeCurrentUser(establishmentId: null);
        var controller = MakeController(http);
        var config = MakeConfig("ignored");
        var sut = new UserJourneyMissingContentExceptionHandler(logger, config, submissionSvc, currentUser);
        var ex = MakeException();

        // Act + Assert
        var thrown = await Assert.ThrowsAsync<InvalidDataException>(() => sut.Handle(controller, ex));
        Assert.Contains(nameof(CurrentUser.ActiveEstablishmentId), thrown.Message);

        await submissionSvc.DidNotReceiveWithAnyArgs().SetSubmissionInaccessibleAsync(default!, default!);
    }
}
