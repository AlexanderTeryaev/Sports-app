alter table Leagues add TeamRegistrationPrice decimal(18,2) null
GO

alter table Leagues add PlayerRegistrationPrice decimal(18,2) null
GO

alter table Leagues add PlayerInsurancePrice decimal(18,2) null
GO

alter table Leagues add MaximumAge datetime null
GO

alter table Leagues add MinimumAge datetime null
GO

alter table Leagues add MinimumPlayersTeam int null
GO

alter table Leagues add MaximumPlayersTeam int null
GO

alter table Leagues add LeagueCode nvarchar(30) null
GO

alter table Teams add NeedShirts bit null default(0)
GO

alter table Teams add InsuranceApproval nvarchar(50) null
GO

alter table Teams add HasArena bit null default(0)
GO

CREATE TABLE dbo.TeamBenefactors
	(
	BenefactorId int NOT NULL IDENTITY (1, 1),
	TeamId int NOT NULL,
	Name nvarchar(500) NOT NULL,
	PlayerCreditAmount decimal(18, 2) NULL,
	MaximumPlayersFunded decimal(18, 2) NULL,
	FinancingInsurance bit NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.TeamBenefactors ADD CONSTRAINT
	PK_TeamBenefactors PRIMARY KEY CLUSTERED 
	(
	BenefactorId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO


ALTER TABLE dbo.TeamBenefactors ADD CONSTRAINT
	FK_TeamBenefactors_Teams FOREIGN KEY
	(
	TeamId
	) REFERENCES dbo.Teams
	(
	TeamId
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO

ALTER TABLE dbo.TeamBenefactors ADD IsApproved bit NULL
GO

ALTER TABLE dbo.TeamBenefactors ADD CreatedDate datetime
GO

alter table dbo.TeamBenefactors add ApprovedUserId int null
GO

alter table dbo.TeamBenefactors add ApprovedDate datetime null
GO


ALTER TABLE dbo.SentMessages ADD
	UserId int NULL
GO
ALTER TABLE dbo.SentMessages ADD CONSTRAINT
	FK_SentMessages_Users FOREIGN KEY
	(
	UserId
	) REFERENCES dbo.Users
	(
	UserId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO


ALTER TABLE dbo.TeamBenefactors
	DROP COLUMN MaximumPlayersFunded
GO

ALTER TABLE dbo.TeamBenefactors ADD
	MaximumPlayersFunded int NULL
GO


ALTER TABLE dbo.TeamsPlayers ADD
	IsLocked bit NULL
GO
--applied