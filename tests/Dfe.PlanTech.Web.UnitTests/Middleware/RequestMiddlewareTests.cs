using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Middleware;

public class RequestMiddlewareTests
{
    private readonly IRedirectProvider _redirectProvider;
    private readonly HttpContext _httpContext;

    private static async Task Next(HttpContext hc)
    {
        hc.Response.StatusCode = StatusCodes.Status200OK;
        await hc.Response.WriteAsync("Response Body");
    }

    public RequestMiddlewareTests()
    {
        _redirectProvider = Substitute.For<IRedirectProvider>();
        _httpContext = Substitute.For<HttpContext>();

        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();

        _httpContext
            .RequestServices.GetService(typeof(IServiceScopeFactory))
            .Returns(serviceScopeFactory);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(serviceProvider);

        serviceProvider.GetService(typeof(IRedirectProvider)).Returns(_redirectProvider);

        _httpContext
            .RequestServices.GetService(typeof(IRedirectProvider))
            .Returns(_redirectProvider);

        var asyncServiceScope = new AsyncServiceScope(serviceScope);
        serviceScopeFactory.CreateAsyncScope().Returns(asyncServiceScope);
    }

    [Fact]
    public async Task Head_Requests_Should_Return_200_With_No_Body()
    {
        var context = new DefaultHttpContext()
        {
            Request = { Method = HttpMethods.Head },
            Response = { Body = new MemoryStream() },
        };

        _httpContext.Request.Returns(context.Request);
        _httpContext.Response.Returns(context.Response);

        var middleware = new RequestMiddleware(Next);
        await middleware.InvokeAsync(_httpContext);

        Assert.Equal(200, _httpContext.Response.StatusCode);
        Assert.Equal(0, _httpContext.Response.Body.Length);
        Assert.Same(Stream.Null, _httpContext.Response.Body);
    }

    [Fact]
    public async Task Get_Requests_Should_Not_Have_Body_Removed()
    {
        var context = new DefaultHttpContext()
        {
            Request = { Method = HttpMethods.Get },
            Response = { Body = new MemoryStream() },
        };

        _httpContext.Request.Returns(context.Request);
        _httpContext.Response.Returns(context.Response);

        var middleware = new RequestMiddleware(Next);
        await middleware.InvokeAsync(_httpContext);

        Assert.Equal(200, _httpContext.Response.StatusCode);
        Assert.NotEqual(0, _httpContext.Response.Body.Length);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync(
            TestContext.Current.CancellationToken
        );

        Assert.Equal("Response Body", response);
    }

    [Fact]
    public async Task Should_Not_Call_RedirectProvider_For_Empty_Path()
    {
        var context = new DefaultHttpContext()
        {
            Request = { Method = HttpMethods.Get, Path = "/" },
            Response = { Body = new MemoryStream() },
        };

        _httpContext.Request.Returns(context.Request);
        _httpContext.Response.Returns(context.Response);

        var middleware = new RequestMiddleware(Next);
        await middleware.InvokeAsync(_httpContext);

        _redirectProvider.Received(0).IsStaticPath(Arg.Any<string>());
        Assert.Equal(StatusCodes.Status200OK, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Should_Not_Redirect_For_Static_Paths()
    {
        var context = new DefaultHttpContext()
        {
            Request = { Method = HttpMethods.Get, Path = "/slug" },
            Response = { Body = new MemoryStream() },
        };

        _httpContext.Request.Returns(context.Request);
        _httpContext.Response.Returns(context.Response);

        _redirectProvider.IsStaticPath(Arg.Any<string>()).Returns(true);

        var middleware = new RequestMiddleware(Next);
        await middleware.InvokeAsync(_httpContext);

        Assert.Equal(StatusCodes.Status200OK, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Should_Not_Redirect_When_No_Redirects_Match()
    {
        var context = new DefaultHttpContext()
        {
            Request = { Method = HttpMethods.Get, Path = "/slug" },
            Response = { Body = new MemoryStream() },
        };

        _httpContext.Request.Returns(context.Request);
        _httpContext.Response.Returns(context.Response);

        _redirectProvider.IsStaticPath(Arg.Any<string>()).Returns(false);
        _redirectProvider.TryGetRedirect(Arg.Any<string>()).Returns(default(string?));

        var middleware = new RequestMiddleware(Next);
        await middleware.InvokeAsync(_httpContext);

        Assert.Equal(StatusCodes.Status200OK, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Should_Not_Redirect_When_Redirect_Found()
    {
        var context = new DefaultHttpContext()
        {
            Request = { Method = HttpMethods.Get, Path = "/slug" },
            Response = { Body = new MemoryStream() },
        };

        _httpContext.Request.Returns(context.Request);
        _httpContext.Response.Returns(context.Response);

        var newSlug = "/new-slug";

        _redirectProvider.IsStaticPath(Arg.Any<string>()).Returns(false);
        _redirectProvider.TryGetRedirect(Arg.Any<string>()).Returns(newSlug);

        var middleware = new RequestMiddleware(Next);
        await middleware.InvokeAsync(_httpContext);

        Assert.Equal(StatusCodes.Status301MovedPermanently, _httpContext.Response.StatusCode);
        Assert.Equal(newSlug, _httpContext.Response.Headers.Location);
    }

    [Fact]
    public async Task Should_Add_QueryString_Back_Onto_Path()
    {
        var queryCollection = new Dictionary<string, string?>() { { "thing", "value" } };

        var queryString = QueryString.Create(queryCollection);

        var context = new DefaultHttpContext()
        {
            Request =
            {
                Method = HttpMethods.Get,
                Path = "/slug",
                QueryString = queryString,
            },
            Response = { Body = new MemoryStream() },
        };

        _httpContext.Request.Returns(context.Request);
        _httpContext.Response.Returns(context.Response);

        var newSlug = "/new-slug";

        _redirectProvider.IsStaticPath(Arg.Any<string>()).Returns(false);
        _redirectProvider.TryGetRedirect(Arg.Any<string>()).Returns(newSlug);

        var middleware = new RequestMiddleware(Next);
        await middleware.InvokeAsync(_httpContext);

        Assert.Equal(StatusCodes.Status301MovedPermanently, _httpContext.Response.StatusCode);
        Assert.EndsWith(queryString.ToString(), _httpContext.Response.Headers.Location);
    }
}
