CREATE TABLE [settlementreports].[MeasurementsReport]
(
    [Id] [int] IDENTITY (1, 1) NOT NULL,
    [UserId] [uniqueidentifier] NOT NULL,
    [ActorId] [uniqueidentifier] NOT NULL,
    [RequestId] [nvarchar] (512) NOT NULL,
    [Status] int NOT NULL,
    [BlobFilename] [nvarchar] (2048) NULL,
    [PeriodStart] [datetime2] NOT NULL,
    [PeriodEnd] [datetime2] NOT NULL,
    [CreatedDateTime] [datetimeoffset] NOT NULL,
    [JobRunId] [bigint] NULL,
    [EndedDateTime] [datetime2] NULL,
    [GridAreaCodes] [nvarchar] (max) NOT NULL,
    CONSTRAINT [PK_MeasurementsReportRequest] PRIMARY KEY ([Id])
)