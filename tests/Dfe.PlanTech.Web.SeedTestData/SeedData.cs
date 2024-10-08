using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Web.SeedTestData.ContentGenerators;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Web.SeedTestData;

public class SeedData(CmsDbContext db)
{
    /// <summary>
    /// List of all mock content generators for the seeded database.
    /// These are ordered. If one generator depends on another it must come later in the list.
    /// </summary>
    private readonly List<ContentGenerator> _contentGenerators =
    [
        new ConnectivityCategory(db),
        new MissingDataCategory(db),
        new SelfAssessmentPage(db)
    ];

    public void CreateData()
    {
        CreateBaseData();
        foreach (var generator in _contentGenerators)
        {
            generator.CreateData();
            db.SaveChanges();
        }
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
