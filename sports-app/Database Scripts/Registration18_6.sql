ALTER TABLE [dbo].[Activities]
    ADD [RestrictGendersByTeam] BIT CONSTRAINT [DF_Activities_RestrictGendersByTeam] DEFAULT ((0)) NOT NULL;