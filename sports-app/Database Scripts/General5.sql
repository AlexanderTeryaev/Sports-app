CREATE TABLE [dbo].[DisciplineRoutes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DisciplineId] [int] NOT NULL,
	[Route] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_DisciplineRoutes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DisciplineRoutes]  WITH CHECK ADD  CONSTRAINT [FK_DisciplineRoutes_Disciplines] FOREIGN KEY([DisciplineId])
REFERENCES [dbo].[Disciplines] ([DisciplineId])
GO

ALTER TABLE [dbo].[DisciplineRoutes] CHECK CONSTRAINT [FK_DisciplineRoutes_Disciplines]
GO

CREATE TABLE [dbo].[RouteRanks](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [RouteId] [int] NOT NULL,
    [Rank] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_RouteRanks] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RouteRanks]  WITH CHECK ADD  CONSTRAINT [FK_RouteRanks_DisciplineRoutes] FOREIGN KEY([RouteId])
REFERENCES [dbo].[DisciplineRoutes] ([Id])
GO

ALTER TABLE [dbo].[RouteRanks] CHECK CONSTRAINT [FK_RouteRanks_DisciplineRoutes]
GO


Alter table Unions Add StatementOfClub nvarchar(255)
GO
Alter table Clubs Add StatementApproved bit not null default 0
GO
Alter table TeamsPlayers Add MedExamDate [datetime] NULL
GO


CREATE TABLE [dbo].[UsersRoutes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[RouteId] [int] NOT NULL,
 CONSTRAINT [PK_UsersRoutes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[UsersRoutes]  WITH CHECK ADD  CONSTRAINT [FK_UsersRoutes_DisciplineRoutes] FOREIGN KEY([RouteId])
REFERENCES [dbo].[DisciplineRoutes] ([Id])
GO

ALTER TABLE [dbo].[UsersRoutes] CHECK CONSTRAINT [FK_UsersRoutes_DisciplineRoutes]
GO

ALTER TABLE [dbo].[UsersRoutes]  WITH CHECK ADD  CONSTRAINT [FK_UsersRoutes_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[UsersRoutes] CHECK CONSTRAINT [FK_UsersRoutes_Users]
GO


CREATE TABLE [dbo].[UsersRanks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[RankId] [int] NOT NULL,
 CONSTRAINT [PK_UsersRanks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[UsersRanks]  WITH CHECK ADD  CONSTRAINT [FK_UsersRanks_RouteRanks] FOREIGN KEY([RankId])
REFERENCES [dbo].[RouteRanks] ([Id])
GO

ALTER TABLE [dbo].[UsersRanks] CHECK CONSTRAINT [FK_UsersRanks_RouteRanks]
GO

ALTER TABLE [dbo].[UsersRanks]  WITH CHECK ADD  CONSTRAINT [FK_UsersRanks_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[UsersRanks] CHECK CONSTRAINT [FK_UsersRanks_Users]
GO

ALTER TABLE [dbo].[UsersRanks] ADD UsersRouteId INT NOT NULL,
	FOREIGN KEY (UsersRouteId) REFERENCES UsersRoutes(Id);

