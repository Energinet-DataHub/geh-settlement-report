CREATE TABLE [settlementreports].[GridAreaOwner]
(
    Id             INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Code           VARCHAR(4)  NOT NULL,
    ActorNumber    VARCHAR(50) NOT NULL,
    ValidFrom      datetimeoffset(7) NOT NULL,
    SequenceNumber INT         NOT NULL DEFAULT 0,
);