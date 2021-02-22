ALTER TABLE [dbo].[Activities]
    ADD [RestrictLeaguesByAge] BIT CONSTRAINT [DF_Activities_RestrictLeaguesByAge] DEFAULT ((0)) NOT NULL;