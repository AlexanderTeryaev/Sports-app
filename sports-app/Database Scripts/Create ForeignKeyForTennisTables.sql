ALTER TABLE [dbo].[TennisGameCycles]  WITH CHECK ADD  CONSTRAINT [FK_TennisGameCycles_Brackets] FOREIGN KEY([BracketId])
REFERENCES [dbo].[TennisPlayoffBrackets] ([Id])
GO

ALTER TABLE [dbo].[TennisGameCycles] CHECK CONSTRAINT [FK_TennisGameCycles_Brackets]
GO

ALTER TABLE [dbo].[TennisGameCycles]  WITH CHECK ADD  CONSTRAINT [FK_TennisGameCycles_Field] FOREIGN KEY([FieldId])
REFERENCES [dbo].[Auditoriums] ([AuditoriumId])
GO

ALTER TABLE [dbo].[TennisGameCycles] CHECK CONSTRAINT [FK_TennisGameCycles_Field]
GO

ALTER TABLE [dbo].[TennisGameCycles]  WITH CHECK ADD  CONSTRAINT [FK_TennisGameCycles_FirstPlayer] FOREIGN KEY([FirstPlayerId])
REFERENCES [dbo].[TeamsPlayers] ([Id])
GO

ALTER TABLE [dbo].[TennisGameCycles] CHECK CONSTRAINT [FK_TennisGameCycles_FirstPlayer]
GO

ALTER TABLE [dbo].[TennisGameCycles]  WITH CHECK ADD  CONSTRAINT [FK_TennisGameCycles_Group] FOREIGN KEY([GroupId])
REFERENCES [dbo].[TennisGroups] ([GroupId])
GO

ALTER TABLE [dbo].[TennisGameCycles] CHECK CONSTRAINT [FK_TennisGameCycles_Group]
GO

ALTER TABLE [dbo].[TennisGameCycles]  WITH CHECK ADD  CONSTRAINT [FK_TennisGameCycles_SecondPlayer] FOREIGN KEY([SecondPlayerId])
REFERENCES [dbo].[TeamsPlayers] ([Id])
GO

ALTER TABLE [dbo].[TennisGameCycles] CHECK CONSTRAINT [FK_TennisGameCycles_SecondPlayer]
GO

ALTER TABLE [dbo].[TennisGameCycles]  WITH CHECK ADD  CONSTRAINT [FK_TennisGameCycles_Stage] FOREIGN KEY([StageId])
REFERENCES [dbo].[TennisStages] ([StageId])
GO

ALTER TABLE [dbo].[TennisGameCycles] CHECK CONSTRAINT [FK_TennisGameCycles_Stage]
GO

ALTER TABLE [dbo].[TennisStages]  WITH CHECK ADD  CONSTRAINT [FK_TennisStages_Category] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Teams] ([TeamId])
GO

ALTER TABLE [dbo].[TennisStages] CHECK CONSTRAINT [FK_TennisStages_Category]
GO

ALTER TABLE [dbo].[TennisStages]  WITH CHECK ADD  CONSTRAINT [FK_TennisStages_Season] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[TennisStages] CHECK CONSTRAINT [FK_TennisStages_Season]
GO

ALTER TABLE [dbo].[TennisGames]  WITH CHECK ADD  CONSTRAINT [FK_TennisGames_Category] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Teams] ([TeamId])
GO

ALTER TABLE [dbo].[TennisGames] CHECK CONSTRAINT [FK_TennisGames_Category]
GO

ALTER TABLE [dbo].[TennisGames]  WITH CHECK ADD  CONSTRAINT [FK_TennisGames_Season] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[TennisGames] CHECK CONSTRAINT [FK_TennisGames_Season]
GO

ALTER TABLE [dbo].[TennisGames]  WITH CHECK ADD  CONSTRAINT [FK_TennisGames_Stage] FOREIGN KEY([StageId])
REFERENCES [dbo].[TennisStages] ([StageId])
GO

ALTER TABLE [dbo].[TennisGames] CHECK CONSTRAINT [FK_TennisGames_Stage]
GO

