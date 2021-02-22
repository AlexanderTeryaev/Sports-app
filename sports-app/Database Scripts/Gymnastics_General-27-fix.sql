delete from TeamsRanks
GO

delete from TeamsRoutes
GO

alter table TeamsRoutes add UserId int not null
GO

alter table TeamsRanks add UserId int not null
GO


ALTER TABLE dbo.TeamsRanks ADD CONSTRAINT
	FK_TeamsRanks_Users FOREIGN KEY
	(
		UserId
	) REFERENCES dbo.Users
	(
		UserId
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
GO

ALTER TABLE dbo.TeamsRoutes ADD CONSTRAINT
	FK_TeamsRoutes_Users FOREIGN KEY
	(
	UserId
	) REFERENCES dbo.Users
	(
	UserId
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO