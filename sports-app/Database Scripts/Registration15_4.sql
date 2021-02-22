ALTER TABLE [dbo].[Activities]
    ADD [CreateClubTeam] BIT CONSTRAINT [DF_Activities_CreateClubTeam] DEFAULT ((0)) NOT NULL;