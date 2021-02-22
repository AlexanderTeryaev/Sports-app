ALTER TABLE [dbo].[GamesCycles] ADD IsDateUpdated BIT NOT NULL DEFAULT 0
ALTER TABLE [dbo].[Leagues] ADD FiveHandicapReduction FLOAT NULL
ALTER TABLE [dbo].[Leagues] ADD IsPositionSettingsEnabled BIT NOT NULL DEFAULT 0

CREATE TABLE [dbo].[PositionSettings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LeagueId] [int] NOT NULL,
	[SeasonId] [int] NOT NULL,
	[Position] [int] NOT NULL,
	[Points] [int] NULL

 CONSTRAINT [PK_PositionSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


ALTER TABLE [dbo].[PositionSettings]  WITH CHECK ADD  CONSTRAINT [FK_PositionSettings_Leagues] FOREIGN KEY([LeagueId])
REFERENCES [dbo].[Leagues] ([LeagueId])
GO

ALTER TABLE [dbo].[PositionSettings]  WITH CHECK ADD  CONSTRAINT [FK_PositionSettings_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

CREATE TABLE [dbo].[RefereeRegistrations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClubId] [int] NOT NULL,
	[LeagueId] [int] NOT NULL,
	[SeasonId] [int] NOT NULL,
	[RefereeId] [int] NOT NULL,
	[IsArchive] [bit] NOT NULL DEFAULT 0

 CONSTRAINT [PK_RefereeRegistrations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RefereeRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_RefereeRegistrations_Leagues] FOREIGN KEY([LeagueId])
REFERENCES [dbo].[Leagues] ([LeagueId])
GO

ALTER TABLE [dbo].[RefereeRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_RefereeRegistrations_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[RefereeRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_RefereeRegistrations_Clubs] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO

ALTER TABLE [dbo].[RefereeRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_RefereeRegistrations_Referees] FOREIGN KEY([RefereeId])
REFERENCES [dbo].[UsersJobs] ([Id])
GO

CREATE TABLE [dbo].[Instruments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DisciplineId] [int] NOT NULL,
	[SeasonId] [int] NOT NULL,
	[Name] varchar(255) NOT NULL

 CONSTRAINT [PK_Instruments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Instruments]  WITH CHECK ADD  CONSTRAINT [FK_Instruments_Discipline] FOREIGN KEY([DisciplineId])
REFERENCES [dbo].[Disciplines] ([DisciplineId])
GO

ALTER TABLE [dbo].[Instruments]  WITH CHECK ADD  CONSTRAINT [FK_Instruments_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[CompetitionRoutes] ADD [InstrumentId] INT NULL

ALTER TABLE [dbo].[CompetitionRoutes]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionRoutes_Instruments] FOREIGN KEY([InstrumentId])
REFERENCES [dbo].[Instruments] ([Id])
GO

ALTER TABLE [dbo].[CompetitionRegistrations] ADD [OrderNumber] INT NULL

ALTER TABLE [dbo].[CompetitionRegistrations] ADD [FinalScore] FLOAT NULL

ALTER TABLE [dbo].[CompetitionRegistrations] ADD [Position] INT NULL

ALTER TABLE [dbo].[CompetitionRoutes] ADD [SecondComposition] INT NULL

ALTER TABLE [dbo].[CompetitionRegistrations] ADD [IsRegisteredInSecondComposition] BIT NULL

ALTER TABLE [dbo].[AdditionalGymnastics] ADD [IsRegisteredInSecondComposition] BIT NOT NULL DEFAULT 0