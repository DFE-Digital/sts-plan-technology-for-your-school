using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Helpers;

public static class RichTextMarkExtensions
{
    public const string UNDERLINE_CLASS = "govuk-!-font-underline";
    public const string UNDERLINE_MARK = "underline";

    public const string BOLD_CLASS = "govuk-!-font-weight-bold";
    public const string BOLD_MARK = "bold";

    public static string GetClass(this RichTextMark mark) => mark.Type switch
    {
        UNDERLINE_MARK => UNDERLINE_CLASS,
        BOLD_MARK => BOLD_CLASS,
        _ => ""
    };
}
