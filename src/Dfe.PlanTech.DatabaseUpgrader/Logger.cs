
namespace Dfe.PlanTech.DatabaseUpgrader;

public class Logger
{
    private readonly ConsoleColor _successColour = ConsoleColor.Green;
    private readonly ConsoleColor _errorColour = ConsoleColor.Red;

    public void DisplaySuccess(string successMessage)
    {
        DisplayMessages(_successColour, successMessage);
        Console.ResetColor();
    }

    public void DisplayError(string errorMessage, Exception? exception = null)
    {
        if (!string.IsNullOrEmpty(exception?.StackTrace))
        {
            DisplayMessages(_errorColour, errorMessage, exception.StackTrace);
        }
        else
        {
            DisplayMessages(_errorColour, errorMessage);
        }
    }

    public void DisplayErrors(params string?[] errors)
    {
        DisplayMessages(_errorColour, errors);
    }

    public void DisplayInfo(params string?[] infos)
    {
        DisplayMessages(ConsoleColor.White, infos);
    }

    private static void DisplayMessages(ConsoleColor foregroundColour, params string?[] messages)
    {
        Console.ForegroundColor = foregroundColour;

        foreach (var message in messages)
        {
            if (string.IsNullOrEmpty(message))
                continue;

            Console.WriteLine(message);
        }

        Console.ResetColor();
    }
}
