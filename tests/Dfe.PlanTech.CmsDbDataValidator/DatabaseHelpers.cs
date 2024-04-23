using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.PlanTech.CmsDbDataValidator;

public static class DatabaseHelpers
{
    public static CmsDbContext CreateDbContext()
    {
        var sqlServer = ConfigurationSetup.Configuration.GetConnectionString("Database");

        ServiceCollection sc = new();
        sc.AddSingleton(new ContentfulOptions(true));
        var serviceProvider = sc.BuildServiceProvider();
        var databaseOptionsBuilder = new DbContextOptionsBuilder<CmsDbContext>()
                                  .UseSqlServer(sqlServer)
                                  .UseApplicationServiceProvider(serviceProvider);

        return new CmsDbContext(databaseOptionsBuilder.Options);
    }
}
