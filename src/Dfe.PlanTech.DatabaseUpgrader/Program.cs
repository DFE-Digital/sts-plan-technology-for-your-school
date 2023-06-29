using DbUp;
using DbUp.Engine;
using System.Reflection;
using Polly;
using Polly.Timeout;
using Polly.Retry;
using DbUp.Helpers;
using System.IO;

/// <summary>
/// PlanTech Database Upgrader.
/// </summary>
internal class Program
{
    private const int ErrorResult = -1;
    private const int SuccessResult = 0;

    private const string ScriptsNamespace = "Dfe.PlanTech.DatabaseUpgrader.Scripts";
    private const string StoredProcsNamespace = "Dfe.PlanTech.DatabaseUpgrader.StoredProcs";

    private static int Main(string[] args)
    {
        if (args == null || !args.Any())
        {
            DisplayError("Please supply a connection string.");
            return ErrorResult;
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

        return result ? SuccessResult : ErrorResult;
    }

    private static bool MigrateDatabase(string connectionString)
    {
        var scriptsUpgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(
                Assembly.GetExecutingAssembly(),
                scriptNamespace => {
                    return scriptNamespace.StartsWith(ScriptsNamespace);
                })
            .LogToConsole()
            .LogScriptOutput()
            .WithTransaction()
            .Build();

        var storedProcUpgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .JournalTo(new NullJournal())
            .WithScriptsEmbeddedInAssembly(
                Assembly.GetExecutingAssembly(),
                scriptNamespace => {
                    return scriptNamespace.StartsWith(StoredProcsNamespace);
                })
            .LogToConsole()
            .LogScriptOutput()
            .WithTransaction()
            .Build();

        if(!Execute(scriptsUpgrader) ||
           !Execute(storedProcUpgrader))
        {
            return false;
        }

        return true;
    }

    private static bool Execute(UpgradeEngine upgrader)
    {
        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            DisplayError("The database migration was not successful.");
            DisplayError(result.Error.Message, result.Error);
            return false;
        }

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