GO

ALTER TABLE UsersJobs ADD SeasonId INT NULL

GO

ALTER TABLE UsersJobs ADD CONSTRAINT FK_Seasons_UserJobs FOREIGN KEY (SeasonId) REFERENCES Seasons(Id);

GO


GO
ALTER TABLE dbo.Leagues ADD
	AboutLeague nvarchar(2000) NULL,
	LeagueStructure nvarchar(3000) NULL


ALTER TABLE dbo.Teams ADD
	GamesUrl nvarchar(500) NULL


GO

CREATE TABLE [dbo].[TeamStandingGames](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GamesUrl] [nvarchar](350) NOT NULL,
	[TeamId] [int] NOT NULL,
	[ClubId] [int] NULL,
 CONSTRAINT [PK_TeamStandingGames] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[TeamStandingGames]  WITH CHECK ADD  CONSTRAINT [FK_TeamStandingGames_Clubs] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO

ALTER TABLE [dbo].[TeamStandingGames] CHECK CONSTRAINT [FK_TeamStandingGames_Clubs]
GO

ALTER TABLE [dbo].[TeamStandingGames]  WITH CHECK ADD  CONSTRAINT [FK_TeamStandingGames_Teams] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] ([TeamId])
GO

ALTER TABLE [dbo].[TeamStandingGames] CHECK CONSTRAINT [FK_TeamStandingGames_Teams]
GO

GO

