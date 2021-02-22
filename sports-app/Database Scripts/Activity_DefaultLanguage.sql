ALTER TABLE [dbo].[Activities]
    ADD [DefaultLanguage] NVARCHAR (MAX) CONSTRAINT [DF_Activities_DefaultLanguage] DEFAULT (N'he') NOT NULL;