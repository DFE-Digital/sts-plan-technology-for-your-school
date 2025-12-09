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

        // Seed test MAT group + links
        db.Database.ExecuteSql($@"
                                SET NOCOUNT ON;

                                -- Clear any existing test data for this group
                                DELETE FROM [dbo].[establishmentLink]
                                WHERE groupUid = '99999';

                                DELETE FROM [dbo].[establishmentGroup]
                                WHERE uid = '99999';

                                -- Insert test MAT group
                                INSERT INTO [dbo].[establishmentGroup] (uid, groupName, groupType, groupStatus)
                                VALUES (
                                    '99999',
                                    'DSI TEST Multi-Academy Trust (010)',
                                    'Multi-academy trust',
                                    'Open'
                                );

                                -- Insert test establishments linked to that MAT
                                INSERT INTO [dbo].[establishmentLink] (groupUid, establishmentName, urn)
                                VALUES
                                ('99999', 'DSI TEST Establishment (001) Community School (01)', '00000002'),
                                ('99999', 'DSI TEST Establishment (001) Miscellaneous (27)', '00000018'),
                                ('99999', 'DSI TEST Establishment (001) Foundation School (05)', '00000005');

                                -- Insert test establishments
                                INSERT INTO [dbo].[establishment] (establishmentRef, establishmentType, orgName, groupUid)
                                VALUES
                                ('00000002', 'Community School', 'DSI TEST Establishment (001) Community School (01)', '99999'),
                                ('00000018', 'Miscellaneous','DSI TEST Establishment (001) Miscellaneous (27)', '99999'),
                                ('00000005', 'Foundation School', 'DSI TEST Establishment (001) Foundation School (05)', '99999');
                           ");

    }
}
