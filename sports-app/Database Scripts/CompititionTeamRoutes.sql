USE [LogLig]
GO

/****** Object:  Table [dbo].[CompetitionTeamRoutes]    Script Date: 11/13/2018 6:45:38 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CompetitionTeamRoutes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DisciplineId] [int] NOT NULL,
	[SeasonId] [int] NOT NULL,
	[RouteId] [int] NOT NULL,
	[RankId] [int] NOT NULL,
	[LeagueId] [int] NOT NULL,
	[Composition] [int] NULL,
	[SecondComposition] [int] NULL,
	[InstrumentIds] [varchar](max) NULL,
	[IsCompetitiveEnabled] [bit] NOT NULL,
 CONSTRAINT [PK_CompetitionTeamRoute] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes] ADD  DEFAULT ((0)) FOR [IsCompetitiveEnabled]
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes]  WITH NOCHECK ADD  CONSTRAINT [FK_Discipline_CompetitionTeam] FOREIGN KEY([DisciplineId])
REFERENCES [dbo].[Disciplines] ([DisciplineId])
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes] CHECK CONSTRAINT [FK_Discipline_CompetitionTeam]
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes]  WITH NOCHECK ADD  CONSTRAINT [FK_League_CompetitionTeam] FOREIGN KEY([LeagueId])
REFERENCES [dbo].[Leagues] ([LeagueId])
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes] CHECK CONSTRAINT [FK_League_CompetitionTeam]
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes]  WITH NOCHECK ADD  CONSTRAINT [FK_Rank_CompetitionTeam] FOREIGN KEY([RankId])
REFERENCES [dbo].[RouteRanks] ([Id])
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes] CHECK CONSTRAINT [FK_Rank_CompetitionTeam]
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes]  WITH NOCHECK ADD  CONSTRAINT [FK_Route_CompetitionTeam] FOREIGN KEY([RouteId])
REFERENCES [dbo].[DisciplineRoutes] ([Id])
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes] CHECK CONSTRAINT [FK_Route_CompetitionTeam]
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes]  WITH NOCHECK ADD  CONSTRAINT [FK_Season_CompetitionTeam] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[CompetitionTeamRoutes] CHECK CONSTRAINT [FK_Season_CompetitionTeam]
GO

