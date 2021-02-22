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
ALTER TABLE dbo.Leagues SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.PenaltyForExclusion ADD
	LeagueIds varchar(max) NULL
GO
ALTER TABLE dbo.PenaltyForExclusion SET (LOCK_ESCALATION = TABLE)
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
ALTER TABLE dbo.PenaltyForExclusion SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.GamesCycles ADD
	AppliedExclusionId int NULL
GO
ALTER TABLE dbo.GamesCycles ADD CONSTRAINT
	FK_GamesCycles_PenaltyForExclusion FOREIGN KEY
	(
	AppliedExclusionId
	) REFERENCES dbo.PenaltyForExclusion
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.GamesCycles SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
