UPDATE dbo.establishment
SET establishmentRef = '900006'
WHERE orgName LIKE 'DSI TEST Establishment (001) Community School%'

UPDATE dbo.establishment
SET establishmentRef = '900008'
WHERE orgName LIKE 'DSI TEST Establishment (001) Miscellaneous%'

UPDATE dbo.establishment
SET establishmentRef = '900007'
WHERE orgName LIKE 'DSI TEST Establishment (001) Foundation School%'

UPDATE dbo.establishmentLink
SET urn = '900006'
WHERE establishmentName LIKE 'DSI TEST Establishment (001) Community School%'

UPDATE dbo.establishmentLink
SET urn = '900008'
WHERE establishmentName LIKE  'DSI TEST Establishment (001) Miscellaneous%'

UPDATE dbo.establishmentLink
SET urn = '900007'
WHERE establishmentName LIKE 'DSI TEST Establishment (001) Foundation School%'
