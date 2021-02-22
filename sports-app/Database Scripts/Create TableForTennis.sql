USE [LogLig]
GO

/****** Object:  Table [dbo].[CompetitionAges]    Script Date: 3/22/2018 3:04:59 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CompetitionAges](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[age_name] [varchar](50) NOT NULL,
	[from_birth] [date] NOT NULL,
	[to_birth] [date] NOT NULL,
 CONSTRAINT [PK_CompetitionAges] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

USE [LogLig]
GO

/****** Object:  Table [dbo].[CompetitionLevel]    Script Date: 3/22/2018 3:05:12 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CompetitionLevel](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[level_name] [varchar](50) NULL,
 CONSTRAINT [PK_CompetitionLevel] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

USE [LogLig]
GO

/****** Object:  Table [dbo].[TennisRank]    Script Date: 3/22/2018 3:05:34 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TennisRank](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[Rank] [int] NULL,
	[Points] [int] NULL,
 CONSTRAINT [PK_TennisRank] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

USE [LogLig]
GO

/****** Object:  Table [dbo].[CompetitionRegion]    Script Date: 3/22/2018 3:05:22 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CompetitionRegion](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[region_name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_CompetitionRegion] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

Alter TABLE Teams
  ADD CompetitionAgeId Int
  Constraint FK_Competition_Ages foreign key (CompetitionAgeId) references dbo.CompetitionAges (id)


Alter TABLE Teams
  ADD GenderId Int
  Constraint FK_Competition_Genders foreign key (GenderId) references dbo.Genders (GenderId)
 
Alter TABLE Teams
  ADD LevelId Int
  Constraint FK_Competition_Levels foreign key (LevelId) references dbo.CompetitionLevel (id)

ALTER TABLE dbo.Leagues
  ADD MinParticipationReq INT

ALTER TABLE dbo.Teams
  ADD MinRank INT,
      MaxRank INT,
	  PlaceForQualification VARCHAR(255),
	  PlaceForFinal VARCHAR(255)

Alter TABLE Teams
  ADD RegionId Int
  Constraint FK_Competition_Regions foreign key (RegionId) references dbo.CompetitionRegion (id)



