ALTER TABLE [dbo].[TeamsPlayers]
    ADD [NextTournamentRoster] BIT CONSTRAINT [DF_TeamsPlayers_NextTournamentRoster] DEFAULT ((0)) NOT NULL;