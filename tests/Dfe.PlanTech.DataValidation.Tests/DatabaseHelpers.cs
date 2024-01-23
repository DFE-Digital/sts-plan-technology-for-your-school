using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dfe.PlanTech.DataValidation.Tests;
public static class DatabaseHelpers
{
  public static CmsDbContext CreateDbContext()
  {
    var sqlServer = ConfigurationSetup.Configuration.GetConnectionString("Database");

    var databaseOptionsBuilder = new DbContextOptionsBuilder<CmsDbContext>()
                              .UseSqlServer(sqlServer);

    return new CmsDbContext(databaseOptionsBuilder.Options);
  }
}
