
ALTER TABLE dbo.UsersJobs
	DROP CONSTRAINT FK_UsersJobs_Seasons
GO
ALTER TABLE dbo.UsersJobs
	DROP COLUMN SeasonId

