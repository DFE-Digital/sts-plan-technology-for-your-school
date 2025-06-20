using System.Text;

namespace Dfe.PlanTech.Domain.Extensions;

public static class StringBuilderExtensions
{
    /// <summary>
    /// More performant method of checking whether the string builder ends with specific text
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="test"></param>
    /// <returns></returns>
    public static bool EndsWith(this StringBuilder sb, string test)
    {
        if (sb.Length < test.Length)
            return false;

        string end = sb.ToString(sb.Length - test.Length, test.Length);
        return end.Equals(test);
    }
}
