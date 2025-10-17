using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class DsiConstants
{
    public const string MatOrganisationCategoryId = "010";

    public static HashSet<string> OrganisationGroupCategories { get; } = new()
    {
        MatOrganisationCategoryId,
        // SatOrganisationCategoryId,
        // SSatOrganisationCategoryId,
    };
}
