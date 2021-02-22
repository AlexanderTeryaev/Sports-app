/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.CompetitionDisciplines SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.CompetitionDisciplineHeatStartTimes
	(
	Id int NOT NULL IDENTITY (1, 1),
	CompetitionDisciplineId int NOT NULL,
	HeatName varchar(MAX) NOT NULL,
	StartTime datetime NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.CompetitionDisciplineHeatStartTimes ADD CONSTRAINT
	PK_CompetitionDisciplineHeatStartTimes PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.CompetitionDisciplineHeatStartTimes ADD CONSTRAINT
	FK_CompetitionDisciplineHeatStartTimes_CompetitionDisciplines FOREIGN KEY
	(
	CompetitionDisciplineId
	) REFERENCES dbo.CompetitionDisciplines
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CompetitionDisciplineHeatStartTimes SET (LOCK_ESCALATION = TABLE)
GO
COMMIT