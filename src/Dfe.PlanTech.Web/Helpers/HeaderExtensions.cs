using System.ComponentModel;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Helpers;

public static class HeaderExtensions
{
    public const string SMALL = "govuk-heading-s";
    public const string MEDIUM = "govuk-heading-m";
    public const string LARGE = "govuk-heading-l";
    public const string EXTRALARGE = "govuk-heading-xl";

    public static string GetClassForSize(this Header header) => header.Size switch
    {
        HeaderSize.Small => SMALL,
        HeaderSize.Medium => MEDIUM,
        HeaderSize.Large => LARGE,
        HeaderSize.ExtraLarge => EXTRALARGE,
        _ => throw new InvalidEnumArgumentException($"Could not find header size for {header.Size}")
    };
}
