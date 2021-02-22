ALTER TABLE [dbo].[Leagues] ADD AllowedCLubsIds VARCHAR(MAX) NULL

CREATE TABLE [dbo].[TeamRegistrations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TeamId] [int] NOT NULL,
	[ClubId] [int] NOT NULL,
	[LeagueId] [int] NOT NULL,
	[SeasonId] [int] NULL

 CONSTRAINT [PK_TeamRegistrations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


ALTER TABLE [dbo].[TeamRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_TeamRegistrations_Teams] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] ([TeamId])
GO

ALTER TABLE [dbo].[TeamRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_TeamRegistrations_Clubs] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO

ALTER TABLE [dbo].[TeamRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_TeamRegistrations_Leagues] FOREIGN KEY([LeagueId])
REFERENCES [dbo].[Leagues] ([LeagueId])
GO

ALTER TABLE [dbo].[TeamRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_TeamRegistrations_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[TeamsPlayers] ADD WithoutLeagueRegistration BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].[ClubTeams] ADD IsTrainingTeam BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].[TeamRegistrations] ADD TeamName VARCHAR(255) NULL

ALTER TABLE [dbo].[TeamRegistrations] ADD IsDeleted BIT NOT NULL DEFAULT 0

CREATE TABLE [dbo].[UnionOfficialSettings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UnionId] [int] NOT NULL,
	[SeasonId] [int] NULL,
	[JobsRolesId] [int] NOT NULL,
	[PaymentPerGame] [decimal](18, 2) NULL,
	[PaymentPerGameCurrency] [int] NOT NULL,
	[PaymentTravel] [decimal](18, 2) NULL,
	[TravelMetricType] [int] NOT NULL,
	[TravelPaymentCurrencyType] [int] NOT NULL,
	[RateAPerGame] [decimal](18, 2) NULL,
	[RateBPerGame] [decimal](18, 2) NULL,
	[RateAForTravel] [decimal](18, 2) NULL,
	[RateBForTravel] [decimal](18, 2) NULL,

 CONSTRAINT [PK_UnionOfficialSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[UnionOfficialSettings]  WITH CHECK ADD  CONSTRAINT [UnionOfficialSettings_JobsRoles] FOREIGN KEY([JobsRolesId])
REFERENCES [dbo].[JobsRoles] ([RoleId])
GO

ALTER TABLE [dbo].[UnionOfficialSettings] CHECK CONSTRAINT [UnionOfficialSettings_JobsRoles]
GO

ALTER TABLE [dbo].[UnionOfficialSettings]  WITH CHECK ADD  CONSTRAINT [UnionOfficialSettings_Unions] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
GO

ALTER TABLE [dbo].[UnionOfficialSettings] CHECK CONSTRAINT [UnionOfficialSettings_Unions]

ALTER TABLE [dbo].[UnionOfficialSettings]  WITH CHECK ADD  CONSTRAINT [UnionOfficialSettings_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[UnionOfficialSettings] CHECK CONSTRAINT [UnionOfficialSettings_Seasons]


ALTER TABLE [dbo].[UnionOfficialSettings] CHECK CONSTRAINT [UnionOfficialSettings_Seasons]

ALTER TABLE [dbo].[Unions] ADD IsOfficialSettingsChecked BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].[TeamsPlayers] ADD DateOfCreate DATETIME NULL 

ALTER TABLE [dbo].[TeamsDetails] ADD RegistrationId INT NULL 

ALTER TABLE [dbo].[TeamsDetails]  WITH CHECK ADD  CONSTRAINT [TeamsDetails_Registrations] FOREIGN KEY([RegistrationId])
REFERENCES [dbo].[TeamRegistrations] ([Id])
GO