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
ALTER TABLE dbo.TeamsPlayers SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Teams SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Seasons SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Leagues SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.TennisCategoryPlayoffRanks
	(
	Id int NOT NULL IDENTITY (1, 1),
	CategoryId int NOT NULL,
	LeagueId int NOT NULL,
	SeasonId int NOT NULL,
	GroupName varchar(50) NOT NULL,
	Points int NULL,
	Correction int NULL,
	PlayerId int NULL,
	PairPlayerId int NULL,
	Rank int NULL,
	RealMaxPos int NULL,
	RealMinPos int NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TennisCategoryPlayoffRanks ADD CONSTRAINT
	PK_TennisCategoryPlayoffRanks PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.TennisCategoryPlayoffRanks ADD CONSTRAINT
	FK_TennisCategoryPlayoffRanks_Leagues FOREIGN KEY
	(
	LeagueId
	) REFERENCES dbo.Leagues
	(
	LeagueId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TennisCategoryPlayoffRanks ADD CONSTRAINT
	FK_TennisCategoryPlayoffRanks_Seasons FOREIGN KEY
	(
	SeasonId
	) REFERENCES dbo.Seasons
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TennisCategoryPlayoffRanks ADD CONSTRAINT
	FK_TennisCategoryPlayoffRanks_Teams FOREIGN KEY
	(
	CategoryId
	) REFERENCES dbo.Teams
	(
	TeamId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TennisCategoryPlayoffRanks ADD CONSTRAINT
	FK_TennisCategoryPlayoffRanks_TeamsPlayers FOREIGN KEY
	(
	PlayerId
	) REFERENCES dbo.TeamsPlayers
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TennisCategoryPlayoffRanks ADD CONSTRAINT
	FK_TennisCategoryPlayoffRanks_TeamsPlayers1 FOREIGN KEY
	(
	PairPlayerId
	) REFERENCES dbo.TeamsPlayers
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.TennisCategoryPlayoffRanks SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

ALTER TABLE dbo.Seasons ADD	MinimumParticipationRequired int NULL
