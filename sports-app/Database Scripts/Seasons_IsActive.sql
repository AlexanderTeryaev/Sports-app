ALTER TABLE dbo.Seasons ADD
	IsActive bit NOT NULL CONSTRAINT DF_Seasons_IsActive DEFAULT 1
	
ALTER TABLE dbo.LeagueTeams ADD CONSTRAINT
	FK_LeagueTeams_Seasons FOREIGN KEY
	(
	SeasonId
	) REFERENCES dbo.Seasons
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 