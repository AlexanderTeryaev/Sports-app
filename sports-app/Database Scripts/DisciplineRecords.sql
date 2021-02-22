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
ALTER TABLE dbo.Disciplines SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.CompetitionAges SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.DisciplineRecords
	(
	Id int NOT NULL IDENTITY (1, 1),
	Name nvarchar(50) NOT NULL,
	Format int NOT NULL,
	Categories nvarchar(MAX) NULL,
	IntentionalIsraeliRecord nvarchar(50) NULL,
	IsraeliRecord nvarchar(50) NULL,
	SeasonRecord nvarchar(50) NULL,
	IntentionalIsraeliRecordSortValue bigint NULL,
	IsraeliRecordSortValue bigint NULL,
	SeasonRecordSortValue bigint NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.DisciplineRecords ADD CONSTRAINT
	PK_DisciplineRecords PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

ALTER TABLE dbo.DisciplineRecords SET (LOCK_ESCALATION = TABLE)
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
ALTER TABLE dbo.DisciplineRecords SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Disciplines ADD
	Record int NULL
GO
ALTER TABLE dbo.Disciplines ADD CONSTRAINT
	FK_Disciplines_DisciplineRecords FOREIGN KEY
	(
	Record
	) REFERENCES dbo.DisciplineRecords
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Disciplines SET (LOCK_ESCALATION = TABLE)
GO
COMMIT



BEGIN TRANSACTION
GO
ALTER TABLE dbo.DisciplineRecords ADD
	UnionId int NOT NULL
GO
ALTER TABLE dbo.DisciplineRecords ADD CONSTRAINT
	FK_DisciplineRecords_Unions FOREIGN KEY
	(
	UnionId
	) REFERENCES dbo.Unions
	(
	UnionId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT



ALTER TABLE dbo.CompetitionDisciplines ADD
	CompetitionRecord nvarchar(50) NULL,
	CompetitionRecordSortValue bigint NULL

ALTER TABLE dbo.CompetitionDisciplines ADD
	IncludeRecordInStartList bit NOT NULL Default(0)

