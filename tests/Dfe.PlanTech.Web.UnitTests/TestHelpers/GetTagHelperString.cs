using Microsoft.AspNetCore.Html;
using System.Text.Encodings.Web;

namespace Dfe.PlanTech.Web.UnitTests;

public static class GetTagHelperString
{
    public static string ToHtmlString(this IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}