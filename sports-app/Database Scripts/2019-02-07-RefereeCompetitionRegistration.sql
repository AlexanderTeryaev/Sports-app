EXEC sp_rename '[dbo].[RefereeCompetitionRegistrations].[SessionId]', 'SessionIds', 'COLUMN'
GO

ALTER TABLE [dbo].[RefereeCompetitionRegistrations] ALTER COLUMN [SessionIds] varchar(255) NULL