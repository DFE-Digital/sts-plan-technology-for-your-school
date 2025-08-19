namespace Dfe.PlanTech.Infrastructure.SignIns.UnitTests;

public partial class DfeOpenIdConnectEventsTests
{
    //[GeneratedRegex(SchemeMatchRegexPattern)]
    //private static partial Regex SchemeMatchRegexAttribute();

    //public const string SchemeMatchRegexPattern = @"^(https?:\/\/)";
    //public readonly static Regex SchemeMatchRegex = SchemeMatchRegexAttribute();

    //[Fact]
    //public async Task OnRedirectToIdentityProvider_Should_Rewrite_Url_Always()
    //{
    //    var config = new DfeSignInConfiguration()
    //    {
    //        CallbackUrl = "/auth/cb",
    //        FrontDoorUrl = "www.frontdoorurl.com"
    //    };

    //    var contextSubstitute = Substitute.For<HttpContext>();
    //    contextSubstitute.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(config);

    //    var context = new RedirectContext(contextSubstitute,
    //        new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
    //        new OpenIdConnectOptions(),
    //        new AuthenticationProperties());

    //    var openIdConnectMessage = new OpenIdConnectMessage
    //    {
    //        RedirectUri = "https://notarealaddress.ua"
    //    };

    //    context.ProtocolMessage = openIdConnectMessage;

    //    await DfeOpenIdConnectEvents.OnRedirectToIdentityProvider(context);

    //    var expectedUrl = "https://" + config.FrontDoorUrl + config.CallbackUrl;

    //    Assert.Equal(expectedUrl, openIdConnectMessage.RedirectUri);
    //}

    //[Fact]
    //public async Task OnRedirectToIdentityProviderForSignOut_Should_Rewrite_Url_When_ProtocolMessage_Is_NotNull()
    //{
    //    var config = new DfeSignInConfiguration()
    //    {
    //        SignoutRedirectUrl = "/signout/cb",
    //        FrontDoorUrl = "www.frontdoorurl.com"
    //    };

    //    var contextSubstitute = Substitute.For<HttpContext>();
    //    contextSubstitute.RequestServices.GetService(typeof(IDfeSignInConfiguration)).Returns(config);

    //    var context = new RedirectContext(contextSubstitute,
    //        new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
    //        new OpenIdConnectOptions(),
    //        new AuthenticationProperties());

    //    var openIdConnectMessage = new OpenIdConnectMessage
    //    {
    //        PostLogoutRedirectUri = "https://notarealaddress.ua"
    //    };

    //    context.ProtocolMessage = openIdConnectMessage;

    //    await DfeOpenIdConnectEvents.OnRedirectToIdentityProviderForSignOut(context);

    //    var expectedUrl = "https://" + config.FrontDoorUrl + config.SignoutRedirectUrl;

    //    Assert.Equal(expectedUrl, openIdConnectMessage.PostLogoutRedirectUri);
    //}

    //[Fact]
    //public async Task RecordUserSignIn_Should_LogError_When_OrgDoesNotExist()
    //{
    //    Guid userId = Guid.NewGuid();

    //    var identitySubstitute = Substitute.For<ClaimsIdentity>();

    //    identitySubstitute.Claims.Returns([new Claim(ClaimConstants.NameIdentifier, userId.ToString()),]);

    //    identitySubstitute.IsAuthenticated.Returns(true);

    //    var claimsPrincipal = new ClaimsPrincipal(identitySubstitute);

    //    var contextSubstitute = Substitute.For<HttpContext>();
    //    var signInSubstitute = Substitute.For<IRecordUserSignInCommand>();
    //    var serviceProvider = Substitute.For<IServiceProvider>();

    //    contextSubstitute.RequestServices.Returns(serviceProvider);

    //    serviceProvider
    //        .GetService(typeof(IRecordUserSignInCommand))
    //        .Returns(signInSubstitute);

    //    var context = new UserInformationReceivedContext(contextSubstitute,
    //        new AuthenticationScheme("name", "display name", typeof(DummyAuthHandler)),
    //        new OpenIdConnectOptions(),
    //        claimsPrincipal,
    //        new AuthenticationProperties());

    //    var logger = Substitute.For<ILogger<DfeSignIn>>();
    //    await OnUserInformationReceivedEvent.RecordUserSignIn(logger, context);

    //    Assert.Single(logger.GetMatchingReceivedMessages($"User {userId} is authenticated but has no establishment", LogLevel.Warning));
    //    await signInSubstitute.Received(1).RecordSignInUserOnly(Arg.Any<string>());
    //}

