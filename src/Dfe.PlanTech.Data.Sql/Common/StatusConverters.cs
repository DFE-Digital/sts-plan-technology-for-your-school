using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dfe.PlanTech.Data.Sql.Common;

internal static class StatusConverters
{
    internal static ValueConverter<RecommendationStatus?, string?> RecommendationStatusConverter =>
        new(v => v.ToString(), v => v.ToRecommendationStatus());

    internal static ValueConverter<SubmissionStatus, string> SubmissionStatusConverter =>
        new(v => v.ToString(), v => v.ToSubmissionStatus());
}
