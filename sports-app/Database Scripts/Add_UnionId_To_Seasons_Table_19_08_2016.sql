GO
ALTER TABLE [dbo].[Seasons]
    ADD [UnionId] INT NOT NULL;

GO
PRINT N'Creating [dbo].[FK_Seasons_Unions]...';

GO
ALTER TABLE [dbo].[Seasons] WITH NOCHECK
    ADD CONSTRAINT [FK_Seasons_Unions] FOREIGN KEY ([UnionId]) REFERENCES [dbo].[Unions] ([UnionId]);

GO
PRINT N'Checking existing data against newly created constraints';

GO
ALTER TABLE [dbo].[Seasons] WITH CHECK CHECK CONSTRAINT [FK_Seasons_Unions];

GO
PRINT N'Update complete.';

GO