using System.Security.Claims;
using System.Text.RegularExpressions;
using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Infrastructure.SignIn.ConnectEvents;
using Dfe.PlanTech.UnitTests.Shared.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.SignIn.UnitTests;

public partial class DfeOpenIdConnectEventsTests
{
    [GeneratedRegex(SchemeMatchRegexPattern)]
    private static partial Regex SchemeMatchRegexAttribute();

    public const string SchemeMatchRegexPattern = @"^(https?:\/\/)";
    public readonly static Regex SchemeMatchRegex = SchemeMatchRegexAttribute();

    [Fact]
    public async Task OnRedirectToIdentityProvider_Should_Rewrite_Url_Always()
    {
        var config = new DfeSignInConfiguration()
        {
            CallbackUrl = "/auth/cb",
            FrontDoorUrl = "www.frontdoorurl.com"
        };

        var contextSubstitute = Substitute.For<HttpContext>();
        contextSubstitute.RequestServices.GetService(typeof(DfeSignInConfiguration)).Returns(config);

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
        contextSubstitute.RequestServices.GetService(typeof(DfeSignInConfiguration)).Returns(config);

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
    public async Task RecordUserSignIn_Should_LogError_When_OrgDoesNotExist()
    {
        Guid userId = Guid.NewGuid();

        var identitySubstitute = Substitute.For<ClaimsIdentity>();
        identitySubstitute.Claims.Returns([new Claim(ClaimConstants.NameIdentifier, userId.ToString()),]);
        identitySubstitute.IsAuthenticated.Returns(true);

        var user = new UserEntity
        {
            Id = 2,
            DfeSignInRef = "testSignInRef",
            DateCreated = DateTime.UtcNow,
            DateLastUpdated = DateTime.UtcNow,
            Responses = []
        };

        var signIn = new SignInEntity
        {
            Id = 3,
            UserId = 2,
            EstablishmentId = 3,
            SignInDateTime = DateTime.UtcNow,
            User = user
        };
        var establishmentRepositorySubstitute = Substitute.For<IEstablishmentRepository>();

        var userRepositorySubstitute = Substitute.For<IUserRepository>();
        userRepositorySubstitute.CreateUserBySignInRefAsync(Arg.Any<string>()).Returns(user);

        var signInRepositorySubstitute = Substitute.For<ISignInRepository>();
        signInRepositorySubstitute.CreateSignInAsync(Arg.Any<int>()).Returns(signIn);

        var signInWorkflowSubstitute = Substitute.For<SignInWorkflow>(establishmentRepositorySubstitute, signInRepositorySubstitute, userRepositorySubstitute);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(ISignInWorkflow)).Returns(signInWorkflowSubstitute);

        var contextSubstitute = Substitute.For<HttpContext>();
        contextSubstitute.RequestServices.Returns(serviceProvider);

        var claimsPrincipal = new ClaimsPrincipal(identitySubstitute);
        var context = new UserInformationReceivedContext(contextSubstitute,
            new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
            new OpenIdConnectOptions(),
            claimsPrincipal,
            new AuthenticationProperties());

        var logger = Substitute.For<ILogger<DfeSignIn>>();
        await OnUserInformationReceivedEvent.RecordUserSignIn(logger, context);

        Assert.Single(logger.GetMatchingReceivedMessages($"User {userId} is authenticated but has no establishment", LogLevel.Warning));
        await signInWorkflowSubstitute.Received(1).RecordSignInUserOnly(Arg.Any<string>());
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
