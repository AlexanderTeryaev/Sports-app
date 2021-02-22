ALTER TABLE [dbo].[Activities]
    ADD [RegistrationForMembers] INT CONSTRAINT [DF_Activities_AllowOnlyCompetitiveMembers] DEFAULT ((0)) NOT NULL;