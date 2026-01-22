using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Core.UnitTests.Helpers;

public class RecommendationSortHelperTests
{
    [Theory]
    [InlineData(null, RecommendationSortOrder.Default)]
    [InlineData("Default", RecommendationSortOrder.Default)]
    [InlineData("Status", RecommendationSortOrder.Status)]
    [InlineData("Last updated", RecommendationSortOrder.LastUpdated)]
    public void Should_Return_Correct_Enum_Value(
        string? sortOrder,
        RecommendationSortOrder expected
    )
    {
        var result = sortOrder.GetRecommendationSortEnumValue();
        Assert.Equal(expected, result);
    }
}
