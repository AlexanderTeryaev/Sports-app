UPDATE [dbo].[LeagueTeams] SET SeasonId=(SELECT SeasonId FROM [dbo].[Leagues] WHERE [dbo].[Leagues].[LeagueId]=[dbo].[LeagueTeams].[LeagueId]) WHERE [dbo].[LeagueTeams].[SeasonId] IS NULL;
--UPDATE [dbo].[TeamsPlayers] SET SeasonId=(SELECT TOP 1 SeasonId FROM [dbo].[LeagueTeams] WHERE [dbo].[Leagueteams].[TeamId]=[dbo].[TeamsPlayers].[TeamId]) WHERE [dbo].[TeamsPlayers].[SeasonId] IS NULL;

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


CREATE TABLE [dbo].[TeamsDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TeamId] [int] NOT NULL,
	[SeasonId] [int] NOT NULL,
	[TeamName] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_TeamsSeason] PRIMARY KEY CLUSTERED 
(
	[Id] ASC,
	[TeamId] ASC,
	[SeasonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[TeamsDetails]  WITH CHECK ADD  CONSTRAINT [FK_TeamsDetails_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[TeamsDetails] CHECK CONSTRAINT [FK_TeamsDetails_Seasons]
GO

ALTER TABLE [dbo].[TeamsDetails]  WITH CHECK ADD  CONSTRAINT [FK_TeamsSeason_Teams] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] ([TeamId])
GO

ALTER TABLE [dbo].[TeamsDetails] CHECK CONSTRAINT [FK_TeamsSeason_Teams]
GO

ALTER TABLE UsersJobs ADD ClubId INT NULL

GO

ALTER TABLE [dbo].[Leagues] ADD ClubId INT NULL
GO  

ALTER TABLE [dbo].[Leagues]  WITH CHECK ADD  CONSTRAINT [FK_Leagues_Clubs] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO

ALTER TABLE [dbo].[Leagues] CHECK CONSTRAINT [FK_Leagues_Clubs]
GO

ALTER TABLE [dbo].[Leagues] ALTER COLUMN UnionId INT NULL
GO

ALTER TABLE [dbo].[Leagues] ALTER COLUMN UnionId INT NULL
GO

ALTER TABLE [dbo].[Auditoriums] ADD ClubId int NULL
GO

ALTER TABLE [dbo].[Auditoriums] WITH CHECK ADD CONSTRAINT [FK_Auditoriums_Clubs] FOREIGN KEY (ClubId) 
REFERENCES dbo.Clubs(ClubId) 
GO

ALTER TABLE [dbo].[Auditoriums] CHECK CONSTRAINT [FK_Auditoriums_Clubs]
GO

