ALTER TABLE [settlementreports].[SettlementReport]
ADD [SplitReportPerGridArea] [bit] NOT NULL DEFAULT(0);
GO

ALTER TABLE [settlementreports].[SettlementReport]
ADD [IncludeMonthlyAmount] [bit] NOT NULL DEFAULT(0);
GO

ALTER TABLE [settlementreports].[SettlementReport]
ADD [GridAreas] [nvarchar](2048) NOT NULL CONSTRAINT SettlementReport_GridAreas_Default_Constraint DEFAULT('');
GO

ALTER TABLE [settlementreports].[SettlementReport]
DROP CONSTRAINT SettlementReport_GridAreas_Default_Constraint 
GO