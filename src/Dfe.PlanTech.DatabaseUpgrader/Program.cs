using Dfe.PlanTech.DatabaseUpgrader;
using CommandLine;

/// <summary>
/// PlanTech Database Upgrader.
/// </summary>
internal class Program
{
    private const int ErrorResult = -1;
    private const int SuccessResult = 0;
    private static readonly Logger _logger = new Logger();

    private static int Main(string[] args)
    {
        bool success = false;

        Parser.Default.ParseArguments<Options>(args)
          .WithParsed((options) =>
          {
              var databaseExecutor = new DatabaseExecutor(options, _logger);
              success = databaseExecutor.MigrateDatabase();
          })
          .WithNotParsed((errors) =>
          {
              success = HandleParseError(errors);
          });

        return success ? SuccessResult : ErrorResult;
    }

    /// <summary>
    /// Ran if error parsing command line arguments
    /// </summary>
    /// <param name="errors"></param>
    private static bool HandleParseError(IEnumerable<Error> errors)
    {
        bool continueProcessing = true;
        foreach (var error in errors)
        {
            _logger.DisplayErrors($"Error parsing {error.Tag}");

            if (error.StopsProcessing)
            {
                continueProcessing = false;
            }
        }

        return continueProcessing;
    }
}