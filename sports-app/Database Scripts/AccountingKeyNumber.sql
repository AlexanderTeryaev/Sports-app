ALTER TABLE [dbo].[Clubs] ADD [AccountingKeyNumber] Int NULL;
CREATE UNIQUE INDEX unique_accountingkeynumber ON [dbo].[Clubs](SeasonId,AccountingKeyNumber) WHERE AccountingKeyNumber IS NOT NULL