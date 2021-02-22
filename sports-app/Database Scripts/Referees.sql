-- Distance table
CREATE TABLE [dbo].[DistanceTable](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UnionId] [int] NULL,
	[ClubId] [int] NULL,
	[CityFromName] [nvarchar](255) NULL,
	[CityToName] [nvarchar](255) NULL,
	[Distance] [int] NULL,
	[DistanceType] [nvarchar](10) NULL,
	CONSTRAINT PK_Distance PRIMARY KEY (Id),
	CONSTRAINT FK_DistanceTablesUnion FOREIGN KEY (UnionId)
    REFERENCES Unions(UnionId),
	CONSTRAINT FK_DistanceTablesClub FOREIGN KEY (ClubId)
    REFERENCES Clubs(ClubId)
)

ALTER TABLE [dbo].[DistanceTable] ADD SeasonId INT NULL
GO
ALTER TABLE [dbo].[DistanceTable]
ADD CONSTRAINT FK_SeasonIdDistanceTable FOREIGN KEY (SeasonId) REFERENCES Seasons(Id);
GO

ALTER TABLE dbo.Clubs ADD
	IsReportsEnabled BIT NOT NULL DEFAULT 0;
ALTER TABLE dbo.Unions ADD
	IsReportsEnabled BIT NOT NULL DEFAULT 0;
GO

ALTER TABLE dbo.Users ADD
	Address nvarchar(500) NULL
GO

ALTER TABLE dbo.UsersJobs ADD
	WithhodlingTax int NULL
GO


IF NOT EXISTS (SELECT [RoleId] FROM [dbo].[JobsRoles] WHERE [RoleId] = 12)
BEGIN
	INSERT INTO [dbo].[JobsRoles] 
	([RoleId], [Title], [RoleName], [Priority])
	VALUES (12, 'Desk', 'desk', 5)
END

ALTER TABLE dbo.GamesCycles ADD
	DeskIds nvarchar(255) NULL
GO

-- league official settings
CREATE TABLE [dbo].[LeagueOfficialsSettings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LeagueId] [int] NOT NULL,
	[JobsRolesId] [int] NOT NULL,
	[PaymentPerGame] [decimal](18, 2) NULL,
	[PaymentPerGameCurrency] [int] NOT NULL,
	[PaymentTravel] [decimal](18, 2) NULL,
	[TravelMetricType] [int] NOT NULL,
	[TravelPaymentCurrencyType] [int] NOT NULL,
 CONSTRAINT [PK_LeagueOfficialsSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LeagueOfficialsSettings]  WITH CHECK ADD  CONSTRAINT [LeagueOfficialsSettings_JobsRoles] FOREIGN KEY([JobsRolesId])
REFERENCES [dbo].[JobsRoles] ([RoleId])
GO

ALTER TABLE [dbo].[LeagueOfficialsSettings] CHECK CONSTRAINT [LeagueOfficialsSettings_JobsRoles]
GO

ALTER TABLE [dbo].[LeagueOfficialsSettings]  WITH CHECK ADD  CONSTRAINT [LeagueOfficialsSettings_Leagues] FOREIGN KEY([LeagueId])
REFERENCES [dbo].[Leagues] ([LeagueId])
GO

ALTER TABLE [dbo].[LeagueOfficialsSettings] CHECK CONSTRAINT [LeagueOfficialsSettings_Leagues]
GO

