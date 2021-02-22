ALTER TABLE [dbo].[Leagues] Add IsCompetitionLeague bit not null default(0)
ALTER TABLE [dbo].[CompetitionDisciplines] Add IsMultiBattle bit not null default(0)
ALTER TABLE [dbo].[CompetitionDisciplines] Add IsForScore bit not null default(0)

ALTER TABLE [dbo].[CompetitionResults] Add ClubPoints decimal(10,2) null


USE [LogLig]
GO

/****** Object:  Table [dbo].[CompetitionClubsCorrections]    Script Date: 1/30/2019 3:20:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CompetitionClubsCorrections](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LeagueId] [int] NULL,
	[SeasonId] [int] NULL,
	[ClubId] [int] NULL,
	[Correction] [decimal](18, 2) NULL,
	[GenderId] [int] NULL,
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CompetitionClubsCorrections]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionClubsCorrections_Clubs] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO

ALTER TABLE [dbo].[CompetitionClubsCorrections] CHECK CONSTRAINT [FK_CompetitionClubsCorrections_Clubs]
GO

ALTER TABLE [dbo].[CompetitionClubsCorrections]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionClubsCorrections_Leagues] FOREIGN KEY([LeagueId])
REFERENCES [dbo].[Leagues] ([LeagueId])
GO

ALTER TABLE [dbo].[CompetitionClubsCorrections] CHECK CONSTRAINT [FK_CompetitionClubsCorrections_Leagues]
GO

ALTER TABLE [dbo].[CompetitionClubsCorrections]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionClubsCorrections_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[CompetitionClubsCorrections] CHECK CONSTRAINT [FK_CompetitionClubsCorrections_Seasons]
GO



