using System.Linq.Expressions;
using Dfe.PlanTech.Core.Options;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class MockAuthControllerTests
{
    private const string ValidSecret = "test-secret";
    private const string SelectorCookieName = "e2e_user";
    private const string SelectorCookieKey = "e2e_key";

    private readonly IEstablishmentRepository _establishmentRepository = Substitute.For<IEstablishmentRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public void Discovery_ReturnsExpectedConfiguration()
    {
        var controller = CreateController();

        var result = controller.Discovery();

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value!;

        Assert.Equal("https://localhost:8080/api/mock-auth", GetProperty<string>(value, "issuer"));
        Assert.Equal("https://localhost:8080/api/mock-auth/authorize", GetProperty<string>(value, "authorization_endpoint"));
        Assert.Equal("https://localhost:8080/api/mock-auth/token", GetProperty<string>(value, "token_endpoint"));
        Assert.Equal("https://localhost:8080/api/mock-auth/jwks", GetProperty<string>(value, "jwks_uri"));
        Assert.Equal("https://localhost:8080/api/mock-auth/endsession", GetProperty<string>(value, "end_session_endpoint"));
    }

    [Fact]
    public void Jwks_ReturnsSingleKey()
    {
        var controller = CreateController();

        var result = controller.Jwks();

        var ok = Assert.IsType<OkObjectResult>(result);
        var keys = GetProperty<Array>(ok.Value!, "keys");

        Assert.NotNull(keys);
        Assert.Single(keys);
    }

    [Fact]
    public async Task Authorize_ReturnsNotFound_WhenKeyCookieMissing()
    {
        var controller = CreateController(keyCookie: null);

        var result = await controller.Authorize(
            "https://localhost:8080/auth/cb",
            "code",
            "state-1",
            "nonce-1");

        Assert.IsType<string>("The service is currently unavailable to use while E2E tests are running.");
    }

    [Fact]
    public async Task Authorize_ReturnsNotFound_WhenKeyCookieInvalid()
    {
        var controller = CreateController(keyCookie: "wrong");

        var result = await controller.Authorize(
            "https://localhost:8080/auth/cb",
            "code",
            "state-1",
            "nonce-1");

        Assert.IsType<string>("The service is currently unavailable to use while E2E tests are running.");
    }

    [Fact]
    public async Task Authorize_ReturnsBadRequest_WhenResponseTypeInvalid()
    {
        var controller = CreateController();

        var result = await controller.Authorize(
            "https://localhost:8080/auth/cb",
            "token",
            "state-1",
            "nonce-1");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Only authorization_code supported", badRequest.Value);
    }

    [Fact]
    public async Task Authorize_Redirects_WithCodeAndState_ForSchool()
    {
        SetupSchoolAndMatData();

        var controller = CreateController(userTypeCookie: "school");

        var result = await controller.Authorize(
            "https://localhost:8080/auth/cb",
            "code",
            "state-123",
            "nonce-123");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.StartsWith("https://localhost:8080/auth/cb?code=", redirect.Url);
        Assert.Contains("&state=state-123", redirect.Url);
    }

    [Fact]
    public async Task Authorize_Throws_WhenSchoolEstablishmentMissing()
    {
        _establishmentRepository
            .GetEstablishmentsByAsync(Arg.Any<Expression<Func<EstablishmentEntity, bool>>>())
            .Returns(c =>
            {
                var result = new List<EstablishmentEntity>();
                return Task.FromResult(result);
            });

        _userRepository.GetUserBySignInRefAsync("E2E_TEST_SCHOOL_USER")
            .Returns(Task.FromResult<UserEntity?>(new UserEntity { Id = 11 }));

        _userRepository.GetUserBySignInRefAsync("E2E_TEST_MAT_USER")
            .Returns(Task.FromResult<UserEntity?>(new UserEntity { Id = 22 }));

        var controller = CreateController();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            controller.Authorize(
                "https://localhost:8080/auth/cb",
                "code",
                "state-1",
                "nonce-1"));
    }

    [Fact]
    public async Task Authorize_UsesSpecificOrganisationLookups()
    {
        EstablishmentEntity[] establishments =
        [
            new() { Id = 100, OrgName = "Other Org" },
            new() { Id = 101, OrgName = "DSI TEST Establishment (001) Miscellanenous (27)" },
            new() { Id = 201, OrgName = "DSI TEST Multi-Academy Trust (010)" }
        ];

        _establishmentRepository
            .GetEstablishmentsByAsync(Arg.Any<Expression<Func<EstablishmentEntity, bool>>>())
            .Returns(callInfo =>
            {
                Expression<Func<EstablishmentEntity, bool>> expression =
                    callInfo.Arg<Expression<Func<EstablishmentEntity, bool>>>();

                Func<EstablishmentEntity, bool> predicate = expression.Compile();

                List<EstablishmentEntity> result = establishments
                    .Where(predicate)
                    .ToList();

                return Task.FromResult(result);
            });

        _userRepository.GetUserBySignInRefAsync("E2E_TEST_SCHOOL_USER")
            .Returns(Task.FromResult<UserEntity?>(new UserEntity() { Id = 11 }));

        _userRepository.GetUserBySignInRefAsync("E2E_TEST_MAT_USER")
            .Returns(Task.FromResult<UserEntity?>(new UserEntity { Id = 22 }));

        var controller = CreateController(userTypeCookie: "school");

        var result = await controller.Authorize(
            "https://localhost:8080/auth/cb",
            "code",
            "state-1",
            "nonce-1");

        Assert.IsType<RedirectResult>(result);

        await _establishmentRepository.Received(2)
            .GetEstablishmentsByAsync(Arg.Any<Expression<Func<EstablishmentEntity, bool>>>());

        await _userRepository.Received(1).GetUserBySignInRefAsync("E2E_TEST_SCHOOL_USER");
        await _userRepository.Received(1).GetUserBySignInRefAsync("E2E_TEST_MAT_USER");
    }

    [Fact]
    public async Task Authorize_AllowsRequest_WhenCorrectSecretCookieProvided()
    {
        SetupSchoolAndMatData();

        var controller = CreateController(
            userTypeCookie: "school",
            keyCookie: ValidSecret);

        var result = await controller.Authorize(
            "https://localhost:8080/auth/cb",
            "code",
            "state-1",
            "nonce-1");

        Assert.IsType<RedirectResult>(result);
    }

    [Fact]
    public async Task Authorize_ReturnsNotFoundResult_WhenIncorrectSecretCookieProvided()
    {
        SetupSchoolAndMatData();

        var controller = CreateController(
            userTypeCookie: "school",
            keyCookie: "Invalid-Secret");

        var result = await controller.Authorize(
            "https://localhost:8080/auth/cb",
            "code",
            "state-1",
            "nonce-1");

        Assert.IsType<string>("The service is currently unavailable to use while E2E tests are running.");
    }

    [Fact]
    public void Token_ReturnsBadRequest_WhenGrantTypeInvalid()
    {
        var controller = CreateController();

        var result = controller.Token(
            "implicit",
            "code-1",
            "https://localhost:8080/callback",
            "client-id");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("unsupported_grant_type", GetProperty<string>(badRequest.Value!, "error"));
    }

    [Fact]
    public void Token_ReturnsBadRequest_WhenCodeMissing()
    {
        var controller = CreateController();

        var result = controller.Token(
            "authorization_code",
            "missing-code",
            "https://localhost:8080/callback",
            "client-id");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("invalid_grant", GetProperty<string>(badRequest.Value!, "error"));
    }

    [Fact]
    public void Token_ReturnsBadRequest_WhenRedirectUriMismatch()
    {
        var controller = CreateController();

        StoreAuthCode("code-1", new AuthCodeRecordBuilder()
            .WithRedirectUri("https://localhost:8080/correct")
            .Build());

        var result = controller.Token(
            "authorization_code",
            "code-1",
            "https://localhost:8080/wrong",
            "client-id");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("redirect_mismatch", GetProperty<string>(badRequest.Value!, "error"));
    }

    [Fact]
    public void Token_ReturnsBadRequest_WhenExpired()
    {
        var controller = CreateController();

        StoreAuthCode("code-1", new AuthCodeRecordBuilder()
            .WithExpires(DateTime.UtcNow.AddMinutes(-1))
            .Build());

        var result = controller.Token(
            "authorization_code",
            "code-1",
            "https://localhost:8080/callback",
            "client-id");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("expired_code", GetProperty<string>(badRequest.Value!, "error"));
    }

    [Fact]
    public void Token_ReturnsTokens_WhenCodeValid()
    {
        var controller = CreateController();

        StoreAuthCode("code-1", new AuthCodeRecordBuilder().Build());

        var result = controller.Token(
            "authorization_code",
            "code-1",
            "https://localhost:8080/callback",
            "client-id");

        var ok = Assert.IsType<OkObjectResult>(result);

        Assert.False(string.IsNullOrWhiteSpace(GetProperty<string>(ok.Value!, "access_token")));
        Assert.False(string.IsNullOrWhiteSpace(GetProperty<string>(ok.Value!, "id_token")));
        Assert.Equal("Bearer", GetProperty<string>(ok.Value!, "token_type"));
        Assert.Equal(1800, GetProperty<int>(ok.Value!, "expires_in"));
    }

    [Fact]
    public void Token_CannotReuseCode()
    {
        var controller = CreateController();

        StoreAuthCode("code-1", new AuthCodeRecordBuilder().Build());

        var first = controller.Token(
            "authorization_code",
            "code-1",
            "https://localhost:8080/callback",
            "client-id");

        Assert.IsType<OkObjectResult>(first);

        var second = controller.Token(
            "authorization_code",
            "code-1",
            "https://localhost:8080/callback",
            "client-id");

        var badRequest = Assert.IsType<BadRequestObjectResult>(second);
        Assert.Equal("invalid_grant", GetProperty<string>(badRequest.Value!, "error"));
    }

    [Fact]
    public void EndSession_ReturnsNotFound_WhenNotAllowed()
    {
        var controller = CreateController(keyCookie: null);

        var result = controller.EndSession( "abc");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void EndSession_RedirectsToDefaultUri()
    {
        var controller = CreateController();

        var result = controller.EndSession( null);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/", redirect.Url);
    }

    [Fact]
    public void EndSession_AppendsState()
    {
        var controller = CreateController();

        var result = controller.EndSession("state-1");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/?state=state-1", redirect.Url);
    }

    [Fact]
    public void EndSession_AppendsStateToExistingQuery()
    {
        var controller = CreateController();

        var result = controller.EndSession("state-1");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/?state=state-1", redirect.Url);
    }

    private MockAuthController CreateController(
        string? userTypeCookie = "school",
        string? keyCookie = ValidSecret,
        string scheme = "https",
        string host = "localhost:8080")
    {
        var options = Options.Create(new AutomatedTestingOptions
        {
            MockAuthentication = new MockAuthenticationOptions
            {
                ClientSecret = ValidSecret
            }
        });

        var controller = new MockAuthController(
            _establishmentRepository,
            _userRepository,
            _cache,
            options);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = scheme;
        httpContext.Request.Host = new HostString(host);

        var cookies = new List<string>();

        if (userTypeCookie is not null)
        {
            cookies.Add($"{SelectorCookieName}={userTypeCookie}");
        }

        if (keyCookie is not null)
        {
            cookies.Add($"{SelectorCookieKey}={keyCookie}");
        }

        if (cookies.Count > 0)
        {
            httpContext.Request.Headers.Cookie = string.Join("; ", cookies);
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    private void SetupSchoolAndMatData()
    {
        EstablishmentEntity[] establishments =
        [
            new EstablishmentEntity { Id = 101, OrgName = "DSI TEST Establishment (001) Miscellanenous (27)" },
            new EstablishmentEntity { Id = 201, OrgName = "DSI TEST Multi-Academy Trust (010)" },
            new EstablishmentEntity { Id = 999, OrgName = "Other Org" }
        ];

        _establishmentRepository
            .GetEstablishmentsByAsync(Arg.Any<Expression<Func<EstablishmentEntity, bool>>>())
            .Returns(callInfo =>
            {
                Expression<Func<EstablishmentEntity, bool>> expression =
                    callInfo.Arg<Expression<Func<EstablishmentEntity, bool>>>();

                Func<EstablishmentEntity, bool> predicate = expression.Compile();

                List<EstablishmentEntity> result = establishments
                    .Where(predicate)
                    .ToList();

                return Task.FromResult(result);
            });

        _userRepository.GetUserBySignInRefAsync("E2E_TEST_SCHOOL_USER")
            .Returns(Task.FromResult<UserEntity?>(new UserEntity { Id = 11 }));

        _userRepository.GetUserBySignInRefAsync("E2E_TEST_MAT_USER")
            .Returns(Task.FromResult<UserEntity?>(new UserEntity { Id = 22 }));
    }

    private void StoreAuthCode(string code, object record)
    {
        _cache.Set(code, record, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.UtcNow.AddMinutes(2)
        });
    }

    private static T GetProperty<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName);
        Assert.NotNull(property);
        return (T)property!.GetValue(obj)!;
    }

    private class AuthCodeRecordBuilder
    {
        private string _subject = "E2E_TEST_SCHOOL_USER";
        private string _email = "school@test.local";
        private string? _organisationJson = "{\"name\":\"School\"}";
        private string _redirectUri = "https://localhost:8080/callback";
        private string? _nonce = "nonce-1";
        private DateTime _expires = DateTime.UtcNow.AddMinutes(1);
        private int _dbEstablishmentId = 101;
        private int _dbUserId = 11;
        private int? _dbMatEstablishmentId;

        public AuthCodeRecordBuilder WithRedirectUri(string redirectUri)
        {
            _redirectUri = redirectUri;
            return this;
        }

        public AuthCodeRecordBuilder WithExpires(DateTime expires)
        {
            _expires = expires;
            return this;
        }

        public object Build()
        {
            var type = typeof(MockAuthController)
                .GetNestedType("AuthCodeRecord", System.Reflection.BindingFlags.NonPublic)!;

            var instance = Activator.CreateInstance(type)!;

            type.GetProperty("Subject")!.SetValue(instance, _subject);
            type.GetProperty("Email")!.SetValue(instance, _email);
            type.GetProperty("OrganisationJson")!.SetValue(instance, _organisationJson);
            type.GetProperty("RedirectUri")!.SetValue(instance, _redirectUri);
            type.GetProperty("Nonce")!.SetValue(instance, _nonce);
            type.GetProperty("Expires")!.SetValue(instance, _expires);
            type.GetProperty("DbEstablishmentId")!.SetValue(instance, _dbEstablishmentId);
            type.GetProperty("DbUserId")!.SetValue(instance, _dbUserId);
            type.GetProperty("DbMatEstablishmentId")!.SetValue(instance, _dbMatEstablishmentId);

            return instance;
        }
    }
}
