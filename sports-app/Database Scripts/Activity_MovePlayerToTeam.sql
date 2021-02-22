ALTER TABLE [dbo].[Activities]
    ADD [MovePlayerToTeam] BIT CONSTRAINT [DF_Activities_MovePlayerToTeam] DEFAULT ((0)) NOT NULL;