using Dfe.PlanTech.Application.SignIn.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.SignIn.Enums;
using Dfe.PlanTech.Domain.SignIn.Models;
using Dfe.PlanTech.Infrastructure.SignIn.ConnectEvents;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NSubstitute;
using System.Security.Claims;
using System.Text.Json;

namespace Dfe.PlanTech.Infrastructure.SignIn.UnitTests;

public class DfeOpenIdConnectEventsTests
{
    [Fact]
    public async Task OnTokenValidated_Should_TryAddRoles_OnTokenValidated_When_DiscoverRolesWithPublicApi_Is_True()
    {
        var wasCalled = false;

        var config = new DfeSignInConfiguration()
        {
            DiscoverRolesWithPublicApi = true
        };

        var dfePublicApiMock = Substitute.For<IDfePublicApi>();

        var claimsPrincipalMock = Substitute.For<ClaimsPrincipal>();
        claimsPrincipalMock.Identity.Returns((callInfo) =>
        {
            wasCalled = true;
            return null;
        });

        var contextMock = Substitute.For<HttpContext>();
        contextMock.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(config);
        contextMock.RequestServices.GetService(typeof(IDfePublicApi)).Returns(dfePublicApiMock);
        var context = new UserInformationReceivedContext(contextMock,
                                                new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
                                                new OpenIdConnectOptions(),
                                                claimsPrincipalMock,
                                                new AuthenticationProperties());

        await OnUserInformationReceivedEvent.OnUserInformationReceived(context);

        Assert.True(wasCalled);
    }

    [Fact]
    public async Task OnTokenValidated_Should_AddRoles_OnTokenValidated_When_DiscoverRolesWithPublicApi_Is_True()
    {
        Guid userId = Guid.NewGuid();
        Guid orgId = Guid.NewGuid();

        var config = new DfeSignInConfiguration()
        {
            DiscoverRolesWithPublicApi = true
        };

        var expectedRole = new Role()
        {
            Status = new Status()
            {
                Id = 1
            },
            Id = Guid.NewGuid(),
            Name = "ExpectedRole",
            Code = "ExpectedCode",
            NumericId = "NumericId"
        };

        var dfePublicApiMock = Substitute.For<IDfePublicApi>();
        var commandMock = Substitute.For<IRecordUserSignInCommand>();
        var userAccessToService = new UserAccessToService()
        {
            UserId = userId,
            OrganisationId = orgId,
            Roles = new List<Role>(){
                expectedRole,
                new Role(){
                    Status = new Status() {
                        Id = 2
                    },
                    Name = "OtherRole",
                    Code = "OtherCode",
                    NumericId = "OtherNumericId",
                }
            }
        };

        dfePublicApiMock.GetUserAccessToService(userId.ToString(), orgId.ToString()).Returns(userAccessToService);

        var identityMock = Substitute.For<ClaimsIdentity>();

        var organisationClaim = new Organisation()
        {
            Id = orgId,
            Name = "Organisation"
        };

        var organisationClaimSerialised = JsonSerializer.Serialize(organisationClaim);

        identityMock.Claims.Returns(new List<Claim>(){
                            new Claim(ClaimConstants.NameIdentifier, userId.ToString()),
                            new Claim(ClaimConstants.Organisation, organisationClaimSerialised),
                        });

        identityMock.IsAuthenticated.Returns(true);

        var claimsPrincipal = new ClaimsPrincipal(identityMock);

        var contextMock = Substitute.For<HttpContext>();
        contextMock.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(config);
        contextMock.RequestServices.GetService(typeof(IDfePublicApi)).Returns(dfePublicApiMock);
        contextMock.RequestServices.GetService(typeof(IRecordUserSignInCommand)).Returns(commandMock);

        var context = new UserInformationReceivedContext(contextMock,
                                                new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
                                                new OpenIdConnectOptions(),
                                                claimsPrincipal,
                                                new AuthenticationProperties());

        await OnUserInformationReceivedEvent.OnUserInformationReceived(context);

        //Assert Service called
        await dfePublicApiMock.Received().GetUserAccessToService(userId.ToString(), orgId.ToString());

        //Assert has right number of identites
        Assert.NotNull(context.Principal);
        var identities = context.Principal.Identities;
        Assert.Equal(2, identities.Count());

        //Assert claims added
        var addedIdentity = identities.Skip(1).FirstOrDefault()!;

        var roleCodeClaim = addedIdentity.Claims.FirstOrDefault(c => c.Type == ClaimConstants.RoleCode);
        Assert.NotNull(roleCodeClaim);
        Assert.Equal(expectedRole.Code, roleCodeClaim.Value);

        var roleIdClaim = addedIdentity.Claims.FirstOrDefault(c => c.Type == ClaimConstants.RoleId);
        Assert.NotNull(roleIdClaim);
        Assert.Equal(expectedRole.Id.ToString(), roleIdClaim.Value);

        var roleNameClaim = addedIdentity.Claims.FirstOrDefault(c => c.Type == ClaimConstants.RoleName);
        Assert.NotNull(roleNameClaim);
        Assert.Equal(expectedRole.Name, roleNameClaim.Value);

        var roleNumericIdClaim = addedIdentity.Claims.FirstOrDefault(c => c.Type == ClaimConstants.RoleNumericId);
        Assert.NotNull(roleNumericIdClaim);
        Assert.Equal(expectedRole.NumericId, roleNumericIdClaim.Value);

    }

    [Fact]
    public async Task OnTokenValidated_Should_AddRoles_OnTokenValidated_When_DiscoverRolesWithPublicApi_Is_False()
    {
        var wasCalled = false;

        var config = new DfeSignInConfiguration()
        {
            DiscoverRolesWithPublicApi = false
        };

        var dfePublicApiMock = Substitute.For<IDfePublicApi>();

        var claimsPrincipalMock = Substitute.For<ClaimsPrincipal>();
        claimsPrincipalMock.Identity.Returns((callInfo) =>
        {
            wasCalled = false;
            return null;
        });

        var contextMock = Substitute.For<HttpContext>();
        contextMock.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(config);
        contextMock.RequestServices.GetService(typeof(IDfePublicApi)).Returns(dfePublicApiMock);

        var context = new UserInformationReceivedContext(contextMock,
                                                new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
                                                new OpenIdConnectOptions(),
                                                claimsPrincipalMock,
                                                new AuthenticationProperties());

        await OnUserInformationReceivedEvent.OnUserInformationReceived(context);

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

        var contextMock = Substitute.For<HttpContext>();
        contextMock.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(config);

        var context = new RedirectContext(contextMock,
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

        var contextMock = Substitute.For<HttpContext>();
        contextMock.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(config);

        var context = new RedirectContext(contextMock,
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