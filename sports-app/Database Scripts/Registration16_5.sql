ALTER TABLE [dbo].[Activities]
    ADD [DoNotAllowDuplicateRegistrations] BIT CONSTRAINT [DF_Activities_DoNotAllowDuplicateRegistrations] DEFAULT ((0)) NOT NULL;