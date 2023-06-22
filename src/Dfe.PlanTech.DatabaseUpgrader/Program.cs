using DbUp;
using System.Reflection;
using Polly;
using Polly.Timeout;
using Polly.Retry;

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

        var result = false;
        var retryPolicy = SetupRetryPolicy();

        try
        {
            result = retryPolicy.Execute(() => MigrateDatabase(connectionString));
        }
        catch (Exception ex)
        {
            DisplayError("An exception occurred whilst migrating the database.");
            DisplayError(ex.Message, ex);
        }

        return result ? SUCCESS_RESULT : ERROR_RESULT;
    }

    private static bool MigrateDatabase(string connectionString)
    {
        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .LogToConsole()
            .LogScriptOutput()
            .WithTransaction()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            DisplayError("The database migration was not successful.");
            DisplayError(result.Error.Message, result.Error);
            return false;
        }

        DisplaySuccess("Successx");

        return true;
    }

    private static RetryPolicy SetupRetryPolicy()
    {
        return Policy.Handle<Exception>().WaitAndRetry(
            new[]
            {
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(2),
                TimeSpan.FromMinutes(3)
            });
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