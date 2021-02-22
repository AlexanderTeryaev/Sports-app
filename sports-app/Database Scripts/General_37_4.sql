USE [LogLig]
GO

/****** Object:  Table [dbo].[CompetitionTeamRouteClubs]    Script Date: 12/6/2018 2:24:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CompetitionTeamRouteClubs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompetitionTeamRouteId] [int] NOT NULL,
	[ClubId] [int] NOT NULL,
	[MaximumRegistrationsAllowed] [int] NULL,
 CONSTRAINT [PK_CompetitionTeamRouteClub] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CompetitionTeamRouteClubs]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionTeamRouteClub_Clubs] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO

ALTER TABLE [dbo].[CompetitionTeamRouteClubs] CHECK CONSTRAINT [FK_CompetitionTeamRouteClub_Clubs]
GO

ALTER TABLE [dbo].[CompetitionTeamRouteClubs]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionTeamRouteClub_CompetitionTeamRoutes] FOREIGN KEY([CompetitionTeamRouteId])
REFERENCES [dbo].[CompetitionTeamRoutes] ([Id])
GO

ALTER TABLE [dbo].[CompetitionTeamRouteClubs] CHECK CONSTRAINT [FK_CompetitionTeamRouteClub_CompetitionTeamRoutes]
GO


CREATE TABLE [dbo].[CompetitionRouteClubs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompetitionRouteId] [int] NOT NULL,
	[ClubId] [int] NOT NULL,
	[MaximumRegistrationsAllowed] [int] NULL,
 CONSTRAINT [PK_CompetitionRouteClub] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CompetitionRouteClubs]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionRouteClub_Clubs] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO

ALTER TABLE [dbo].[CompetitionRouteClubs] CHECK CONSTRAINT [FK_CompetitionRouteClub_Clubs]
GO

ALTER TABLE [dbo].[CompetitionRouteClubs]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionRouteClub_CompetitionRoutes] FOREIGN KEY([CompetitionRouteId])
REFERENCES [dbo].[CompetitionRoutes] ([Id])
GO

ALTER TABLE [dbo].[CompetitionRouteClubs] CHECK CONSTRAINT [FK_CompetitionRouteClub_CompetitionRoutes]
GO