    //[Theory]
    //[InlineData("www.should-return-this.com")]
    //[InlineData("www.and-return-this.co.uk")]
    //[InlineData("this-should-also-work.gov.uk")]
    //public void GetOriginUrl_Should_Return_XForwardedHost_If_Found(string host)
    //{
    //    var configUrl = "www.shouldnt-return-this.com";

    //    var httpContext = Substitute.For<HttpContext>();
    //    var context = new RedirectContext(httpContext, new AuthenticationScheme("", "", typeof(DummyAuthHandler)), new OpenIdConnectOptions(), new AuthenticationProperties() { });

    //    var request = Substitute.For<HttpRequest>();
    //    var headers = new HeaderDictionary
    //    {
    //        { "X-Forwarded-Host", host }
    //    };

    //    request.Headers.Returns(headers);
    //    context.Request.Returns(request);

    //    var config = new DfeSignInConfiguration()
    //    {
    //        FrontDoorUrl = configUrl
    //    };

    //    var originUrl = DfeOpenIdConnectEvents.GetOriginUrl(context, config);

    //    Assert.Contains(host, originUrl);
    //}


    //[Theory]
    //[InlineData("www.should-return-this.com")]
    //[InlineData("www.and-return-this.co.uk")]
    //[InlineData("this-should-also-work.gov.uk")]
    //public void GetOriginUrl_Should_Return_FrontDoorURL_If_Header_Not_Found(string host)
    //{
    //    var httpContext = Substitute.For<HttpContext>();
    //    var context = new RedirectContext(httpContext, new AuthenticationScheme("", "", typeof(DummyAuthHandler)), new OpenIdConnectOptions(), new AuthenticationProperties() { });

    //    var request = Substitute.For<HttpRequest>();
    //    var headers = new HeaderDictionary
    //    {
    //    };

    //    request.Headers.Returns(headers);
    //    context.Request.Returns(request);

    //    var config = new DfeSignInConfiguration()
    //    {
    //        FrontDoorUrl = host
    //    };

    //    var originUrl = DfeOpenIdConnectEvents.GetOriginUrl(context, config);

    //    Assert.Contains(host, originUrl);
    //}


    //[Theory]
    //[InlineData("www.should-return-this.com")]
    //[InlineData("www.should-return-this.com/")]
    //public void GetOriginUrl_Should_Return_Append_ForwardSlash_To_FrontDoorUrl_If_Missing(string host)
    //{
    //    var httpContext = Substitute.For<HttpContext>();
    //    var context = new RedirectContext(httpContext, new AuthenticationScheme("", "", typeof(DummyAuthHandler)), new OpenIdConnectOptions(), new AuthenticationProperties() { });

    //    var request = Substitute.For<HttpRequest>();
    //    var headers = new HeaderDictionary
    //    {
    //    };

    //    request.Headers.Returns(headers);
    //    context.Request.Returns(request);

    //    var config = new DfeSignInConfiguration()
    //    {
    //        FrontDoorUrl = host
    //    };

    //    var originUrl = DfeOpenIdConnectEvents.GetOriginUrl(context, config);

    //    Assert.Equal(host, originUrl);
    //}

    //[Theory]
    //[InlineData("www.should-return-this.com")]
    //[InlineData("www.should-return-this.com/")]
    //public void GetOriginUrl_Should_Return_Append_ForwardSlash_To_ForwardedHostHeader_If_Missing(string host)
    //{
    //    var configUrl = "www.shouldnt-return-this.com";

    //    var httpContext = Substitute.For<HttpContext>();
    //    var context = new RedirectContext(httpContext, new AuthenticationScheme("", "", typeof(DummyAuthHandler)), new OpenIdConnectOptions(), new AuthenticationProperties() { });

    //    var request = Substitute.For<HttpRequest>();
    //    var headers = new HeaderDictionary
    //    {
    //        { "X-Forwarded-Host", host }
    //    };

    //    request.Headers.Returns(headers);
    //    context.Request.Returns(request);

    //    var config = new DfeSignInConfiguration()
    //    {
    //        FrontDoorUrl = configUrl
    //    };

    //    var originUrl = DfeOpenIdConnectEvents.GetOriginUrl(context, config);

    //    Assert.Equal(host, originUrl);
    //    Assert.DoesNotContain(configUrl, originUrl);
    //}

    //[Theory]
    //[InlineData("www.plantech.com", "auth/cb")]
    //[InlineData("www.plantech.com", "/auth/cb")]
    //[InlineData("www.plantech.com/", "/auth/cb")]
    //[InlineData("www.plantech.com/", "auth/cb")]
    //[InlineData("plantech.education.gov.uk/", "/auth/cb")]
    //[InlineData("plantech.education.gov.uk", "/auth/cb")]
    //[InlineData("plantech.education.gov.uk/", "auth/cb")]
    //[InlineData("https://plantech.education.gov.uk/", "auth/cb")]
    //[InlineData("https://plantech.education.gov.uk/", "/auth/cb")]
    //[InlineData("http://plantech.education.gov.uk", "auth/cb")]
    //[InlineData("https://plantech.education.gov.uk", "/auth/cb")]
    //[InlineData("https://plantech.education.gov.uk", "auth/cb")]
    //public void CreateCallbackUrl_Should_Return_Correct_Url_For_ForwardedHost_Headers(string host, string callback)
    //{
    //    var configUrl = "www.shouldnt-return-this.com";

