ALTER TABLE [dbo].[Activities]
    ADD [ClubLeagueTeamsOnly] BIT CONSTRAINT [DF_Activities_ClubLeagueTeamsOnly] DEFAULT ((0)) NOT NULL;