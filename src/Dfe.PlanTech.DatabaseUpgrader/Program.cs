using CommandLine;
using Dfe.PlanTech.DatabaseUpgrader;

/// <summary>
/// PlanTech Database Upgrader.
/// </summary>
internal class Program
{
    private static readonly Logger _logger = new Logger();

    private static void Main(string[] args)
    {
        var success = false;

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

        if (!success)
        {
            throw new InvalidOperationException("Errors occurred migrating database");
        }
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