    //    var httpContext = Substitute.For<HttpContext>();
    //    var context = new RedirectContext(httpContext, new AuthenticationScheme("", "", typeof(DummyAuthHandler)), new OpenIdConnectOptions(), new AuthenticationProperties() { });

    //    var request = Substitute.For<HttpRequest>();
    //    var headers = new HeaderDictionary
    //    {
    //        { "X-Forwarded-Host", host }
    //    };

    //    request.Headers.Returns(headers);
    //    context.Request.Returns(request);

    //    var config = new DfeSignInConfiguration()
    //    {
    //        FrontDoorUrl = configUrl
    //    };

    //    var originUrl = DfeOpenIdConnectEvents.CreateCallbackUrl(context, config, callback);

    //    ValidateOriginUrlResult(originUrl, host, callback);
    //}

    //[Theory]
    //[InlineData("www.plantech.com", "auth/cb")]
    //[InlineData("www.plantech.com", "/auth/cb")]
    //[InlineData("www.plantech.com/", "/auth/cb")]
    //[InlineData("www.plantech.com/", "auth/cb")]
    //[InlineData("plantech.education.gov.uk/", "/auth/cb")]
    //[InlineData("plantech.education.gov.uk", "/auth/cb")]
    //[InlineData("plantech.education.gov.uk/", "auth/cb")]
    //[InlineData("https://plantech.education.gov.uk/", "auth/cb")]
    //[InlineData("https://plantech.education.gov.uk/", "/auth/cb")]
    //[InlineData("http://plantech.education.gov.uk", "auth/cb")]
    //[InlineData("https://plantech.education.gov.uk", "/auth/cb")]
    //[InlineData("https://plantech.education.gov.uk", "auth/cb")]
    //public void CreateCallbackUrl_Should_Return_Correct_Url_For_FrontdoorUrl(string host, string callback)
    //{
    //    var httpContext = Substitute.For<HttpContext>();
    //    var context = new RedirectContext(httpContext, new AuthenticationScheme("", "", typeof(DummyAuthHandler)), new OpenIdConnectOptions(), new AuthenticationProperties() { });

    //    var request = Substitute.For<HttpRequest>();
    //    var headers = new HeaderDictionary
    //    {
    //    };

    //    request.Headers.Returns(headers);
    //    context.Request.Returns(request);

    //    var config = new DfeSignInConfiguration()
    //    {
    //        FrontDoorUrl = host
    //    };

    //    var originUrl = DfeOpenIdConnectEvents.CreateCallbackUrl(context, config, callback);

    //    ValidateOriginUrlResult(originUrl, host, callback);
    //}

    //private static void ValidateOriginUrlResult(string originUrl, string host, string callbackUrl)
    //{
    //    if (!Uri.TryCreate(originUrl, UriKind.RelativeOrAbsolute, out Uri? uri))
    //    {
    //        Assert.Fail(CreateFailureMessage(originUrl, host, callbackUrl));
    //    }

    //    Assert.NotNull(uri);
    //    Assert.InRange(uri.Scheme, Uri.UriSchemeHttp, Uri.UriSchemeHttps);

    //    string expectedPath = GetExpectedPath(callbackUrl);
    //    Assert.Equal(expectedPath, uri.AbsolutePath);

    //    string expectedHost = GetExpectedHost(host);
    //    Assert.Equal(expectedHost, uri.Host);

    //    Assert.False(uri.AbsolutePath.StartsWith("//"), CreateFailureMessage(originUrl, expectedPath, expectedHost));
    //}

    //private static string GetExpectedPath(string callbackUrl)
    //{
    //    return !callbackUrl.StartsWith('/') ? '/' + callbackUrl : callbackUrl;
    //}

    //private static string GetExpectedHost(string host)
    //{
    //    //Remove scheme from the host
    //    var expectedHost = SchemeMatchRegex.Replace(host, match => "");

    //    //Remove trailing / if present
    //    return expectedHost.EndsWith('/') ? expectedHost[..^1] : expectedHost;
    //}

    //private static string CreateFailureMessage(string originUrl, string host, string callbackUrl)
    //=> $"Result not a valid URI - \"{originUrl}\" from host \"{host}\" and callback url \"{callbackUrl}\".";
}
