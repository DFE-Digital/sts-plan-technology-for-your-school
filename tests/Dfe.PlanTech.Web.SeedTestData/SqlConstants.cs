namespace Dfe.PlanTech.Web.SeedTestData
{
    public static class SqlConstants
    {
        public const string DsiSeedData = $@"
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
                                ('00000002', 'Community School', 'DSI TEST Establishment (001) Community School (01)', null),
                                ('00000018', 'Miscellaneous','DSI TEST Establishment (001) Miscellaneous (27)', null),
                                ('00000005', 'Foundation School', 'DSI TEST Establishment (001) Foundation School (05)', null),
                                ('00000046', null, 'DSI TEST Multi-Academy Trust (010)', '99999');
                           ";

    }
}
