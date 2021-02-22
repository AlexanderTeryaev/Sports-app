ALTER TABLE [dbo].[Activities]
    ADD [MultiTeamRegistrations] BIT CONSTRAINT [DF_Activities_MultiTeamRegistrations] DEFAULT ((0)) NOT NULL;