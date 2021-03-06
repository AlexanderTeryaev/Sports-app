IF not EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Hierarchy'
          AND Object_ID = Object_ID(N'[dbo].[DisciplineRoutes]'))
BEGIN
    alter table [dbo].[DisciplineRoutes] add Hierarchy int null
END

IF not EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'FromAge'
          AND Object_ID = Object_ID(N'[dbo].[RouteRanks]'))
BEGIN
    alter table [dbo].[RouteRanks] add FromAge int null
END

IF not EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'ToAge'
          AND Object_ID = Object_ID(N'[dbo].[RouteRanks]'))
BEGIN
    alter table [dbo].[RouteRanks] add ToAge int null
END


IF OBJECT_ID('dbo.[TeamsRanks]', 'U') IS NOT NULL 
	DROP TABLE dbo.[TeamsRanks]; 

IF OBJECT_ID('dbo.[RouteTeamRanks]', 'U') IS NOT NULL 
	DROP TABLE dbo.[RouteTeamRanks]; 

IF OBJECT_ID('dbo.[TeamsRoutes]', 'U') IS NOT NULL 
	DROP TABLE dbo.[TeamsRoutes]; 

IF OBJECT_ID('dbo.[DisciplineTeamRoutes]', 'U') IS NOT NULL 
	DROP TABLE dbo.[DisciplineTeamRoutes]; 



CREATE TABLE [dbo].[DisciplineTeamRoutes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DisciplineId] [int] NOT NULL,
	[Route] [nvarchar](255) NOT NULL,
	[Hierarchy] [int] NULL,
	CONSTRAINT [PK_DisciplineTeamRoutes] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
) 
GO


CREATE TABLE [dbo].[RouteTeamRanks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TeamRouteId] [int] NOT NULL,
	[Rank] [nvarchar](50) NOT NULL,
	[FromAge] [int] NULL,
	[ToAge] [int] NULL,
	CONSTRAINT [PK_RouteTeamRanks] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
GO

CREATE TABLE [dbo].[TeamsRanks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TeamId] [int] NOT NULL,
	[RankId] [int] NOT NULL,
	[TeamsRouteId] [int] NOT NULL,
	CONSTRAINT [PK_TeamsRanks] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
) 
GO

CREATE TABLE [dbo].[TeamsRoutes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TeamId] [int] NOT NULL,
	[RouteId] [int] NOT NULL,
	CONSTRAINT [PK_TeamsRoutes] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
) 
GO

ALTER TABLE [dbo].[DisciplineTeamRoutes]  WITH CHECK ADD  CONSTRAINT [FK_DisciplineTeamRoutes_Disciplines] FOREIGN KEY([DisciplineId])
REFERENCES [dbo].[Disciplines] ([DisciplineId])
GO

ALTER TABLE [dbo].[DisciplineTeamRoutes] CHECK CONSTRAINT [FK_DisciplineTeamRoutes_Disciplines]
GO

ALTER TABLE [dbo].[RouteTeamRanks]  WITH CHECK ADD  CONSTRAINT [FK_RouteTeamRanks_DisciplineTeamRoutes] FOREIGN KEY([TeamRouteId])
REFERENCES [dbo].[DisciplineTeamRoutes] ([Id])
GO

ALTER TABLE [dbo].[RouteTeamRanks] CHECK CONSTRAINT [FK_RouteTeamRanks_DisciplineTeamRoutes]
GO

ALTER TABLE [dbo].[TeamsRanks]  WITH CHECK ADD FOREIGN KEY([TeamsRouteId])
REFERENCES [dbo].[TeamsRoutes] ([Id])
GO

ALTER TABLE [dbo].[TeamsRanks]  WITH CHECK ADD  CONSTRAINT [FK_TeamsRanks_RouteRanks] FOREIGN KEY([RankId])
REFERENCES [dbo].[RouteTeamRanks] ([Id])
GO

ALTER TABLE [dbo].[TeamsRanks] CHECK CONSTRAINT [FK_TeamsRanks_RouteRanks]
GO

ALTER TABLE [dbo].[TeamsRanks]  WITH CHECK ADD  CONSTRAINT [FK_TeamsRanks_Teams] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] ([TeamId])
GO

ALTER TABLE [dbo].[TeamsRanks] CHECK CONSTRAINT [FK_TeamsRanks_Teams]
GO

ALTER TABLE [dbo].[TeamsRoutes]  WITH CHECK ADD  CONSTRAINT [FK_TeamsRoutes_DisciplineTeamRoutes] FOREIGN KEY([RouteId])
REFERENCES [dbo].[DisciplineTeamRoutes] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[TeamsRoutes] CHECK CONSTRAINT [FK_TeamsRoutes_DisciplineTeamRoutes]
GO

ALTER TABLE [dbo].[TeamsRoutes]  WITH CHECK ADD  CONSTRAINT [FK_TeamsRoutes_Teams] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] ([TeamId])
GO

ALTER TABLE [dbo].[TeamsRoutes] CHECK CONSTRAINT [FK_TeamsRoutes_Teams]
GO
