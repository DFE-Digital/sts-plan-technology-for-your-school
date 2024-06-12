using Dfe.PlanTech.Application.SignIns.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.SignIns.Enums;
using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Domain.Users.Exceptions;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Domain.Users.Models;
using Dfe.PlanTech.Infrastructure.SignIns.ConnectEvents;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NSubstitute;
using System.Security.Claims;
using System.Text.Json;

namespace Dfe.PlanTech.Infrastructure.SignIns.UnitTests;

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

        var dfePublicApiSubstitute = Substitute.For<IDfePublicApi>();

        var claimsPrincipalSubstitute = Substitute.For<ClaimsPrincipal>();
        claimsPrincipalSubstitute.Identity.Returns((callInfo) =>
        {
            wasCalled = true;
            return null;
        });

        var contextSubstitute = Substitute.For<HttpContext>();
        contextSubstitute.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(config);
        contextSubstitute.RequestServices.GetService(typeof(IDfePublicApi)).Returns(dfePublicApiSubstitute);
        var context = new UserInformationReceivedContext(contextSubstitute,
            new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
            new OpenIdConnectOptions(),
            claimsPrincipalSubstitute,
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

        var dfePublicApiSubstitute = Substitute.For<IDfePublicApi>();
        var commandSubstitute = Substitute.For<IRecordUserSignInCommand>();
        var dbUserIdValue = 1234;
        var dbEstablishmentIdValue = 5678;
        commandSubstitute.RecordSignIn(Arg.Any<RecordUserSignInDto>()).Returns(new Domain.SignIns.Models.SignIn()
        {
            UserId = dbUserIdValue,
            EstablishmentId = dbEstablishmentIdValue,
            SignInDateTime = DateTime.UtcNow,
            Id = 1,
        });

        var userAccessToService = new UserAccessToService()
        {
            UserId = userId,
            OrganisationId = orgId,
            Roles = new List<Role>()
            {
                expectedRole,
                new Role()
                {
                    Status = new Status()
                    {
                        Id = 2
                    },
                    Name = "OtherRole",
                    Code = "OtherCode",
                    NumericId = "OtherNumericId",
                },
                new Role()
                {
                    Status = new Status()
                    {
                        Id = 2
                    },
                    Name = "OtherRole",
                    Code = "plan_tech_for_school_estalishment_only",
                    NumericId = "OtherNumericId",
                }
            }
        };

        dfePublicApiSubstitute.GetUserAccessToService(userId.ToString(), orgId.ToString()).Returns(userAccessToService);

        var identitySubstitute = Substitute.For<ClaimsIdentity>();

        var organisationClaim = new Organisation()
        {
            Id = orgId,
            Name = "Organisation"
        };

        var organisationClaimSerialised = JsonSerializer.Serialize(organisationClaim);

        identitySubstitute.Claims.Returns(new List<Claim>()
        {
            new Claim(ClaimConstants.NameIdentifier, userId.ToString()),
            new Claim(ClaimConstants.Organisation, organisationClaimSerialised),
        });

        identitySubstitute.IsAuthenticated.Returns(true);

        var claimsPrincipal = new ClaimsPrincipal(identitySubstitute);

        var contextSubstitute = Substitute.For<HttpContext>();
        contextSubstitute.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(config);
        contextSubstitute.RequestServices.GetService(typeof(IDfePublicApi)).Returns(dfePublicApiSubstitute);
        contextSubstitute.RequestServices.GetService(typeof(IRecordUserSignInCommand)).Returns(commandSubstitute);

        var context = new UserInformationReceivedContext(contextSubstitute,
            new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
            new OpenIdConnectOptions(),
            claimsPrincipal,
            new AuthenticationProperties());

        await OnUserInformationReceivedEvent.OnUserInformationReceived(context);

        //Assert Service called
        await dfePublicApiSubstitute.Received().GetUserAccessToService(userId.ToString(), orgId.ToString());

        //Assert has right number of identites
        Assert.NotNull(context.Principal);
        var identities = context.Principal.Identities;
        Assert.Equal(3, identities.Count());

        //Assert claims added
        var roleCodeClaim = context.Principal.Claims.FirstOrDefault(c => c.Type == ClaimConstants.RoleCode);
        Assert.NotNull(roleCodeClaim);
        Assert.Equal(expectedRole.Code, roleCodeClaim.Value);

        var roleIdClaim = context.Principal.Claims.FirstOrDefault(c => c.Type == ClaimConstants.RoleId);
        Assert.NotNull(roleIdClaim);
        Assert.Equal(expectedRole.Id.ToString(), roleIdClaim.Value);

        var roleNameClaim = context.Principal.Claims.FirstOrDefault(c => c.Type == ClaimConstants.RoleName);
        Assert.NotNull(roleNameClaim);
        Assert.Equal(expectedRole.Name, roleNameClaim.Value);

        var roleNumericIdClaim = context.Principal.Claims.FirstOrDefault(c => c.Type == ClaimConstants.RoleNumericId);
        Assert.NotNull(roleNumericIdClaim);
        Assert.Equal(expectedRole.NumericId, roleNumericIdClaim.Value);

        //Has DB User + Establishment Ids
        var dbUserId = context.Principal.Claims.FirstOrDefault(claim => claim.Type == ClaimConstants.DB_USER_ID);
        Assert.NotNull(dbUserId);
        Assert.Equal(dbUserIdValue, int.Parse(dbUserId.Value));

        var dbEstablishmentId =
            context.Principal.Claims.FirstOrDefault(claim => claim.Type == ClaimConstants.DB_ESTABLISHMENT_ID);
        Assert.NotNull(dbEstablishmentId);
        Assert.Equal(dbEstablishmentIdValue, int.Parse(dbEstablishmentId.Value));
    }

    [Fact]
    public async Task OnTokenValidated_Should_AddRoles_OnTokenValidated_When_DiscoverRolesWithPublicApi_Is_False()
    {
        var wasCalled = false;

        var config = new DfeSignInConfiguration()
        {
            DiscoverRolesWithPublicApi = false
        };

        var dfePublicApiSubstitute = Substitute.For<IDfePublicApi>();

        var claimsPrincipalSubstitute = Substitute.For<ClaimsPrincipal>();
        claimsPrincipalSubstitute.Identity.Returns((callInfo) =>
        {
            wasCalled = false;
            return null;
        });

        var contextSubstitute = Substitute.For<HttpContext>();
        contextSubstitute.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(config);
        contextSubstitute.RequestServices.GetService(typeof(IDfePublicApi)).Returns(dfePublicApiSubstitute);

        var context = new UserInformationReceivedContext(contextSubstitute,
            new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
            new OpenIdConnectOptions(),
            claimsPrincipalSubstitute,
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

        var contextSubstitute = Substitute.For<HttpContext>();
        contextSubstitute.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(config);

        var context = new RedirectContext(contextSubstitute,
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

        var contextSubstitute = Substitute.For<HttpContext>();
        contextSubstitute.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(config);

        var context = new RedirectContext(contextSubstitute,
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


    [Fact]
    public async Task The_Correct_Role_Does_Not_Exist()
    {
        Guid userId = Guid.NewGuid();
        Guid orgId = Guid.NewGuid();

        var dfePublicApiSubstitute = Substitute.For<IDfePublicApi>();
        var commandSubstitute = Substitute.For<IRecordUserSignInCommand>();
        var dbUserIdValue = 1234;
        var dbEstablishmentIdValue = 5678;
        commandSubstitute.RecordSignIn(Arg.Any<RecordUserSignInDto>()).Returns(new Domain.SignIns.Models.SignIn()
        {
            UserId = dbUserIdValue,
            EstablishmentId = dbEstablishmentIdValue,
            SignInDateTime = DateTime.UtcNow,
            Id = 1,
        });

        var userAccessToService = new UserAccessToService()
        {
            UserId = userId,
            OrganisationId = orgId,
            Roles = new List<Role>()
            {
                new Role()
                {
                    Status = new Status()
                    {
                        Id = 2
                    },
                    Name = "OtherRole",
                    Code = "OtherCode",
                    NumericId = "OtherNumericId",
                }
            }
        };

        dfePublicApiSubstitute.GetUserAccessToService(userId.ToString(), orgId.ToString()).Returns(userAccessToService);

        var identitySubstitute = Substitute.For<ClaimsIdentity>();

        var organisationClaim = new Organisation()
        {
            Id = orgId,
            Name = "Organisation"
        };

        var organisationClaimSerialised = JsonSerializer.Serialize(organisationClaim);

        _ = identitySubstitute.Claims.Returns(new List<Claim>()
        {
            new(ClaimConstants.NameIdentifier, userId.ToString()),
            new Claim(ClaimConstants.Organisation, organisationClaimSerialised),
        });

        identitySubstitute.IsAuthenticated.Returns(true);

        var claimsPrincipal = new ClaimsPrincipal(identitySubstitute);
        var configMock = Substitute.For<IDfeSignInConfiguration>();
        configMock.DiscoverRolesWithPublicApi = true;

        var contextSubstitute = Substitute.For<HttpContext>();
        contextSubstitute.RequestServices.GetService(typeof(IDfePublicApi)).Returns(dfePublicApiSubstitute);
        contextSubstitute.RequestServices.GetService(typeof(IRecordUserSignInCommand)).Returns(commandSubstitute);
        contextSubstitute.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(configMock);

        var context = new UserInformationReceivedContext(contextSubstitute,
            new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
            new OpenIdConnectOptions(),
            claimsPrincipal,
            new AuthenticationProperties());

        await Assert.ThrowsAnyAsync<UserAccessRoleNotFoundException>(() => OnUserInformationReceivedEvent.OnUserInformationReceived(context));
    }


    [Fact]
    public async Task The_Users_Organisation_Does_Not_Exist()
    {
        Guid userId = Guid.NewGuid();

        var identitySubstitute = Substitute.For<ClaimsIdentity>();


        identitySubstitute.Claims.Returns(new List<Claim>()
        {
            new Claim(ClaimConstants.NameIdentifier, userId.ToString()),
        });

        identitySubstitute.IsAuthenticated.Returns(true);

        var claimsPrincipal = new ClaimsPrincipal(identitySubstitute);

        var contextSubstitute = Substitute.For<HttpContext>();

        var context = new UserInformationReceivedContext(contextSubstitute,
            new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
            new OpenIdConnectOptions(),
            claimsPrincipal,
            new AuthenticationProperties());

        await Assert.ThrowsAnyAsync<KeyNotFoundException>(() =>
            OnUserInformationReceivedEvent.OnUserInformationReceived(context));
    }

    [Fact]
    public void GetOriginUrl_Should_Return_XForwardedHost_If_Found()
    {
        var host = "www.should-return-this.com";

        var httpContext = Substitute.For<HttpContext>();
        var context = new RedirectContext(httpContext, new AuthenticationScheme("", "", typeof(DummyAuthHandler)), new OpenIdConnectOptions(), new AuthenticationProperties() { });

        var request = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary
        {
            { "X-Forwarded-Host", host }
        };

        request.Headers.Returns(headers);
        context.Request.Returns(request);
        var originUrl = DfeOpenIdConnectEvents.GetOriginUrl(context, new DfeSignInConfiguration());

        Assert.Contains(host, originUrl);
    }

    [Fact]
    public void GetOriginUrl_Should_Return_FrontDoorURL_If_Header_Not_Found()
    {
        var host = "www.should-return-this.com";

        var httpContext = Substitute.For<HttpContext>();
        var context = new RedirectContext(httpContext, new AuthenticationScheme("", "", typeof(DummyAuthHandler)), new OpenIdConnectOptions(), new AuthenticationProperties() { });

        var request = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary
        {
        };

        request.Headers.Returns(headers);
        context.Request.Returns(request);

        var config = new DfeSignInConfiguration()
        {
            FrontDoorUrl = host
        };

        var originUrl = DfeOpenIdConnectEvents.GetOriginUrl(context, config);

        Assert.Contains(host, originUrl);
    }
}
