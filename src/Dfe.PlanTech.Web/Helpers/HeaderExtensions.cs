using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Helpers;

public static class HeaderExtensions
{
    public static string GetClassForSize(this Header header) => header.Size switch
    {
        HeaderSize.Small => "govuk-heading-s",
        HeaderSize.Medium => "govuk-heading-m",
        HeaderSize.Large => "govuk-heading-l",
        HeaderSize.ExtraLarge => "govuk-heading-xl",
        _ => "govuk-heading-s"
    };
}