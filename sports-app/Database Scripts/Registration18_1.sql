ALTER TABLE [dbo].[Activities]
    ADD [RestrictGenders]     BIT CONSTRAINT [DF_Activities_RestrictGenders] DEFAULT ((0)) NOT NULL,
        [CheckCompetitionAge] BIT CONSTRAINT [DF_Activities_CheckCompetitionAge] DEFAULT ((0)) NOT NULL;