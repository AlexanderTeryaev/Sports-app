UPDATE [dbo].[Clubs] SET RegionalId = NULL WHERE RegionalId NOT IN (SELECT RegionalId FROM Regionals)

ALTER TABLE [dbo].[Clubs] WITH NOCHECK
    ADD CONSTRAINT [FK_Clubs_Regionals] FOREIGN KEY ([RegionalId]) REFERENCES [dbo].[Regionals] ([RegionalId]);

ALTER TABLE [dbo].[Clubs] WITH CHECK CHECK CONSTRAINT [FK_Clubs_Regionals];