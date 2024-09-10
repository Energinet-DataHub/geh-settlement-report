ALTER TABLE [settlementreports].[SettlementReport]
ADD [JobId] [BigInt] NULL;
GO

ALTER TABLE [settlementreports].[SettlementReport]
ALTER COLUMN [RequestId] [nvarchar](512) NULL;
GO