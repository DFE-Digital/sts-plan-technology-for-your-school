-- Seed data for a fresh local / E2E database. Run after migrations.
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Dummy users and establishments for test account logins.
    ;WITH Range(n) AS (SELECT 1 UNION ALL SELECT n + 1 FROM Range WHERE n < 100)
    INSERT INTO dbo.[user] (dfeSignInRef)
    SELECT CONCAT('sign-in-ref-', n) FROM Range;

    ;WITH Range(n) AS (SELECT 1 UNION ALL SELECT n + 1 FROM Range WHERE n < 100)
    INSERT INTO dbo.establishment (establishmentRef, establishmentType, orgName)
    SELECT CONCAT('Test Ref ', n), 'Test School', CONCAT('Test Establishment ', n) FROM Range;

    -- GIAS lookup data.
    INSERT INTO gias.groupStatus (groupStatusCode, groupStatusName)
    VALUES ('CLOSED', 'Closed'), ('OPEN', 'Open'), ('PROPOSED_TO_OPEN', 'Proposed to open');

    INSERT INTO gias.groupType (groupTypeCode, groupTypeName)
    VALUES (1, 'Federation'), (2, 'Trust'), (5, 'School sponsor'),
           (6, 'Multi-academy trust'), (7, 'Umbrella trust'),
           (8, 'Children''s Centres Group'), (9, 'Children''s Centres Collaboration'),
           (10, 'Single-academy trust'), (11, 'Secure single-academy trust');

    INSERT INTO gias.establishmentStatus (establishmentStatusCode, establishmentStatusName)
    VALUES (1, 'Open'), (2, 'Closed'), (3, 'Open, but proposed to close'), (4, 'Proposed to open');

    INSERT INTO gias.gender (genderCode, genderName)
    VALUES (0, 'Not applicable'), (1, 'Boys'), (2, 'Girls'), (3, 'Mixed'), (9, 'Unknown');

    INSERT INTO gias.phase (phaseCode, phaseName)
    VALUES (0, 'Not applicable'), (1, 'Nursery'), (2, 'Primary'), (3, 'Middle deemed primary'),
           (4, 'Secondary'), (5, 'Middle deemed secondary'), (6, '16 plus'), (7, 'All-through');

    INSERT INTO gias.localAuthority (localAuthorityCode, localAuthorityName)
    VALUES
        (0, N'Does not apply'),
        (201, N'City of London'),
        (202, N'Camden'),
        (203, N'Greenwich'),
        (204, N'Hackney'),
        (205, N'Hammersmith and Fulham'),
        (206, N'Islington'),
        (207, N'Kensington and Chelsea'),
        (208, N'Lambeth'),
        (209, N'Lewisham'),
        (210, N'Southwark'),
        (211, N'Tower Hamlets'),
        (212, N'Wandsworth'),
        (213, N'Westminster'),
        (301, N'Barking and Dagenham'),
        (302, N'Barnet'),
        (303, N'Bexley'),
        (304, N'Brent'),
        (305, N'Bromley'),
        (306, N'Croydon'),
        (307, N'Ealing'),
        (308, N'Enfield'),
        (309, N'Haringey'),
        (310, N'Harrow'),
        (311, N'Havering'),
        (312, N'Hillingdon'),
        (313, N'Hounslow'),
        (314, N'Kingston upon Thames'),
        (315, N'Merton'),
        (316, N'Newham'),
        (317, N'Redbridge'),
        (318, N'Richmond upon Thames'),
        (319, N'Sutton'),
        (320, N'Waltham Forest'),
        (330, N'Birmingham'),
        (331, N'Coventry'),
        (332, N'Dudley'),
        (333, N'Sandwell'),
        (334, N'Solihull'),
        (335, N'Walsall'),
        (336, N'Wolverhampton'),
        (340, N'Knowsley'),
        (341, N'Liverpool'),
        (342, N'St. Helens'),
        (343, N'Sefton'),
        (344, N'Wirral'),
        (350, N'Bolton'),
        (351, N'Bury'),
        (352, N'Manchester'),
        (353, N'Oldham'),
        (354, N'Rochdale'),
        (355, N'Salford'),
        (356, N'Stockport'),
        (357, N'Tameside'),
        (358, N'Trafford'),
        (359, N'Wigan'),
        (370, N'Barnsley'),
        (371, N'Doncaster'),
        (372, N'Rotherham'),
        (373, N'Sheffield'),
        (380, N'Bradford'),
        (381, N'Calderdale'),
        (382, N'Kirklees'),
        (383, N'Leeds'),
        (384, N'Wakefield'),
        (390, N'Gateshead'),
        (391, N'Newcastle upon Tyne'),
        (392, N'North Tyneside'),
        (393, N'South Tyneside'),
        (394, N'Sunderland'),
        (420, N'Isles of Scilly'),
        (660, N'Isle of Anglesey'),
        (661, N'Gwynedd'),
        (662, N'Conwy'),
        (663, N'Denbighshire'),
        (664, N'Flintshire'),
        (665, N'Wrexham'),
        (666, N'Powys'),
        (667, N'Ceredigion'),
        (668, N'Pembrokeshire'),
        (669, N'Carmarthenshire'),
        (670, N'Swansea'),
        (671, N'Neath Port Talbot'),
        (672, N'Bridgend'),
        (673, N'Vale of Glamorgan'),
        (674, N'Rhondda Cynon Taf'),
        (675, N'Merthyr Tydfil'),
        (676, N'Caerphilly'),
        (677, N'Blaenau Gwent'),
        (678, N'Torfaen'),
        (679, N'Monmouthshire'),
        (680, N'Newport'),
        (681, N'Cardiff'),
        (701, N'Scotland Offshore Establishments'),
        (702, N'BFPO Overseas Establishments'),
        (704, N'Fieldwork Overseas Establishments'),
        (705, N'Isle of Man Offshore Establishments'),
        (706, N'Guernsey Offshore Establishments'),
        (707, N'Jersey Offshore Establishments'),
        (708, N'Gibraltar Overseas Establishments'),
        (800, N'Bath and North East Somerset'),
        (801, N'Bristol, City of'),
        (802, N'North Somerset'),
        (803, N'South Gloucestershire'),
        (805, N'Hartlepool'),
        (806, N'Middlesbrough'),
        (807, N'Redcar and Cleveland'),
        (808, N'Stockton-on-Tees'),
        (810, N'Kingston upon Hull, City of'),
        (811, N'East Riding of Yorkshire'),
        (812, N'North East Lincolnshire'),
        (813, N'North Lincolnshire'),
        (815, N'North Yorkshire'),
        (816, N'York'),
        (821, N'Luton'),
        (822, N'Bedford'),
        (823, N'Central Bedfordshire'),
        (825, N'Buckinghamshire'),
        (826, N'Milton Keynes'),
        (830, N'Derbyshire'),
        (831, N'Derby'),
        (835, N'Pre-LGR 2019 Dorset'),
        (836, N'Pre-LGR 2019 Poole'),
        (837, N'Pre-LGR 2019 Bournemouth'),
        (838, N'Dorset'),
        (839, N'Bournemouth, Christchurch and Poole'),
        (840, N'County Durham'),
        (841, N'Darlington'),
        (845, N'East Sussex'),
        (846, N'Brighton and Hove'),
        (850, N'Hampshire'),
        (851, N'Portsmouth'),
        (852, N'Southampton'),
        (855, N'Leicestershire'),
        (856, N'Leicester'),
        (857, N'Rutland'),
        (860, N'Staffordshire'),
        (861, N'Stoke-on-Trent'),
        (865, N'Wiltshire'),
        (866, N'Swindon'),
        (867, N'Bracknell Forest'),
        (868, N'Windsor and Maidenhead'),
        (869, N'West Berkshire'),
        (870, N'Reading'),
        (871, N'Slough'),
        (872, N'Wokingham'),
        (873, N'Cambridgeshire'),
        (874, N'Peterborough'),
        (876, N'Halton'),
        (877, N'Warrington'),
        (878, N'Devon'),
        (879, N'Plymouth'),
        (880, N'Torbay'),
        (881, N'Essex'),
        (882, N'Southend-on-Sea'),
        (883, N'Thurrock'),
        (884, N'Herefordshire, County of'),
        (885, N'Worcestershire'),
        (886, N'Kent'),
        (887, N'Medway'),
        (888, N'Lancashire'),
        (889, N'Blackburn with Darwen'),
        (890, N'Blackpool'),
        (891, N'Nottinghamshire'),
        (892, N'Nottingham'),
        (893, N'Shropshire'),
        (894, N'Telford and Wrekin'),
        (895, N'Cheshire East'),
        (896, N'Cheshire West and Chester'),
        (908, N'Cornwall'),
        (909, N'Pre-LGR 2023 Cumbria'),
        (916, N'Gloucestershire'),
        (919, N'Hertfordshire'),
        (921, N'Isle of Wight'),
        (925, N'Lincolnshire'),
        (926, N'Norfolk'),
        (928, N'Pre-LGR 2021 Northamptonshire'),
        (929, N'Northumberland'),
        (931, N'Oxfordshire'),
        (933, N'Somerset'),
        (935, N'Suffolk'),
        (936, N'Surrey'),
        (937, N'Warwickshire'),
        (938, N'West Sussex'),
        (940, N'North Northamptonshire'),
        (941, N'West Northamptonshire'),
        (942, N'Cumberland'),
        (943, N'Westmorland and Furness');

    -- DSI test schools and MAT in GIAS.
    INSERT INTO gias.establishment
        (urn, uprn, ukprn, establishmentNumber, establishmentName,
         establishmentStatusCode, genderCode, localAuthorityCode, phaseCode, syncedAt)
    VALUES
        (900006, NULL, NULL, NULL, N'DSI TEST Establishment (001) Community School (01)', 1, 3, 352, 7, SYSUTCDATETIME()),
        (900007, NULL, NULL, NULL, N'DSI TEST Establishment (001) Foundation School (05)', 1, 3, 352, 7, SYSUTCDATETIME()),
        (900008, NULL, NULL, NULL, N'DSI TEST Establishment (001) Miscellaneous (27)', 1, 3, 352, 7, SYSUTCDATETIME());

    INSERT INTO gias.establishmentGroup
        (groupUid, groupId, ukprn, groupName, groupStatusCode, groupTypeCode, syncedAt)
    VALUES (99999, NULL, NULL, N'DSI TEST Multi-Academy Trust (010)', 'OPEN', 6, SYSUTCDATETIME());

    INSERT INTO gias.groupMembership (urn, groupUid, syncedAt)
    VALUES (900006, 99999, SYSUTCDATETIME()),
           (900007, 99999, SYSUTCDATETIME()),
           (900008, 99999, SYSUTCDATETIME());

    -- DSI test MAT and schools in the Plan Tech application schema.
    -- Retain the original cleanup and insertion order.
    DELETE FROM dbo.establishmentLink WHERE groupUid = '99999';
    DELETE FROM dbo.establishmentGroup WHERE uid = '99999';

    INSERT INTO dbo.establishmentGroup (uid, groupName, groupType, groupStatus)
    VALUES ('99999', 'DSI TEST Multi-Academy Trust (010)', 'Multi-academy trust', 'Open');

    INSERT INTO dbo.establishmentLink (groupUid, establishmentName, urn)
    VALUES
        ('99999', 'DSI TEST Establishment (001) Community School (01)', '900006'),
        ('99999', 'DSI TEST Establishment (001) Miscellaneous (27)', '900008'),
        ('99999', 'DSI TEST Establishment (001) Foundation School (05)', '900007');

    INSERT INTO dbo.establishment (establishmentRef, establishmentType, orgName, groupUid)
    VALUES
        ('900006', 'Community School', 'DSI TEST Establishment (001) Community School (01)', NULL),
        ('900008', 'Miscellaneous', 'DSI TEST Establishment (001) Miscellaneous (27)', NULL),
        ('900007', 'Foundation School', 'DSI TEST Establishment (001) Foundation School (05)', NULL),
        ('00000046', NULL, 'DSI TEST Multi-Academy Trust (010)', '99999');

    -- Mock authentication accounts.
    INSERT INTO dbo.[user] (dfeSignInRef)
    VALUES ('E2E_TEST_SCHOOL_USER'), ('E2E_TEST_MAT_USER');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
