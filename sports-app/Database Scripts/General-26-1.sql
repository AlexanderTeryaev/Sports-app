ALTER TABLE Unions ADD IsClubsBlocked BIT NOT NULL DEFAULT 0
ALTER TABLE Leagues ADD DuplicatedLeagueId INT NULL

ALTER TABLE [dbo].[Leagues] WITH NOCHECK
    ADD CONSTRAINT [FK_Leagues_DuplicatedLeague] FOREIGN KEY ([DuplicatedLeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[TennisStages] ADD LeagueId INT NULL

ALTER TABLE [dbo].[TennisStages] WITH NOCHECK
    ADD CONSTRAINT [FK_TennisStage_LeagueId] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);