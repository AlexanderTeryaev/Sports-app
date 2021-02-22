ALTER TABLE [dbo].[Activities]
    ADD [RestrictSchools]   BIT           CONSTRAINT [DF_Activities_RestrictSchools] DEFAULT ((0)) NOT NULL,
        [RestrictedSchools] VARCHAR (MAX) NULL;