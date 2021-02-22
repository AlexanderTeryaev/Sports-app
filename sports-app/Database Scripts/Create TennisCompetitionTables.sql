USE [LogLig]
GO

/****** Object:  Table [dbo].[TennisGameCycles]    Script Date: 4/25/2018 6:03:47 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TennisGameCycles](
	[CycleId] [int] IDENTITY(1,1) NOT NULL,
	[StageId] [int] NOT NULL,
	[CycleNum] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[FieldId] [int] NULL,
	[FirstPlayerId] [int] NULL,
	[FirstPlayerScore] [int] NOT NULL,
	[SecondPlayerId] [int] NULL,
	[SecondPlayerScore] [int] NOT NULL,
	[RefereeIds] [varchar](255) NULL,
	[GroupId] [int] NULL,
	[GameStatus] [nvarchar](50) NULL,
	[TechnicalWinnerId] [int] NULL,
	[MaxPlayoffPos] [int] NULL,
	[MinPlayoffPos] [int] NULL,
	[BracketId] [int] NULL,
	[IsPublished] [bit] NOT NULL,
	[BracketIndex] [int] NULL,
	[SpectatorIds] [nvarchar](255) NULL,
	[PdfGameReport] [varchar](255) NULL,
	[Note] [varchar](max) NULL,
	[IsDateUpdated] [bit] NOT NULL,
	[FirstPlayerPos] [int] NULL,
	[SecondPlayerPos] [int] NULL,
 CONSTRAINT [PK_TennisGameCycles] PRIMARY KEY CLUSTERED 
(
	[CycleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO



USE [LogLig]
GO

USE [LogLig]
GO

/****** Object:  Table [dbo].[TennisStages]    Script Date: 4/25/2018 6:09:18 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TennisStages](
	[StageId] [int] IDENTITY(1,1) NOT NULL,
	[Number] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[IsArchive] [bit] NOT NULL,
	[SeasonId] [int] NOT NULL,
 CONSTRAINT [PK_TennisStages] PRIMARY KEY CLUSTERED 
(
	[StageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO





/****** Object:  Table [dbo].[TennisGames]    Script Date: 4/25/2018 6:04:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TennisGames](
	[GameId] [int] IDENTITY(1,1) NOT NULL,
	[CategoryId] [int] NOT NULL,
	[GameDays] [varchar](20) NULL,
	[StartDate] [datetime] NOT NULL,
	[GamesInterval] [char](5) NULL,
	[PointsWin] [int] NOT NULL,
	[PointsDraw] [int] NOT NULL,
	[PointsLoss] [int] NOT NULL,
	[PointTechWin] [int] NOT NULL,
	[SortDescriptions] [varchar](20) NOT NULL,
	[PointsTechLoss] [int] NOT NULL,
	[StageId] [int] NULL,
	[SeasonId] [int] NULL,
	[ActiveWeeksNumber] [int] NOT NULL,
	[BreakWeeksNumber] [int] NOT NULL,
 CONSTRAINT [PK_TennisGames] PRIMARY KEY CLUSTERED 
(
	[GameId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



USE [LogLig]
GO

/****** Object:  Table [dbo].[TennisGameSets]    Script Date: 4/25/2018 6:05:07 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TennisGameSets](
	[GameSetId] [int] IDENTITY(1,1) NOT NULL,
	[FirstPlayerScore] [int] NOT NULL,
	[SecondPlayerScore] [int] NOT NULL,
	[GameCycleId] [int] NOT NULL,
	[SetNumber] [int] NOT NULL,
 CONSTRAINT [PK_TennisGameSets] PRIMARY KEY CLUSTERED 
(
	[GameSetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



USE [LogLig]
GO

/****** Object:  Table [dbo].[TennisGroups]    Script Date: 4/25/2018 6:05:32 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TennisGroups](
	[GroupId] [int] IDENTITY(1,1) NOT NULL,
	[StageId] [int] NOT NULL,
	[TypeId] [int] NOT NULL,
	[Name] [nvarchar](120) NOT NULL,
	[IsArchive] [bit] NOT NULL,
	[NumberOfCycles] [int] NULL,
	[IsAdvanced] [bit] NOT NULL,
	[PointEditType] [int] NULL,
	[SeasonId] [int] NULL,
	[IsIndividual] [bit] NOT NULL,
	[NumberOfPlayers] [int] NULL,
 CONSTRAINT [PK_TennisGroups] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



USE [LogLig]
GO

/****** Object:  Table [dbo].[TennisGroupTeams]    Script Date: 4/25/2018 6:05:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TennisGroupTeams](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NOT NULL,
	[Pos] [int] NOT NULL,
	[MaxPlayoffPos] [int] NULL,
	[MinPlayoffPos] [int] NULL,
	[Points] [int] NULL,
	[SeasonId] [int] NULL,
	[PlayerId] [int] NULL,
	[TeamId] [int] NULL,
 CONSTRAINT [PK_TennisGroupTeams] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



USE [LogLig]
GO

/****** Object:  Table [dbo].[TennisPlayoffBrackets]    Script Date: 4/25/2018 6:07:36 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TennisPlayoffBrackets](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MaxPos] [int] NOT NULL,
	[MinPos] [int] NOT NULL,
	[WinnerId] [int] NULL,
	[LoserId] [int] NULL,
	[GroupId] [int] NOT NULL,
	[ParentBracket1Id] [int] NULL,
	[ParentBracket2Id] [int] NULL,
	[Type] [int] NOT NULL,
	[Stage] [int] NOT NULL,
	[FirstPlayerId] [int] NULL,
	[SecondPlayerId] [int] NULL,
 CONSTRAINT [PK_TennisPlayoffBrackets] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



USE [LogLig]
GO

/****** Object:  Table [dbo].[TennisPointsSettings]    Script Date: 4/25/2018 6:08:37 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TennisPointsSettings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LevelId] [int] NOT NULL,
	[SeasonId] [int] NOT NULL,
	[Rank] [int] NOT NULL,
	[Points] [int] NOT NULL,
 CONSTRAINT [PK_TennisPointsSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



