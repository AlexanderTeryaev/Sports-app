ALTER TABLE [dbo].[TeamsPlayers]
    ADD [IsNewPlayerInUnion] BIT CONSTRAINT [DF_TeamsPlayers_IsNewPlayerInUnion] DEFAULT ((0)) NOT NULL;