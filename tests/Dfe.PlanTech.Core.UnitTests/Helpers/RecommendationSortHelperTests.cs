using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Core.UnitTests.Helpers;

public class RecommendationSortHelperTests
{
    [Theory]
    [InlineData(null, RecommendationSort.Default)]
    [InlineData("Default", RecommendationSort.Default)]
    [InlineData("Status", RecommendationSort.Status)]
    [InlineData("Last updated", RecommendationSort.LastUpdated)]
    public void Should_Return_Correct_Enum_Value(string? sortOrder, RecommendationSort expected)
    {
        var result = sortOrder.GetRecommendationSortEnumValue();
        Assert.Equal(expected, result);
    }
}
