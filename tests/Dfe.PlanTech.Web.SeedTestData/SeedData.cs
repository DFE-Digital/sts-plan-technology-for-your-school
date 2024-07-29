using Dfe.PlanTech.Infrastructure.Data;
using Dfe.PlanTech.Web.SeedTestData.ContentGenerators;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Web.SeedTestData;

public class SeedData(CmsDbContext db)
{
    /// <summary>
    /// The order of this list matters, it includes all content generator classes
    /// and any content which includes content from another generator must be further down the list
    /// </summary>
    private readonly List<IContentGenerator> _contentGenerators =
    [
        new SelfAssessmentPage(db)
    ];

    public void CreateData()
    {
        //CreateBaseData();
        foreach (var generator in _contentGenerators)
        {
            generator.CreateData();
            db.SaveChanges();
        }
    }

    /// <summary>
    /// Creates the basic data like establishment, user, homepage,
    /// that plan tech requires to operate.
    /// </summary>
    private void CreateBaseData()
    {
        db.Database.ExecuteSql($@"SET IDENTITY_INSERT [dbo].[user] ON
Insert into [dbo].[user] (id, dfeSignInRef) Select 53, 'sign-in-ref'
SET IDENTITY_INSERT [dbo].[user] OFF");

        db.Database.ExecuteSql($@"SET IDENTITY_INSERT [dbo].[establishment] ON
Insert into [dbo].[establishment] (id, establishmentRef, establishmentType, orgName) 
Select 16, '00000002', 'Test School', 'Test Establishment'
SET IDENTITY_INSERT [dbo].[establishment] OFF");
    }
}