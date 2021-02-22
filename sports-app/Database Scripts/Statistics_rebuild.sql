USE [LogLig]
GO
DROP TABLE [dbo].[Statistics]
GO

CREATE TABLE [dbo].[Statistics](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Abbreviation] [nchar](255) NULL,
	[GameId] [int] NOT NULL,
	[Point_x] [float] NOT NULL,
	[Point_y] [float] NOT NULL,
	[PlayerId] [int] NOT NULL,
	[TeamId] [int] NOT NULL,
	[Timestamp] [datetime2](7) NOT NULL,
	[GameTime] [bigint] NOT NULL,
 CONSTRAINT [PK_Statistics] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Statistics]  WITH CHECK ADD  CONSTRAINT [StatisticsTeamPlayers] FOREIGN KEY([PlayerId])
REFERENCES [dbo].[TeamsPlayers] ([Id])
GO

ALTER TABLE [dbo].[Statistics]  WITH CHECK ADD  CONSTRAINT [StatisticsGameCycle] FOREIGN KEY([GameId])
REFERENCES [dbo].[GamesCycles] ([CycleId])
GO

ALTER TABLE [dbo].[Statistics]  WITH CHECK ADD  CONSTRAINT [StatisticsTeam] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] ([TeamId])
GO

CREATE TABLE [dbo].[GameStatistics] (
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[GameId] [int] NOT NULL,
	[PlayerId] [int] NOT NULL,
	[MinutesPlayed] [bigint] NULL,
	[FG] [int] NULL,
	[FGA] [int] NULL,
	[ThreePT] [int] NULL,
	[ThreePA] [int] NULL,
	[TwoPT] [int] NULL,
	[TwoPA] [int] NULL,
	[FT] [int] NULL,
	[FTA] [int] NULL,
	[OREB] [int] NULL,
	[DREB] [int] NULL,
	[REB] [int] NULL,
	[TOT] [int] NULL,
	[AST] [int] NULL,
	[STL] [int] NULL,
	[BLK] [int] NULL,
	[TO] [int] NULL,
	[BS] [int] NULL,
	[PF] [int] NULL,
	[PTS] [int] NULL,
	[EFF] [float] NULL,
	[DIFF] [float] NULL,
	[FGM] [int] NULL,
	[FTM] [int] NULL



 CONSTRAINT [PK_Game_Statistics] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[GameStatistics]  WITH CHECK ADD  CONSTRAINT [GameStatisticsTeamPlayers] FOREIGN KEY([PlayerId])
REFERENCES [dbo].[TeamsPlayers] ([Id])
GO

ALTER TABLE [dbo].[GameStatistics]  WITH CHECK ADD  CONSTRAINT [GameStatisticsGameCycle] FOREIGN KEY([GameId])
REFERENCES [dbo].[GamesCycles] ([CycleId])
GO

ALTER TABLE [dbo].[Statistics] ADD IsProcessed BIT NOT NULL DEFAULT 0
ALTER TABLE [dbo].[GameStatistics] ADD TeamId INT NULL
ALTER TABLE [dbo].[GameStatistics]  WITH CHECK ADD CONSTRAINT [GameStatisticsTeam] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] (TeamId)