using DbUp;
using System.Reflection;

/// <summary>
/// PlanTech Database Upgrader.
/// </summary>
internal class Program
{
    const int ERROR_RESULT = -1;
    const int SUCCESS_RESULT = 0;

    private static int Main(string[] args)
    {
        if (args == null || !args.Any())
        {
            DisplayError("Please supply a connection string.");
            return ERROR_RESULT;
        }

        var connectionString = args[0];

        var result = MigrateDatabase(connectionString);

        return result ? SUCCESS_RESULT : ERROR_RESULT;
    }

    private static bool MigrateDatabase(string connectionString)
    {
        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            DisplayError(result.Error.Message, result.Error);
            return false;
        }

        DisplaySuccess("Success!");

        return true;
    }

    private static void DisplaySuccess(string successMessage)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(successMessage);
        Console.ResetColor();
    }

    private static void DisplayError(string errorMessage, Exception? exception = null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(errorMessage);
        if (exception != null)
        {
            Console.WriteLine(exception.StackTrace);
        }
        Console.ResetColor();
    }
}