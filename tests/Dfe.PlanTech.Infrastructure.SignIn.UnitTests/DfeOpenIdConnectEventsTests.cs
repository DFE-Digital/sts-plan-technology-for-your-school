using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dfe.PlanTech.Application.SignIns.Interfaces;
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

namespace Dfe.PlanTech.Infrastructure.SignIns.UnitTests;

public partial class DfeOpenIdConnectEventsTests
{
    [GeneratedRegex(SchemeMatchRegexPattern)]
    private static partial Regex SchemeMatchRegexAttribute();

    public const string SchemeMatchRegexPattern = @"^(https?:\/\/)";
    public readonly static Regex SchemeMatchRegex = SchemeMatchRegexAttribute();

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

        var expectedUrl = "https://" + config.FrontDoorUrl + config.CallbackUrl;

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

        var expectedUrl = "https://" + config.FrontDoorUrl + config.SignoutRedirectUrl;

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

    [Theory]
    [InlineData("www.should-return-this.com")]
    [InlineData("www.and-return-this.co.uk")]
    [InlineData("this-should-also-work.gov.uk")]
    public void GetOriginUrl_Should_Return_XForwardedHost_If_Found(string host)
    {
        var configUrl = "www.shouldnt-return-this.com";

        var httpContext = Substitute.For<HttpContext>();
        var context = new RedirectContext(httpContext, new AuthenticationScheme("", "", typeof(DummyAuthHandler)), new OpenIdConnectOptions(), new AuthenticationProperties() { });

        var request = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary
        {
            { "X-Forwarded-Host", host }
        };

        request.Headers.Returns(headers);
        context.Request.Returns(request);

        var config = new DfeSignInConfiguration()
        {
            FrontDoorUrl = configUrl
        };

        var originUrl = DfeOpenIdConnectEvents.GetOriginUrl(context, config);

        Assert.Contains(host, originUrl);
    }


