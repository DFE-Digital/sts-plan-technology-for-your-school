namespace Dfe.ContentSupport.Web.Extensions;

public static class DateTimeExtensions
{
    public static string ToLongString(this DateTime dateTime)
    {
        return dateTime.ToString("dd MMMM yyyy");
    }
}
