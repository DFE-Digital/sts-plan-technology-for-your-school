using DbUp;
using System.Reflection;



/// <summary>
/// Prototype DPUP.
/// </summary>

internal class Program
{
    private static int Main(string[] args)
    {
        const string connectionString = "";

        var upgrader =
            DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ResetColor();
#if DEBUG
            Console.ReadLine();
#endif
            return -1;
        }
        Console.WriteLine("Success!");
        Console.WriteLine(result);
        return 0;
    }
}