ALTER TABLE [dbo].[TeamsPlayers]
    ADD [LeagueId] INT NULL,
        [ClubId]   INT NULL;
ALTER TABLE [dbo].[TeamsPlayers] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamsPlayers_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);
ALTER TABLE [dbo].[TeamsPlayers] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamsPlayers_Leagues] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[TeamsPlayers] WITH CHECK CHECK CONSTRAINT [FK_TeamsPlayers_Clubs];

ALTER TABLE [dbo].[TeamsPlayers] WITH CHECK CHECK CONSTRAINT [FK_TeamsPlayers_Leagues];