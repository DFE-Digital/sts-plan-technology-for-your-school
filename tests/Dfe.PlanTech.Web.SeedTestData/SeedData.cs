using Dfe.PlanTech.Data.Sql;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Web.SeedTestData;

public class SeedData(PlanTechDbContext db)
{

    public void CreateData()
    {
        CreateBaseData();
    }

    /// <summary>
    /// Creates the basic data like establishments, users,
    /// that plan tech requires to operate.
    /// </summary>
    private void CreateBaseData()
    {
        // Add 100 dummy users & establishments to align with test account logins
        db.Database.ExecuteSql(@$"With Range(n) AS (Select 1 union all select n+1 from Range where n < 100)
Insert into [dbo].[user] (dfeSignInRef) Select CONCAT('sign-in-ref-', n) From Range");

        db.Database.ExecuteSql($@"With Range(n) AS (Select 1 union all select n+1 from Range where n < 100)
Insert into [dbo].[establishment] (establishmentRef, establishmentType, orgName)
Select CONCAT('Test Ref ', n), 'Test School', CONCAT('Test Establishment ', n) from Range");
    }
}
