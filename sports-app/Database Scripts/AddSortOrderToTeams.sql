ALTER TABLE [dbo].[Teams]
    ADD [SortOrder] SMALLINT CONSTRAINT [DF_Teams_SortOrder] DEFAULT ((0)) NOT NULL
GO