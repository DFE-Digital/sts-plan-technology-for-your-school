using System.Globalization;
using Dfe.PlanTech.Domain.Helpers;

namespace Dfe.PlanTech.Domain.UnitTests.Helpers;

public class DateTimeFormatterTests
{
    [Theory]
    [InlineData("2015/10/15 10:35:00", "10:35am")]
    [InlineData("2015/10/15 09:35:00", "9:35am")]
    [InlineData("2015/10/15 14:20:00", "2:20pm")]
    public void FormattedTime_Should_Display_Correctly(string inputDate, string expected)
    {
        var dateTime = DateTime.Parse(inputDate, new CultureInfo("en-GB"));
        Assert.Equal(expected, DateTimeFormatter.FormattedTime(dateTime));
    }

    [Theory]
    [InlineData("2015/09/15", "15 September 2015")]
    [InlineData("2020/01/09", "9 January 2020")]
    public void FormattedDateLong_Should_Display_Correctly(string inputDate, string expected)
    {
        var dateTime = DateTime.Parse(inputDate, new CultureInfo("en-GB"));
        Assert.Equal(expected, DateTimeFormatter.FormattedDateLong(dateTime));
    }

    [Theory]
    [InlineData("2015/09/15", "15 Sept 2015")]
    [InlineData("2020/01/09", "9 Jan 2020")]
    public void FormattedDateShort_Should_Display_Correctly(string inputDate, string expected)
    {
        var dateTime = DateTime.Parse(inputDate, new CultureInfo("en-GB"));
        Assert.Equal(expected, DateTimeFormatter.FormattedDateShort(dateTime));
    }
}
