namespace Dfe.PlanTech.Core.Helpers;

public static class DateTimeHelper
{
    public static string FormattedTime(DateTime dateTime) => $"{dateTime:h:mmtt}".ToLower();

    public static string FormattedDateLong(DateTime dateTime) => $"{dateTime:d MMMM yyyy}";

    public static string FormattedDateShort(DateTime dateTime) => $"{dateTime:d MMM yyyy}";
}
