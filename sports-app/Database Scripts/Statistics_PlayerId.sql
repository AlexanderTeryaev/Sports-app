ALTER TABLE [dbo].[Statistics] DROP CONSTRAINT [StatisticsTeamPlayers];
ALTER TABLE [dbo].[Statistics] ALTER COLUMN [PlayerId] INT NULL;
ALTER TABLE [dbo].[Statistics] WITH NOCHECK
    ADD CONSTRAINT [StatisticsTeamPlayers] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[TeamsPlayers] ([Id]);
ALTER TABLE [dbo].[Statistics] WITH CHECK CHECK CONSTRAINT [StatisticsTeamPlayers];