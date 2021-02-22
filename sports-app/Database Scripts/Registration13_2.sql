ALTER TABLE [dbo].[ActivityFormsSubmittedData] DROP CONSTRAINT [FK_ActivityFormsSubmittedData_Teams];

ALTER TABLE [dbo].[Activities]
    ADD [NoTeamRegistration] BIT CONSTRAINT [DF_Activities_NoTeamRegistration] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData] ALTER COLUMN [TeamId] INT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_Teams] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]);

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_Teams];