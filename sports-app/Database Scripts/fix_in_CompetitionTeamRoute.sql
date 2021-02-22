USE [LogLig]
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes] DROP CONSTRAINT [FK_Rank_CompetitionTeam];
ALTER TABLE [dbo].[CompetitionTeamRoutes] DROP CONSTRAINT [FK_Route_CompetitionTeam];
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes]  WITH NOCHECK ADD  CONSTRAINT [FK_Rank_CompetitionTeam] FOREIGN KEY([RankId])
REFERENCES [dbo].[RouteTeamRanks] ([Id])
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes] CHECK CONSTRAINT [FK_Rank_CompetitionTeam]
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes]  WITH NOCHECK ADD  CONSTRAINT [FK_Route_CompetitionTeam] FOREIGN KEY([RouteId])
REFERENCES [dbo].[DisciplineTeamRoutes] ([Id])
GO