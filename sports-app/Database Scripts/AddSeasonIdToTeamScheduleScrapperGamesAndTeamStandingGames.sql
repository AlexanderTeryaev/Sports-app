ALTER TABLE [dbo].[TeamScheduleScrapperGames] ADD SeasonId INT NULL
GO

ALTER TABLE [dbo].[TeamScheduleScrapperGames]
ADD CONSTRAINT FK_SeasonId_TeamScheduleScrapperGames FOREIGN KEY (SeasonId) REFERENCES Seasons(Id);
GO

ALTER TABLE [dbo].[TeamStandingGames] ADD SeasonId INT NULL
GO

ALTER TABLE [dbo].[TeamStandingGames]
ADD CONSTRAINT FK_SeasonId_TeamStandingGames FOREIGN KEY (SeasonId) REFERENCES Seasons(Id);
GO