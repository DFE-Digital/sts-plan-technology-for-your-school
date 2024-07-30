using System.Reflection;
using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using Polly;
using Polly.Retry;

namespace Dfe.PlanTech.DatabaseUpgrader;

public class DatabaseExecutor
{
    private readonly Logger _logger;
    private readonly Options _options;

    private const string SCRIPTS_NAMESPACE = "Dfe.PlanTech.DatabaseUpgrader.Scripts";
    private const string ENVIRONMENT_SPECIFIC_SCRIPTS_NAMESPACE = "Dfe.PlanTech.DatabaseUpgrader.EnvironmentSpecificScripts";

    public DatabaseExecutor(Options options, Logger logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool MigrateDatabase()
    {
        UpgradeEngine scriptsUpgrader = CreateUpgradeEngine();

        var result = SetupRetryPolicy().ExecuteAndCapture(() => TryExecute(scriptsUpgrader));

        return result.Result;
    }

    private UpgradeEngine CreateUpgradeEngine()
    {
        var executingAssembley = Assembly.GetExecutingAssembly();

        var engine = DeployChanges.To.SqlDatabase(_options.DatabaseConnectionString)
                                      .WithScriptsEmbeddedInAssembly(executingAssembley, ScriptNamespaceMatches(SCRIPTS_NAMESPACE));

        AddSqlParameters(engine);
        AddEnvironmentSpecificScripts(executingAssembley, engine);

        return engine.LogToConsole()
        .LogScriptOutput()
        .WithTransaction()
        .Build();
    }

    private void AddEnvironmentSpecificScripts(Assembly executingAssembley, UpgradeEngineBuilder engine)
    {
        foreach (var environment in _options.Environments)
        {
            var ns = GetNamespaceForEnvironment(environment);
            engine.WithScriptsEmbeddedInAssembly(executingAssembley, ScriptNamespaceMatches(ns));
        }
    }

    private void AddSqlParameters(UpgradeEngineBuilder engine)
    {
        var parameters = _options.FormattedSqlParameters;
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                engine.WithVariable(param.Key, param.Value);
            }
        }
    }

    private static Func<string, bool> ScriptNamespaceMatches(string expectedStartsWith)
     => scriptNamespace => scriptNamespace.StartsWith(expectedStartsWith, StringComparison.InvariantCultureIgnoreCase);

    private static string GetNamespaceForEnvironment(string environment)
     => string.Format("{0}.{1}", ENVIRONMENT_SPECIFIC_SCRIPTS_NAMESPACE, environment);

    private bool TryExecute(UpgradeEngine upgrader)
    {
        var result = upgrader.PerformUpgrade();

        if (result.Successful)
            return true;

        _logger.DisplayErrors("The database migration was not successful.", result.Error.Message, result.Error.StackTrace);
        return false;
    }

    private static RetryPolicy SetupRetryPolicy() => Policy.Handle<Exception>().WaitAndRetry(
          new[]
          {
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(2),
                TimeSpan.FromMinutes(3)
          });
}
