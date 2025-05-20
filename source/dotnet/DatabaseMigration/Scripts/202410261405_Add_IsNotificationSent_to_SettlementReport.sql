ALTER TABLE [settlementreports].[SettlementReport]
ADD [IsNotificationSent] [bit] NOT NULL DEFAULT(1); --For all existing records, we will assume that notification has been sent.
GO