ALTER TABLE [dbo].[Schools]
    ADD [SortOrder] SMALLINT CONSTRAINT [DF_Schools_SortOrder] DEFAULT ((0)) NOT NULL
GO