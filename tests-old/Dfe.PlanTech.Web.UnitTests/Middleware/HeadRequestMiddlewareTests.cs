using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Middleware;

public class HeadRequestMiddlewareTests
{
    private static async Task _next(HttpContext hc)
    {
        hc.Response.StatusCode = StatusCodes.Status200OK;
        await hc.Response.WriteAsync("Response Body");
    }

    [Fact]
    public async Task Head_Requests_Should_Return_200_With_No_Body()
    {
        var context = new DefaultHttpContext()
        {
            Request = { Method = HttpMethods.Head },
            Response = { Body = new MemoryStream() }
        };

        var middleware = new HeadRequestMiddleware(_next);
        await middleware.InvokeAsync(context);

        Assert.Equal(200, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
        Assert.Same(Stream.Null, context.Response.Body);
    }

    [Fact]
    public async Task Get_Requests_Should_Not_Have_Body_Removed()
    {
        var context = new DefaultHttpContext()
        {
            Request = { Method = HttpMethods.Get },
            Response = { Body = new MemoryStream() }
        };

        var middleware = new HeadRequestMiddleware(_next);
        await middleware.InvokeAsync(context);

        Assert.Equal(200, context.Response.StatusCode);
        Assert.NotEqual(0, context.Response.Body.Length);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var response = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.Equal("Response Body", response);
    }
}
