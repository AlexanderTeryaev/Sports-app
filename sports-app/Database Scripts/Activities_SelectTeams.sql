ALTER TABLE [dbo].[Activities]
    ADD [RestrictTeams]   BIT            CONSTRAINT [DF_Activities_RestrictTeams] DEFAULT ((0)) NOT NULL,
        [RestrictedTeams] NVARCHAR (MAX) NULL;