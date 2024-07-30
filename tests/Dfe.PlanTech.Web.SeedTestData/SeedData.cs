using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Web.SeedTestData.ContentGenerators;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Web.SeedTestData;

public class SeedData(CmsDbContext db)
{
    /// <summary>
    /// List of all standalone mock content generators for the seeded database
    /// </summary>
    private readonly List<IContentGenerator> _contentGenerators =
    [
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
    /// Creates the basic data like establishment, user,
    /// that plan tech requires to operate.
    /// </summary>
    private void CreateBaseData()
    {
        // Match establishment and user ID of test account
        db.Database.ExecuteSql($@"SET IDENTITY_INSERT [dbo].[user] ON
Insert into [dbo].[user] (id, dfeSignInRef) Select 53, 'sign-in-ref'
SET IDENTITY_INSERT [dbo].[user] OFF");

        db.Database.ExecuteSql($@"SET IDENTITY_INSERT [dbo].[establishment] ON
Insert into [dbo].[establishment] (id, establishmentRef, establishmentType, orgName) 
Select 16, 'Test Ref', 'Test School', 'Test Establishment'
SET IDENTITY_INSERT [dbo].[establishment] OFF");
    }
}
