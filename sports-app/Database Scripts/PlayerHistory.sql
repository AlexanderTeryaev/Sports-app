CREATE TABLE dbo.PlayerHistory
	(
	Id int NOT NULL IDENTITY (1, 1),
	UserId int NOT NULL,
	TeamId int NOT NULL,
	SeasonId int NOT NULL,
	TimeStamp bigint NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.PlayerHistory ADD CONSTRAINT
	PK_PlayerHistory PRIMARY KEY CLUSTERED 
	(
	Id
	)

GO
ALTER TABLE dbo.PlayerHistory ADD CONSTRAINT
	FK_PlayerHistory_Users FOREIGN KEY
	(UserId	) REFERENCES dbo.Users 	(UserId	)
	
GO
ALTER TABLE dbo.PlayerHistory ADD CONSTRAINT
	FK_PlayerHistory_Seasons FOREIGN KEY
	(SeasonId) REFERENCES dbo.Seasons (Id)
	
GO
ALTER TABLE dbo.PlayerHistory ADD CONSTRAINT
	FK_PlayerHistory_Teams FOREIGN KEY
	(TeamId) REFERENCES dbo.Teams (TeamId)

