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
CREATE TABLE dbo.AthleticLeagues
	(
	Id int NOT NULL IDENTITY (1, 1),
	Name nvarchar(50) NULL,
	SeasonId int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.AthleticLeagues ADD CONSTRAINT
	PK_AthleticLeagues PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.AthleticLeagues ADD CONSTRAINT
	FK_AthleticLeagues_Seasons FOREIGN KEY
	(
	SeasonId
	) REFERENCES dbo.Seasons
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.AthleticLeagues SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

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
ALTER TABLE dbo.AthleticLeagues SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Leagues ADD
	AthleticLeagueId int NULL
GO
ALTER TABLE dbo.Leagues ADD CONSTRAINT
	FK_Leagues_AthleticLeagues FOREIGN KEY
	(
	AthleticLeagueId
	) REFERENCES dbo.AthleticLeagues
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Leagues SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

ALTER TABLE dbo.Disciplines ADD DisciplineType nvarchar(50) NULL

ALTER TABLE dbo.CompetitionClubsCorrections ADD Points [decimal](18, 2) NULL
 