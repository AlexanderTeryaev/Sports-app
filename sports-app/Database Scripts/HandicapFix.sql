ALTER TABLE [dbo].[TeamsPlayers] DROP CONSTRAINT [DF__TeamsPlay__Handi__1975C517]
GO
ALTER TABLE [dbo].[TeamsPlayers]
ALTER COLUMN [HandicapLevel] DECIMAL(5,2) not null
GO
ALTER TABLE [dbo].[TeamsPlayers] ADD  DEFAULT ((1.0)) FOR [HandicapLevel]
GO


ALTER TABLE [dbo].[Leagues] DROP CONSTRAINT [DF__Leagues__Maximum__15A53433]
GO
ALTER TABLE [dbo].[Leagues]
ALTER COLUMN [MaximumHandicapScoreValue] DECIMAL(5,2) not null
GO
ALTER TABLE [dbo].[Leagues] ADD  DEFAULT ((0.0)) FOR [MaximumHandicapScoreValue]
GO