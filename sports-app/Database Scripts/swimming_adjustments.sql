ALTER TABLE [dbo].[Users] ADD [ClassS] int NULL;
ALTER TABLE [dbo].[Users] ADD [ClassSB] int NULL;
ALTER TABLE [dbo].[Users] ADD [ClassSM] int NULL;

ALTER TABLE dbo.Auditoriums ADD	LanesNumber int NULL, Length int NULL
	
ALTER TABLE dbo.Leagues ADD	CompetitionAuditoriumId int NULL

ALTER TABLE dbo.Leagues ADD CONSTRAINT
	FK_Competition_Auditoriums FOREIGN KEY
	(
	CompetitionAuditoriumId
	) REFERENCES dbo.Auditoriums
	(
	AuditoriumId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	 
	 
ALTER TABLE dbo.Disciplines ADD	Class int NULL
	

