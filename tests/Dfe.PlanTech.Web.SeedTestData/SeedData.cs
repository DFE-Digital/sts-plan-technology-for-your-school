using Dfe.PlanTech.Data.Sql;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Web.SeedTestData;

public class SeedData(PlanTechDbContext db)
{
    public void CreateData()
    {
        var scriptPath = Path.Combine(
            AppContext.BaseDirectory,
            "Scripts",
            "SeedTestData.sql"
        );

        var sql = File.ReadAllText(scriptPath);

        db.Database.ExecuteSqlRaw(sql);
    }
}
