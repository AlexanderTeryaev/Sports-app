ALTER TABLE [dbo].[Activities]
    ADD [ShowOnlyApprovedTeams] BIT CONSTRAINT [DF_Activities_ShowOnlyApprovedTeams] DEFAULT ((0)) NOT NULL;