--- Merger of 755 and 791 into 791
GO
--This will add the new owner of the grid area 755, which is valid from the 3rd of January 2022
INSERT INTO [settlementreports].[GridAreaOwner] (Code, ActorNumber, ValidFrom, SequenceNumber)
VALUES
    ('755', '5790000705689', '2022-01-03T00:00:00.0000000+00:00', (SELECT max(SequenceNumber)+1 from  [settlementreports].[GridAreaOwner]))

--This is the previous entry we have, which is valid from the 1st of January 2020, which is the earliest date we have in the database
UPDATE [settlementreports].[GridAreaOwner]
SET ActorNumber = '5790000836703'
WHERE id = 62 AND SequenceNumber = 233 -- matches the correct row in the production database

--- Merger of 512 and 543 into 543
GO
--This will add the new owner of the grid area 755, which is valid from the 3rd of January 2022
INSERT INTO [settlementreports].[GridAreaOwner] (Code, ActorNumber, ValidFrom, SequenceNumber)
VALUES
    ('512', '5790000610976', '2022-01-03T00:00:00.0000000+00:00', (SELECT max(SequenceNumber)+1 from  [settlementreports].[GridAreaOwner]))

--This is the previous entry we have, which is valid from the 1st of January 2020, which is the earliest date we have in the database
UPDATE [settlementreports].[GridAreaOwner]
SET ActorNumber = '5790001087975'
WHERE id = 63 AND SequenceNumber = 234 -- matches the correct row in the production database

--- Merger of 152 and 131 into 131
GO
--This will add the new owner of the grid area 755, which is valid from the 3rd of January 2022
INSERT INTO [settlementreports].[GridAreaOwner] (Code, ActorNumber, ValidFrom, SequenceNumber)
VALUES
    ('152', '5790001089030', '2023-01-03T00:00:00.0000000+00:00', (SELECT max(SequenceNumber)+1 from  [settlementreports].[GridAreaOwner]))

--This is the previous entry we have, which is valid from the 1st of January 2020, which is the earliest date we have in the database
UPDATE [settlementreports].[GridAreaOwner]
SET ActorNumber = '5790000681372'
WHERE id = 61 AND SequenceNumber = 235 -- matches the correct row in the production database

--- Merger of 398 and 131 into 131
GO
--This will add the new owner of the grid area 755, which is valid from the 3rd of January 2022
INSERT INTO [settlementreports].[GridAreaOwner] (Code, ActorNumber, ValidFrom, SequenceNumber)
VALUES
    ('398', '5790001089030', '2023-01-03T00:00:00.0000000+00:00', (SELECT max(SequenceNumber)+1 from  [settlementreports].[GridAreaOwner]))

--This is the previous entry we have, which is valid from the 1st of January 2020, which is the earliest date we have in the database
UPDATE [settlementreports].[GridAreaOwner]
SET ActorNumber = '5790001095413'
WHERE id = 60 AND SequenceNumber = 236 -- matches the correct row in the production database