ALTER TABLE [dbo].[TennisGameSets]  WITH CHECK ADD  CONSTRAINT [FK_TennisGameSets_Cycle] FOREIGN KEY([GameCycleId])
REFERENCES [dbo].[TennisGameCycles] ([CycleId])
GO

ALTER TABLE [dbo].[TennisGameSets] CHECK CONSTRAINT [FK_TennisGameSets_Cycle]
GO

ALTER TABLE [dbo].[TennisGroups]  WITH CHECK ADD  CONSTRAINT [FK_TennisGroups_Season] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[TennisGroups] CHECK CONSTRAINT [FK_TennisGroups_Season]
GO

ALTER TABLE [dbo].[TennisGroups]  WITH CHECK ADD  CONSTRAINT [FK_TennisGroups_Stage] FOREIGN KEY([StageId])
REFERENCES [dbo].[TennisStages] ([StageId])
GO

ALTER TABLE [dbo].[TennisGroups] CHECK CONSTRAINT [FK_TennisGroups_Stage]
GO

ALTER TABLE [dbo].[TennisGroupTeams]  WITH CHECK ADD  CONSTRAINT [FK_TennisGroupTeams_Group] FOREIGN KEY([GroupId])
REFERENCES [dbo].[TennisGroups] ([GroupId])
GO

ALTER TABLE [dbo].[TennisGroupTeams] CHECK CONSTRAINT [FK_TennisGroupTeams_Group]
GO

ALTER TABLE [dbo].[TennisGroupTeams]  WITH CHECK ADD  CONSTRAINT [FK_TennisGroupTeams_Players] FOREIGN KEY([PlayerId])
REFERENCES [dbo].[TeamsPlayers] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[TennisGroupTeams] CHECK CONSTRAINT [FK_TennisGroupTeams_Players]
GO

ALTER TABLE [dbo].[TennisGroupTeams]  WITH CHECK ADD  CONSTRAINT [FK_TennisGroupTeams_Season] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[TennisGroupTeams] CHECK CONSTRAINT [FK_TennisGroupTeams_Season]
GO

ALTER TABLE [dbo].[TennisPlayoffBrackets]  WITH CHECK ADD  CONSTRAINT [FK_TennisPlayoffBrackets_Group] FOREIGN KEY([GroupId])
REFERENCES [dbo].[TennisGroups] ([GroupId])
GO

ALTER TABLE [dbo].[TennisPlayoffBrackets] CHECK CONSTRAINT [FK_TennisPlayoffBrackets_Group]
GO

ALTER TABLE [dbo].[TennisPlayoffBrackets]  WITH CHECK ADD  CONSTRAINT [FK_TennisPlayoffBrackets_TennisPlayoffBrackets1] FOREIGN KEY([ParentBracket1Id])
REFERENCES [dbo].[TennisPlayoffBrackets] ([Id])
GO

ALTER TABLE [dbo].[TennisPlayoffBrackets] CHECK CONSTRAINT [FK_TennisPlayoffBrackets_TennisPlayoffBrackets1]
GO

ALTER TABLE [dbo].[TennisPlayoffBrackets]  WITH CHECK ADD  CONSTRAINT [FK_TennisPlayoffBrackets_TennisPlayoffBrackets2] FOREIGN KEY([ParentBracket2Id])
REFERENCES [dbo].[TennisPlayoffBrackets] ([Id])
GO

ALTER TABLE [dbo].[TennisPlayoffBrackets] CHECK CONSTRAINT [FK_TennisPlayoffBrackets_TennisPlayoffBrackets2]
GO

ALTER TABLE [dbo].[TennisPointsSettings]  WITH CHECK ADD  CONSTRAINT [FK_TennisPointsSettings_Level] FOREIGN KEY([LevelId])
REFERENCES [dbo].[CompetitionLevel] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[TennisPointsSettings] CHECK CONSTRAINT [FK_TennisPointsSettings_Level]
GO

ALTER TABLE [dbo].[TennisPointsSettings]  WITH CHECK ADD  CONSTRAINT [FK_TennisPointsSettings_Season] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[TennisPointsSettings] CHECK CONSTRAINT [FK_TennisPointsSettings_Season]
GO