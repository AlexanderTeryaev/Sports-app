ALTER TABLE [dbo].[Activities]
    ADD [ForbidToChangeNameForExistingPlayers] BIT CONSTRAINT [DF_Activities_ForbitToChangeNameForExistingPlayers] DEFAULT ((0)) NOT NULL;