CREATE TABLE [dbo].[TeamStandings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Rank] [int] NOT NULL,
	[Team] [nvarchar](90) NOT NULL,
	[Games] [tinyint] NOT NULL,
	[Wins] [tinyint] NOT NULL,
	[Lost] [tinyint] NOT NULL,
	[Pts] [int] NOT NULL,
	[Papf] [nvarchar](50) NOT NULL,
	[Home] [nvarchar](50) NOT NULL,
	[Road] [nvarchar](50) NOT NULL,
	[ScoreHome] [nvarchar](50) NOT NULL,
	[ScoreRoad] [nvarchar](50) NOT NULL,
	[Last5] [nvarchar](50) NOT NULL,
	[TeamStandingGamesId] [int] NULL,
 CONSTRAINT [PK_TeamStandings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[TeamStandings] ADD  CONSTRAINT [DF_TeamStandings_Games]  DEFAULT ((0)) FOR [Games]
GO

ALTER TABLE [dbo].[TeamStandings] ADD  CONSTRAINT [DF_TeamStandings_Wins]  DEFAULT ((0)) FOR [Wins]
GO

ALTER TABLE [dbo].[TeamStandings] ADD  CONSTRAINT [DF_TeamStandings_Lost]  DEFAULT ((0)) FOR [Lost]
GO

ALTER TABLE [dbo].[TeamStandings] ADD  CONSTRAINT [DF_TeamStandings_Pts]  DEFAULT ((0)) FOR [Pts]
GO

ALTER TABLE [dbo].[TeamStandings] ADD  CONSTRAINT [DF_TeamStandings_Papf]  DEFAULT (N'0/0') FOR [Papf]
GO

ALTER TABLE [dbo].[TeamStandings] ADD  CONSTRAINT [DF_TeamStandings_Home]  DEFAULT (N'0/0') FOR [Home]
GO

ALTER TABLE [dbo].[TeamStandings] ADD  CONSTRAINT [DF_TeamStandings_Road]  DEFAULT (N'0/0') FOR [Road]
GO

ALTER TABLE [dbo].[TeamStandings] ADD  CONSTRAINT [DF_TeamStandings_ScoreHome]  DEFAULT (N'-/-') FOR [ScoreHome]
GO

ALTER TABLE [dbo].[TeamStandings] ADD  CONSTRAINT [DF_TeamStandings_ScoreRoad]  DEFAULT (N'-/-') FOR [ScoreRoad]
GO

ALTER TABLE [dbo].[TeamStandings] ADD  CONSTRAINT [DF_TeamStandings_Last5]  DEFAULT (N'0/0') FOR [Last5]
GO

ALTER TABLE [dbo].[TeamStandings]  WITH CHECK ADD  CONSTRAINT [FK_TeamStandings_TeamStandingGames] FOREIGN KEY([TeamStandingGamesId])
REFERENCES [dbo].[TeamStandingGames] ([Id])
GO

ALTER TABLE [dbo].[TeamStandings] CHECK CONSTRAINT [FK_TeamStandings_TeamStandingGames]
GO

GO
ALTER TABLE [dbo].[TeamStandings] ADD
 [PlusMinusField] [nvarchar](50) NOT NULL DEFAULT (N'')


GO
 UPDATE tp
 SET    tp.SeasonId = 11
 FROM   ClubTeams ct
 JOIN   TeamsPlayers tp ON ct.TeamId = tp.TeamId
 JOIN   Seasons s ON ct.SeasonId = s.Id
 JOIN   Clubs c ON s.ClubId = c.ClubId
 JOIN   Sections sc ON c.SectionId = sc.SectionId
 WHERE  ct.SeasonId = (SELECT ses.Id FROM Seasons ses WHERE ses.ClubId = ct.ClubId)
 AND    sc.Alias = 'basketball'
 
GO
DECLARE @ExistTeamId int
DECLARE @ExistUserId int
DECLARE @ExistShirtNum int
DECLARE @ExistPosId int
DECLARE @ExistIsActive bit

DECLARE TP_CURSOR CURSOR
 LOCAL STATIC READ_ONLY FORWARD_ONLY

 FOR
 Select tp.TeamId, tp.UserId, tp.PosId, tp.ShirtNum, tp.IsActive
 FROM   ClubTeams ct
 JOIN   TeamsPlayers tp ON ct.TeamId = tp.TeamId
 JOIN   Seasons s ON ct.SeasonId = s.Id
 JOIN   Clubs c ON s.ClubId = c.ClubId
 JOIN   Sections sc ON c.SectionId = sc.SectionId
 WHERE  ct.SeasonId = (SELECT ses.Id FROM Seasons ses WHERE ses.ClubId = ct.ClubId)
 AND    sc.Alias = 'basketball'

 OPEN TP_CURSOR
  FETCH NEXT FROM TP_CURSOR INTO @ExistTeamId, @ExistUserId, @ExistPosId, @ExistShirtNum, @ExistIsActive 
WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO [LogLig].[dbo].[TeamsPlayers] VALUES (@ExistTeamId, @ExistUserId, @ExistPosId, @ExistShirtNum, 1, 13, 0 , 1)

		FETCH NEXT FROM TP_CURSOR INTO @ExistTeamId, @ExistUserId, @ExistPosId, @ExistShirtNum, @ExistIsActive 
	END

CLOSE TP_CURSOR
DEALLOCATE TP_CURSOR



GO
CREATE TABLE [dbo].[TeamScheduleScrapperGames](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GameUrl] [nvarchar](150) NOT NULL,
	[TeamId] [int] NULL,
	[ClubId] [int] NULL,
 CONSTRAINT [PK_TeamScheduleScrapperGames] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

GO
CREATE TABLE [dbo].[TeamScheduleScrapper](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[HomeTeam] [nvarchar](100) NOT NULL,
	[GuestTeam] [nvarchar](100) NOT NULL,
	[Score] [nvarchar](50) NOT NULL,
	[Auditorium] [nvarchar](50) NOT NULL,
	[SchedulerScrapperGamesId] [int] NOT NULL,
 CONSTRAINT [PK_TeamScheduleScrapper] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[TeamScheduleScrapper]  WITH CHECK ADD  CONSTRAINT [FK_TeamScheduleScrapper_TeamScheduleScrapperGames] FOREIGN KEY([SchedulerScrapperGamesId])
REFERENCES [dbo].[TeamScheduleScrapperGames] ([Id])
GO

ALTER TABLE [dbo].[TeamScheduleScrapper] CHECK CONSTRAINT [FK_TeamScheduleScrapper_TeamScheduleScrapperGames]
GO



GO
 ALTER TABLE  [dbo].[TeamScheduleScrapperGames]
	ADD [ExternalTeamName] NVARCHAR(80)NULL


GO
ALTER TABLE [dbo].[TeamStandingGames]
	ADD [ExternalTeamName] NVARCHAR (80) NULL

