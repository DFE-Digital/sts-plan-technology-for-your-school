using System.Globalization;
using System.Text.Json;
using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Models;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class CookieServiceTests
{
    private static (CookieService cookieService, DefaultHttpContext httpContext, ICookieWorkflow cookieWorkflow) Build(DfeCookieModel? cookieModel = null)
    {
        var httpContext = new DefaultHttpContext();

        if (cookieModel is not null)
        {
            var json = JsonSerializer.Serialize(cookieModel);
            var encoded = Uri.EscapeDataString(json);
            httpContext.Request.Headers["Cookie"] = $"{CookieService.CookieKey}={encoded}";
        }

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        var cookieWorkflow = Substitute.For<ICookieWorkflow>();
        var cookieService = new CookieService(accessor, cookieWorkflow);
        return (cookieService, httpContext, cookieWorkflow);
    }

    private static string[] GetSetCookie(DefaultHttpContext httpContext) => httpContext.Response.Headers.SetCookie
        .Where(stringValue => !string.IsNullOrWhiteSpace(stringValue))
        .Cast<string>()
        .ToArray();

    private static DfeCookieModel GetLatestModelFromHeaders(DefaultHttpContext httpContext)
    {
        var setCookies = GetSetCookie(httpContext);
        var header = setCookies.Last(s => s.Contains($"{CookieService.CookieKey}="));
        var start = header.IndexOf($"{CookieService.CookieKey}=", StringComparison.Ordinal);
        var valueIndex = start + CookieService.CookieKey.Length + 1;
        var end = header.IndexOf(';', valueIndex);
        var encoded = header.Substring(valueIndex, end - valueIndex);
        var json = Uri.UnescapeDataString(encoded);
        return JsonSerializer.Deserialize<DfeCookieModel>(json)!;
    }

    private static DateTimeOffset? GetExpires(DefaultHttpContext httpContext)
    {
        var header = GetSetCookie(httpContext).Last(s => s.Contains($"{CookieService.CookieKey}="));
        var expiresPart = header.Split(';').FirstOrDefault(p => p.TrimStart().StartsWith("expires=", StringComparison.InvariantCultureIgnoreCase));
        if (expiresPart is null)
        {
            return null;
        }

        var value = expiresPart.Split('=', 2)[1].Trim();
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dto)
            ? dto
            : null;
    }

    [Fact]
    public void Cookie_NoExisting_Returns_DefaultModel()
    {
        var (cookieService, _, _) = Build(null);
        var model = cookieService.Cookie;

        var expectedValue = JsonSerializer.Serialize(new DfeCookieModel());
        Assert.Equal(expectedValue, JsonSerializer.Serialize(model));
    }

    [Fact]
    public void GetCookie_Deserializes_Existing_From_Header()
    {
        var existing = new DfeCookieModel { UserAcceptsCookies = true, IsVisible = true };
        var (cookieService, _, _) = Build(existing);

        var model = cookieService.GetCookie();

        Assert.True(model.UserAcceptsCookies);
        Assert.True(model.IsVisible);
    }

    [Fact]
    public void SetVisibility_Writes_Delete_Then_Append_With_OneYearExpiry_And_Preserves_Acceptance()
    {
        var existing = new DfeCookieModel { UserAcceptsCookies = true, IsVisible = false };
        var (cookieService, httpContext, _) = Build(existing);

        cookieService.SetVisibility(true);

        var headers = GetSetCookie(httpContext);
        Assert.Equal(2, headers.Length);
        Assert.All(headers, header => Assert.Contains(CookieService.CookieKey, header, StringComparison.Ordinal));

        var last = headers.Last();
        Assert.Contains("httponly", last, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("secure", last, StringComparison.InvariantCultureIgnoreCase);

        var exp = GetExpires(httpContext);
        Assert.NotNull(exp);
        Assert.InRange(exp!.Value.UtcDateTime,
                       DateTime.UtcNow.AddMonths(11),
                       DateTime.UtcNow.AddYears(1).AddDays(7));

        var written = GetLatestModelFromHeaders(httpContext);
        Assert.True(written.IsVisible);
        Assert.True(written.UserAcceptsCookies);
    }

    [Fact]
    public void SetCookieAcceptance_Throws_When_HttpContext_Null()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var cookieWorkflow = Substitute.For<ICookieWorkflow>();
        var cookieService = new CookieService(accessor, cookieWorkflow);

        Assert.Throws<InvalidOperationException>(() => cookieService.SetCookieAcceptance(true));
    }

    [Fact]
    public void SetCookieAcceptance_True_Writes_Cookie_And_DoesNot_Remove_NonEssential()
    {
        var existing = new DfeCookieModel { UserAcceptsCookies = null, IsVisible = false };
        var (cookieService, httpContext, cookieWorkflow) = Build(existing);

        cookieService.SetCookieAcceptance(true);

        var headers = GetSetCookie(httpContext);
        Assert.Equal(2, headers.Length);

        var model = GetLatestModelFromHeaders(httpContext);
        Assert.True(model.UserAcceptsCookies);
        Assert.False(model.IsVisible);

        cookieWorkflow.DidNotReceiveWithAnyArgs().RemoveNonEssentialCookies(default!);
    }

    [Fact]
    public void SetCookieAcceptance_False_Writes_Cookie_And_Removes_NonEssential()
    {
        var existing = new DfeCookieModel { UserAcceptsCookies = true, IsVisible = true };
        var (cookieService, httpContext, cookieWorkflow) = Build(existing);

        cookieService.SetCookieAcceptance(false);

        var headers = GetSetCookie(httpContext);
        Assert.Equal(2, headers.Length);

        var model = GetLatestModelFromHeaders(httpContext);
        Assert.False(model.UserAcceptsCookies);
        Assert.True(model.IsVisible);

        cookieWorkflow.Received(1).RemoveNonEssentialCookies(httpContext);
    }

    [Fact]
    public void Merge_Semantics_Across_Calls_Are_Correct()
    {
        var existing = new DfeCookieModel { UserAcceptsCookies = false, IsVisible = true };
        var (cookieService, httpContext, _) = Build(existing);

        cookieService.SetVisibility(false);
        var model1 = GetLatestModelFromHeaders(httpContext);
        Assert.False(model1.IsVisible);
        Assert.False(model1.UserAcceptsCookies);
        Assert.Equal(2, GetSetCookie(httpContext).Length);

        httpContext.Response.Headers.Clear();

        cookieService.SetCookieAcceptance(true);
        var model2 = GetLatestModelFromHeaders(httpContext);
        Assert.False(model2.IsVisible);
        Assert.True(model2.UserAcceptsCookies);

        Assert.Equal(2, GetSetCookie(httpContext).Length);
    }
}
