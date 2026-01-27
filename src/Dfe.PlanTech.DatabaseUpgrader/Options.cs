using CommandLine;

namespace Dfe.PlanTech.DatabaseUpgrader;

public class Options
{
    [Option(
        'c',
        "connectionstring",
        Required = true,
        HelpText = "Connection string for the database we should execute scripts on"
    )]
    public string DatabaseConnectionString { get; init; } = null!;

    [Option(
        "env",
        Required = false,
        HelpText = "What environments should we run additional environment specific scripts on"
    )]
    public IEnumerable<string> Environments { get; init; } = Array.Empty<string>();

    [Option(
        'p',
        "sql-params",
        Required = false,
        HelpText = "Parameters for SQL scripts. Enumerable formatted as KEY=VALUE"
    )]
    public IEnumerable<string>? SqlParameters { get; init; }

    public Dictionary<string, string>? FormattedSqlParameters =>
        SqlParameters
            ?.Select(param => param.Split('='))
            .ToDictionary(param => param[0], param => param[1]);
}
