namespace Dfe.PlanTech.Domain.Helpers;

public static class DateTimeFormatter
{
    public static string FormattedTime(DateTime dateTime) => $"{dateTime:h:mmtt}".ToLower();

    public static string FormattedDateLong(DateTime dateTime) => $"{dateTime:d MMMM yyyy}";

    public static string FormattedDateShort(DateTime dateTime) => $"{dateTime:d MMM yyyy}";
}
