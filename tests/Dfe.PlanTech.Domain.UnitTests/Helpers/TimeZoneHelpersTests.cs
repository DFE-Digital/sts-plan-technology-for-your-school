using System.Globalization;
using Dfe.PlanTech.Domain.Helpers;

namespace Dfe.PlanTech.Domain.UnitTests.Helpers;

public class TimeZoneHelpersTests
{
    [Theory]
    [InlineData("2015/11/15 10:30:00", "2015/11/15 10:30:00")]
    [InlineData("2020/06/10 10:30:00", "2020/06/10 11:30:00")] // BST
    [InlineData("2020/06/10 23:59:00", "2020/06/11 00:59:00")] // BST
    public void ToUkTime_Should_Convert_Utc_Time_Correctly(string inputDate, string ukTime)
    {
        var dateTime = DateTime.Parse(inputDate, new CultureInfo("en-GB"));
        var expected = DateTime.Parse(ukTime, new CultureInfo("en-GB"));
        Assert.Equal(expected, TimeZoneHelpers.ToUkTime(dateTime));
    }
}
