using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class ContentfulMicrocopyConstants
{
    // Organised by view and/or component. Values correspond to Key field on Contentful microcopy entries.

    // Home
    public const string HomeHeader = "homeHeader";
    public const string HomeCardStatusSingleNotStarted = "cardStatusSingleNotStarted";
    public const string HomeCardStatusMultipleNotStarted = "cardStatusMultipleNotStarted";
    public const string HomeCardStatusContinue = "cardStatusContinue";
    public const string HomeCardStatusViewRecommendations = "cardStatusViewRecommendations";

    public static readonly List<string> EmptyFallback = new()
    {
        HomeHeader,
    };

    public static readonly List<string> CardsFallback = new()
    {
        HomeCardStatusSingleNotStarted,
        HomeCardStatusMultipleNotStarted,
        HomeCardStatusContinue,
        HomeCardStatusViewRecommendations,
    };
}
