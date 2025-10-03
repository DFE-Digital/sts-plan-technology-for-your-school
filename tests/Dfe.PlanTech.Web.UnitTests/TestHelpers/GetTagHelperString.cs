using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Dfe.PlanTech.Web.UnitTests.TestHelpers;

public static class GetTagHelperString
{
    public static string ToHtmlString(this IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}
