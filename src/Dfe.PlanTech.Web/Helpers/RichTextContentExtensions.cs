using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Helpers;

public static class RichTextContentExtensions
{
    public static string GetClass(this RichTextMark mark) => mark.Type switch
    {
        "underline" => "govuk-!-font-underline",
        "bold" => "govuk-!-font-weight-bold",
        _ => ""
    };
}