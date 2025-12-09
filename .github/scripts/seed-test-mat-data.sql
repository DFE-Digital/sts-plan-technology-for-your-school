SET NOCOUNT ON;

DELETE FROM dbo.establishmentLink
WHERE groupUid = '99999';

DELETE FROM dbo.establishmentGroup
WHERE uid = '99999';

INSERT INTO dbo.establishmentGroup (uid, groupName, groupType, groupStatus)
VALUES
(
    '99999',
    'DSI TEST Multi-Academy Trust (010)',
    'Multi-academy trust',
    'Open'
);


INSERT INTO dbo.establishmentLink (groupUid, establishmentName, urn)
VALUES
('99999', 'DSI TEST Establishment (001) Community School (01)', 2),
('99999', 'DSI TEST Establishment (001) Miscellaneous (27)', 18),
('99999', 'DSI TEST Establishment (001) Foundation School (05)', 5);

PRINT 'Seed test data completed successfully.';
