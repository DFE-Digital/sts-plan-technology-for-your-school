using System.Security.Claims;
using Dfe.PlanTech.Application.SignIn.Interfaces;
using Dfe.PlanTech.Domain.SignIn.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Moq;

namespace Dfe.PlanTech.Infrastructure.SignIn.UnitTests;

public class DfeOpenIdConnectEventsTests
{
    [Fact]
    public async Task OnTokenValidated_Should_AddRoles_OnTokenValidated_When_DiscoverRolesWithPublicApi_Is_True()
    {
        var wasCalled = false;

        var config = new DfeSignInConfiguration()
        {
            DiscoverRolesWithPublicApi = true
        };

        var dfePublicApiMock = new Mock<IDfePublicApi>();

        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.SetupGet(principal => principal.Identity).Returns(() =>
        {
            wasCalled = true;
            return null;
        });

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(context => context.RequestServices.GetService(typeof(IDfeSignInConfiguration))).Returns(config);
        contextMock.Setup(context => context.RequestServices.GetService(typeof(IDfePublicApi))).Returns(dfePublicApiMock.Object);
        var context = new TokenValidatedContext(contextMock.Object,
                                                new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
                                                new OpenIdConnectOptions(), claimsPrincipalMock.Object,
                                                new AuthenticationProperties());

        await DfeOpenIdConnectEvents.OnTokenValidated(context);

        Assert.True(wasCalled);
    }


    [Fact]
    public async Task OnTokenValidated_Should_AddRoles_OnTokenValidated_When_DiscoverRolesWithPublicApi_Is_False()
    {
        var wasCalled = false;

        var config = new DfeSignInConfiguration()
        {
            DiscoverRolesWithPublicApi = false
        };

        var dfePublicApiMock = new Mock<IDfePublicApi>();

        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.SetupGet(principal => principal.Identity).Returns(() =>
        {
            wasCalled = true;
            return null;
        });

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(context => context.RequestServices.GetService(typeof(IDfeSignInConfiguration))).Returns(config);
        contextMock.Setup(context => context.RequestServices.GetService(typeof(IDfePublicApi))).Returns(dfePublicApiMock.Object);

        var context = new TokenValidatedContext(contextMock.Object,
                                                new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
                                                new OpenIdConnectOptions(),
                                                claimsPrincipalMock.Object,
                                                new AuthenticationProperties());

        await DfeOpenIdConnectEvents.OnTokenValidated(context);

        Assert.False(wasCalled);
    }

    [Fact]
    public async Task OnRedirectToIdentityProvider_Should_Rewrite_Url_Always()
    {
        var config = new DfeSignInConfiguration()
        {
            CallbackUrl = "/auth/cb",
            FrontDoorUrl = "www.frontdoorurl.com"
        };

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(context => context.RequestServices.GetService(typeof(IDfeSignInConfiguration))).Returns(config);

        var context = new RedirectContext(contextMock.Object,
                                        new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
                                        new OpenIdConnectOptions(),
                                        new AuthenticationProperties());

        var openIdConnectMessage = new OpenIdConnectMessage
        {
            RedirectUri = "https://notarealaddress.ua"
        };

        context.ProtocolMessage = openIdConnectMessage;

        await DfeOpenIdConnectEvents.OnRedirectToIdentityProvider(context);

        var expectedUrl = config.FrontDoorUrl + config.CallbackUrl;

        Assert.Equal(expectedUrl, openIdConnectMessage.RedirectUri);
    }

    [Fact]
    public async Task OnRedirectToIdentityProviderForSignOut_Should_Rewrite_Url_When_ProtocolMessage_Is_NotNull()
    {
        var config = new DfeSignInConfiguration()
        {
            SignoutRedirectUrl = "/signout/cb",
            FrontDoorUrl = "www.frontdoorurl.com"
        };

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(context => context.RequestServices.GetService(typeof(IDfeSignInConfiguration))).Returns(config);

        var context = new RedirectContext(contextMock.Object,
                                        new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
                                        new OpenIdConnectOptions(),
                                        new AuthenticationProperties());

        var openIdConnectMessage = new OpenIdConnectMessage
        {
            PostLogoutRedirectUri = "https://notarealaddress.ua"
        };

        context.ProtocolMessage = openIdConnectMessage;

        await DfeOpenIdConnectEvents.OnRedirectToIdentityProviderForSignOut(context);

        var expectedUrl = config.FrontDoorUrl + config.SignoutRedirectUrl;

        Assert.Equal(expectedUrl, openIdConnectMessage.PostLogoutRedirectUri);
    }
}