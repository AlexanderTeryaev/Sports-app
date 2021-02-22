ALTER TABLE [dbo].[Activities]
    ADD [OnlyApprovedClubs] BIT CONSTRAINT [DF_Activities_OnlyApprovedClubs] DEFAULT ((0)) NOT NULL;