    [Theory]
    [InlineData("www.should-return-this.com")]
    [InlineData("www.and-return-this.co.uk")]
    [InlineData("this-should-also-work.gov.uk")]
    public void GetOriginUrl_Should_Return_FrontDoorURL_If_Header_Not_Found(string host)
    {
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


    [Theory]
    [InlineData("www.should-return-this.com")]
    [InlineData("www.should-return-this.com/")]
    public void GetOriginUrl_Should_Return_Append_ForwardSlash_To_FrontDoorUrl_If_Missing(string host)
    {
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

        Assert.Equal(host, originUrl);
    }

    [Theory]
    [InlineData("www.should-return-this.com")]
    [InlineData("www.should-return-this.com/")]
    public void GetOriginUrl_Should_Return_Append_ForwardSlash_To_ForwardedHostHeader_If_Missing(string host)
    {
        var configUrl = "www.shouldnt-return-this.com";

        var httpContext = Substitute.For<HttpContext>();
        var context = new RedirectContext(httpContext, new AuthenticationScheme("", "", typeof(DummyAuthHandler)), new OpenIdConnectOptions(), new AuthenticationProperties() { });

        var request = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary
        {
            { "X-Forwarded-Host", host }
        };

        request.Headers.Returns(headers);
        context.Request.Returns(request);

        var config = new DfeSignInConfiguration()
        {
            FrontDoorUrl = configUrl
        };

        var originUrl = DfeOpenIdConnectEvents.GetOriginUrl(context, config);

        Assert.Equal(host, originUrl);
        Assert.DoesNotContain(configUrl, originUrl);
    }

    [Theory]
    [InlineData("www.plantech.com", "auth/cb")]
    [InlineData("www.plantech.com", "/auth/cb")]
    [InlineData("www.plantech.com/", "/auth/cb")]
    [InlineData("www.plantech.com/", "auth/cb")]
    [InlineData("plantech.education.gov.uk/", "/auth/cb")]
    [InlineData("plantech.education.gov.uk", "/auth/cb")]
    [InlineData("plantech.education.gov.uk/", "auth/cb")]
    [InlineData("https://plantech.education.gov.uk/", "auth/cb")]
    [InlineData("https://plantech.education.gov.uk/", "/auth/cb")]
    [InlineData("http://plantech.education.gov.uk", "auth/cb")]
    [InlineData("https://plantech.education.gov.uk", "/auth/cb")]
    [InlineData("https://plantech.education.gov.uk", "auth/cb")]
    public void CreateCallbackUrl_Should_Return_Correct_Url_For_ForwardedHost_Headers(string host, string callback)
    {
        var configUrl = "www.shouldnt-return-this.com";

        var httpContext = Substitute.For<HttpContext>();
        var context = new RedirectContext(httpContext, new AuthenticationScheme("", "", typeof(DummyAuthHandler)), new OpenIdConnectOptions(), new AuthenticationProperties() { });

        var request = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary
        {
            { "X-Forwarded-Host", host }
        };

        request.Headers.Returns(headers);
        context.Request.Returns(request);

        var config = new DfeSignInConfiguration()
        {
            FrontDoorUrl = configUrl
        };

        var originUrl = DfeOpenIdConnectEvents.CreateCallbackUrl(context, config, callback);

        ValidateOriginUrlResult(originUrl, host, callback);
    }

    [Theory]
    [InlineData("www.plantech.com", "auth/cb")]
    [InlineData("www.plantech.com", "/auth/cb")]
    [InlineData("www.plantech.com/", "/auth/cb")]
    [InlineData("www.plantech.com/", "auth/cb")]
    [InlineData("plantech.education.gov.uk/", "/auth/cb")]
    [InlineData("plantech.education.gov.uk", "/auth/cb")]
    [InlineData("plantech.education.gov.uk/", "auth/cb")]
    [InlineData("https://plantech.education.gov.uk/", "auth/cb")]
    [InlineData("https://plantech.education.gov.uk/", "/auth/cb")]
    [InlineData("http://plantech.education.gov.uk", "auth/cb")]
    [InlineData("https://plantech.education.gov.uk", "/auth/cb")]
    [InlineData("https://plantech.education.gov.uk", "auth/cb")]
    public void CreateCallbackUrl_Should_Return_Correct_Url_For_FrontdoorUrl(string host, string callback)
    {
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

        var originUrl = DfeOpenIdConnectEvents.CreateCallbackUrl(context, config, callback);

        ValidateOriginUrlResult(originUrl, host, callback);
    }

    private static void ValidateOriginUrlResult(string originUrl, string host, string callbackUrl)
    {
        if (!Uri.TryCreate(originUrl, UriKind.RelativeOrAbsolute, out Uri? uri))
        {
            Assert.Fail(CreateFailureMessage(originUrl, host, callbackUrl));
        }

        Assert.NotNull(uri);
        Assert.InRange(uri.Scheme, Uri.UriSchemeHttp, Uri.UriSchemeHttps);

        string expectedPath = GetExpectedPath(callbackUrl);
        Assert.Equal(expectedPath, uri.AbsolutePath);

        string expectedHost = GetExpectedHost(host);
        Assert.Equal(expectedHost, uri.Host);

        Assert.False(uri.AbsolutePath.StartsWith("//"), CreateFailureMessage(originUrl, expectedPath, expectedHost));
    }

    private static string GetExpectedPath(string callbackUrl)
    {
        return !callbackUrl.StartsWith('/') ? '/' + callbackUrl : callbackUrl;
    }

    private static string GetExpectedHost(string host)
    {
        //Remove scheme from the host
        var expectedHost = SchemeMatchRegex.Replace(host, match => "");

        //Remove trailing / if present
        return expectedHost.EndsWith('/') ? expectedHost[..^1] : expectedHost;
    }

    private static string CreateFailureMessage(string originUrl, string host, string callbackUrl)
    => $"Result not a valid URI - \"{originUrl}\" from host \"{host}\" and callback url \"{callbackUrl}\".";
}
