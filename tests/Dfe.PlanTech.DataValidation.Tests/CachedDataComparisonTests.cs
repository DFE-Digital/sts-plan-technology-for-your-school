using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dfe.PlanTech.DataValidation.Tests;

public class CachedDataComparisonTests
{
  private readonly CmsDbContext _db;

  public CachedDataComparisonTests()
  {
    _db = CreateDbContext();
  }

  private static CmsDbContext CreateDbContext()
  {
    var configuration = TestsSetup.BuildConfiguration();

    var databaseOptionsBuilder = new DbContextOptionsBuilder<CmsDbContext>()
                              .UseSqlServer(configuration.GetConnectionString("Database"));

    return new CmsDbContext(databaseOptionsBuilder.Options);
  }

  [Fact]
  public async Task Database_Should_Connect()
  {
    var answers = await _db.Answers.ToListAsync();

    Assert.NotEmpty(answers);
  }
}