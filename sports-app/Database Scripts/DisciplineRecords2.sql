ALTER TABLE dbo.DisciplineRecords ADD
	Disciplines nvarchar(MAX) NULL
	
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
ALTER TABLE dbo.Disciplines
	DROP CONSTRAINT FK_Disciplines_DisciplineRecords
GO
ALTER TABLE dbo.DisciplineRecords SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Disciplines
	DROP COLUMN Record
GO
ALTER TABLE dbo.Disciplines SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
