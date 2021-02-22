ALTER TABLE [dbo].[Activities] DROP CONSTRAINT [FK_Activities_Unions];

ALTER TABLE [dbo].[ActivityBranches] DROP CONSTRAINT [FK_ActivityBranches_Unions];

ALTER TABLE [dbo].[Activities] ALTER COLUMN [UnionId] INT NULL;

ALTER TABLE [dbo].[Activities]
    ADD [ClubId] INT NULL;

ALTER TABLE [dbo].[ActivityBranches] ALTER COLUMN [UnionId] INT NULL;

ALTER TABLE [dbo].[ActivityBranches]
    ADD [ClubId] INT NULL;

ALTER TABLE [dbo].[Activities] WITH NOCHECK
    ADD CONSTRAINT [FK_Activities_Unions] FOREIGN KEY ([UnionId]) REFERENCES [dbo].[Unions] ([UnionId]) ON DELETE CASCADE;

ALTER TABLE [dbo].[ActivityBranches] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityBranches_Unions] FOREIGN KEY ([UnionId]) REFERENCES [dbo].[Unions] ([UnionId]);

ALTER TABLE [dbo].[Activities] WITH NOCHECK
    ADD CONSTRAINT [FK_Activities_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);

ALTER TABLE [dbo].[Activities] WITH CHECK CHECK CONSTRAINT [FK_Activities_Unions];

ALTER TABLE [dbo].[ActivityBranches] WITH CHECK CHECK CONSTRAINT [FK_ActivityBranches_Unions];

ALTER TABLE [dbo].[Activities] WITH CHECK CHECK CONSTRAINT [FK_Activities_Clubs];