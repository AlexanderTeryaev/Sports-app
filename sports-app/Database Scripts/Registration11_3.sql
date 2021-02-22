ALTER TABLE [dbo].[TeamsPlayers]
    ADD [IsTrainerPlayer] BIT CONSTRAINT [DF_TeamsPlayers_IsTrainerPlayer] DEFAULT ((0)) NOT NULL;