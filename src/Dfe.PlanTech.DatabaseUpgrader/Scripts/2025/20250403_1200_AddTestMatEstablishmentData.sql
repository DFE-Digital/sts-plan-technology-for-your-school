UPDATE dbo.establishment
SET groupUid = '99999'
WHERE orgName = 'DSI TEST Multi-Academy Trust (010)'
INSERT INTO dbo.establishmentGroup
(uid, groupName, groupType, groupStatus)
VALUES
('99999', 'DSI TEST Multi-Academy Trust (010)', 'Multi-academy trust', 'Open')
INSERT INTO dbo.establishmentLink
(groupUid, establishmentName, urn)
VALUES
('99999', 'DSI TEST Establishment (001) Community School (01)', '00000002'),
('99999', 'DSI TEST Establishment (001) Miscellaneous (27)', '00000018'),
('99999', 'DSI TEST Establishment (001) Foundation School (05', '00000005');