ALTER TABLE [dbo].[Teams] ADD IsReligiousTeam BIT NOT NULL DEFAULT 0

CREATE TABLE [dbo].[DaysForHosting]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LeagueId] [int] NOT NULL,
	[SeasonId] [int] NOT NULL,
	[Day] [int] NOT NULL,
	[StartTime] varchar(255) NOT NULL,
	[EndTime] varchar(255) NOT NULL
CONSTRAINT [PK_DaysForHosting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DaysForHosting]  WITH CHECK ADD  CONSTRAINT [FK_DaysForHosting_Leagues] FOREIGN KEY([LeagueId])
REFERENCES [dbo].[Leagues] ([LeagueId])
GO

ALTER TABLE [dbo].[DaysForHosting]  WITH CHECK ADD  CONSTRAINT [FK_DaysForHosting_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

CREATE TABLE [dbo].[TeamHostingDays]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TeamId] [int] NOT NULL,
	[HostingDayId] [int] NOT NULL
CONSTRAINT [PK_TeamHostingDays] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TeamHostingDays]  WITH CHECK ADD  CONSTRAINT [FK_TeamHostingDays_Teams] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] (TeamId)
GO

ALTER TABLE [dbo].[TeamHostingDays]  WITH CHECK ADD  CONSTRAINT [FK_TeamHostingDays_Hosting] FOREIGN KEY([HostingDayId])
REFERENCES [dbo].[DaysForHosting] (Id)
GO


ALTER TABLE [dbo].[Clubs] ADD [ClubInsurance] VARCHAR(255) NULL

ALTER TABLE [dbo].[Teams] ADD [IsUnionInsurance] BIT NULL
ALTER TABLE [dbo].[Teams] ADD [IsClubInsurance] BIT NULL
ALTER TABLE [dbo].[TeamsPlayers] ADD [TennisPositionOrder] INT NULl