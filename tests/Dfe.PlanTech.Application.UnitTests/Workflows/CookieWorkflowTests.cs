using System.Globalization;
using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Application.Workflows.Options;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class CookieWorkflowTests
{
    // Helpers ---------------------------------------------------------------

    private static DefaultHttpContext ContextWithCookieHeader(string cookieHeader)
    {
        var context = new DefaultHttpContext();
        if (!string.IsNullOrWhiteSpace(cookieHeader))
            context.Request.Headers["Cookie"] = cookieHeader;
        return context;
    }

    private static string[] GetSetCookieHeaders(DefaultHttpContext context)
        => context.Response.Headers.SetCookie.Where(header => !string.IsNullOrWhiteSpace(header)).Cast<string>().ToArray();

    private static DateTimeOffset? GetExpiresFromHeader(string setCookieHeader)
    {
        var part = setCookieHeader.Split(';')
            .FirstOrDefault(p => p.TrimStart()
                .StartsWith("expires=", StringComparison.InvariantCultureIgnoreCase));
        if (part is null)
            return null;

        var value = part.Split('=', 2)[1].Trim();
        // RFC date format is GMT
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dto)
            ? dto
            : null;
    }

    // Tests -----------------------------------------------------------------

    [Fact]
    public void RemoveNonEssentialCookies_Deletes_Only_NonEssential()
    {
        // Arrange: essential prefixes are "keep" and "ess"
        var options = new CookieWorkflowOptions
        {
            EssentialCookies = new[] { "keep", "ess" }
        };
        var workflow = new CookieWorkflow(options);

        // Cookies in request: 2 essentials (keep, ess-token), 2 non-essentials (non1, non2)
        var context = ContextWithCookieHeader("keep=1; ess-token=2; non1=3; non2=4");

        // Act
        workflow.RemoveNonEssentialCookies(context);

        // Assert: two Set-Cookie headers (non1, non2) are written
        var setCookies = GetSetCookieHeaders(context);
        Assert.Equal(2, setCookies.Length);

        Assert.Contains(setCookies, header => header.StartsWith("non1=", StringComparison.Ordinal));
        Assert.Contains(setCookies, header => header.StartsWith("non2=", StringComparison.Ordinal));
        Assert.DoesNotContain(setCookies, header => header.StartsWith("keep=", StringComparison.Ordinal));
        Assert.DoesNotContain(setCookies, header => header.StartsWith("ess-token=", StringComparison.Ordinal));
    }

    [Fact]
    public void RemoveNonEssentialCookies_Writes_Empty_Value_Past_Expiry_Secure_HttpOnly()
    {
        var options = new CookieWorkflowOptions
        {
            EssentialCookies = new[] { "keep" }
        };
        var workflow = new CookieWorkflow(options);

        // nonessential cookie 'x'; essential cookie 'keep'
        var context = ContextWithCookieHeader("x=abc; keep=1");

        workflow.RemoveNonEssentialCookies(context);

        var header = GetSetCookieHeaders(context).Single();
        // empty value
        Assert.StartsWith("x=", header, StringComparison.Ordinal);
        // Expires in the past (we give a reasonable bound of "earlier than now")
        var expires = GetExpiresFromHeader(header);
        Assert.NotNull(expires);
        Assert.True(expires!.Value < DateTimeOffset.UtcNow, "Expiry should be in the past");

        // flags
        Assert.Contains("httponly", header, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("secure", header, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void RemoveNonEssentialCookies_No_Request_Cookies_Writes_Nothing()
    {
        var options = new CookieWorkflowOptions
        {
            EssentialCookies = ["keep"]
        };
        var workflow = new CookieWorkflow(options);

        var context = ContextWithCookieHeader(string.Empty);

        workflow.RemoveNonEssentialCookies(context);

        Assert.Empty(GetSetCookieHeaders(context));
    }

    [Fact]
    public void RemoveNonEssentialCookies_Essential_Prefix_Is_Case_Sensitive()
    {
        // IMPORTANT: implementation uses string.StartsWith without StringComparison,
        // so matching is case-sensitive. Prefix "Keep" will NOT match "keepX".
        var options = new CookieWorkflowOptions
        {
            EssentialCookies = new[] { "Keep" } // note capital K
        };
        var workflow = new CookieWorkflow(options);

        var context = ContextWithCookieHeader("keepX=1; KeepY=2");

        workflow.RemoveNonEssentialCookies(context);

        var setCookies = GetSetCookieHeaders(context);
        // 'keepX' should be treated as NON-essential (prefix mismatch) -> deleted
        Assert.Contains(setCookies, header => header.StartsWith("keepX=", StringComparison.Ordinal));
        // 'KeepY' starts with "Keep" (exact case) -> essential -> NOT deleted
        Assert.DoesNotContain(setCookies, header => header.StartsWith("KeepY=", StringComparison.Ordinal));
    }

    [Fact]
    public void RemoveNonEssentialCookies_Prefix_Match_Not_Exact_Name()
    {
        // Ensure prefix logic applies, not full-name matching
        var options = new CookieWorkflowOptions
        {
            EssentialCookies = new[] { "__Host-" }
        };
        var workflow = new CookieWorkflow(options);

        var context = ContextWithCookieHeader("__Host-Auth=1; __Host-Session=2; other=3");

        workflow.RemoveNonEssentialCookies(context);

        var setCookies = GetSetCookieHeaders(context);
        // Only 'other' should be marked for deletion
        Assert.Single(setCookies);
        Assert.StartsWith("other=", setCookies[0], StringComparison.Ordinal);
    }

}
