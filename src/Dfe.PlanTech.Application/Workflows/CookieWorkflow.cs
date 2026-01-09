using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Application.Workflows.Options;
using Microsoft.AspNetCore.Http;

namespace Dfe.PlanTech.Application.Workflows;

///<remarks>
///Removes non-essential cookies from the browser when user rejects cookies.
///</remarks>
public class CookieWorkflow(CookieWorkflowOptions options) : ICookieWorkflow
{
    public void RemoveNonEssentialCookies(HttpContext context)
    {
        var nonessentialCookies = GetNonEssentialCookies(context);

        foreach (var nonessentialCookie in nonessentialCookies)
        {
            MarkCookieForDeletion(context, nonessentialCookie);
        }
    }

    /// <summary>
    /// Marks a cookie for deletion in the HTTP response
    /// </summary>
    /// <param name="context"></param>
    /// <param name="nonessentialCookie"></param>
    private static void MarkCookieForDeletion(HttpContext context, string nonessentialCookie)
    {
        context.Response.Cookies.Append(nonessentialCookie, "", CreateExpiryCookieOptions());
    }

    /// <summary>
    /// Creates cookie options that expire one second ago
    /// </summary>
    /// <remarks>
    /// We can't actually delete the cookie, as it's not already in the response cookies.
    /// So we override it with a new one (with an empty value), and set its expiry to a time in the past.
    /// It should then be deleted by the browser either on receiving the response, or next page load.
    /// Either way, the value will now be empty, and thus the 🪄 original tracking cookie is gone.🪄
    /// </remarks>
    /// <returns></returns>
    private static CookieOptions CreateExpiryCookieOptions() =>
        new()
        {
            Expires = DateTime.Now.AddSeconds(-1),
            HttpOnly = true,
            Secure = true,
        };

    /// <summary>
    /// Returns all cookies whose name doesn't start with any of the values in the <see cref="opts">options</see>
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private IEnumerable<string> GetNonEssentialCookies(HttpContext context) =>
        context
            .Request.Cookies.Where(cookie =>
                !Array.Exists(
                    options.EssentialCookies,
                    essentialCookie => cookie.Key.StartsWith(essentialCookie)
                )
            )
            .Select(cookie => cookie.Key);
}
