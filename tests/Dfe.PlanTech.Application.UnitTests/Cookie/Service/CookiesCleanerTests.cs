using Dfe.PlanTech.Domain.Cookie;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Cookie.Service.Tests;

public class CookiesCleanerTests
{
    [Fact]
    public void RemoveNonEssentialCookies_RemovesNonEssentialCookies()
    {
        var nonEssentialCookieKey = "nonessential_cookie";
        var essentialCookies = new string[] { "essential1", "essential2" };

        var cleaner = new CookiesCleaner(new CookiesCleanerOptions() { EssentialCookies = essentialCookies });
        var context = new DefaultHttpContext();

        context.Request.Headers.TryAdd("Cookie", "essential1_cookie=value; essential2_cookie=value; nonessential_cookie=value;");

        cleaner.RemoveNonEssentialCookies(context);

        var cookies = context.Response.GetTypedHeaders().SetCookie;
        var nonEssentialCookie = cookies.FirstOrDefault(c => c.Name == nonEssentialCookieKey);
        Assert.NotNull(nonEssentialCookie);
        Assert.Equal("", nonEssentialCookie.Value);
        var dateTimeDifference = nonEssentialCookie.Expires - DateTime.Now;
        Assert.True(dateTimeDifference.HasValue && dateTimeDifference.Value.TotalSeconds < 10);

        foreach (var essentialCookie in essentialCookies)
        {
            var matchingCookie = cookies.FirstOrDefault(c => c.Name.ToString().StartsWith(essentialCookie));
            Assert.Null(matchingCookie);
        }
    }
}
