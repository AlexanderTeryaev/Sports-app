USE [LogLig]
GO

/****** Object:  Table [dbo].[AdditionalGymnastics]    Script Date: 11/29/2018 5:29:42 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AdditionalTeamGymnastics](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[SeasonId] [int] NOT NULL,
	[LeagueId] [int] NOT NULL,
	[ClubId] [int] NOT NULL,
	[CompetitionRouteId] [int] NOT NULL,
	[IsRegisteredInSecondComposition] [bit] NOT NULL,
 CONSTRAINT [PK_AdditionalTeamGymnastic] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[AdditionalTeamGymnastics] ADD  DEFAULT ((0)) FOR [IsRegisteredInSecondComposition]
GO

ALTER TABLE [dbo].[AdditionalTeamGymnastics]  WITH NOCHECK ADD  CONSTRAINT [FK_Clubs_AdditionalTeamGymnastic] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO

ALTER TABLE [dbo].[AdditionalTeamGymnastics] CHECK CONSTRAINT [FK_Clubs_AdditionalTeamGymnastic]
GO

ALTER TABLE [dbo].[AdditionalTeamGymnastics]  WITH NOCHECK ADD  CONSTRAINT [FK_League_AdditionalTeamGymanstic] FOREIGN KEY([LeagueId])
REFERENCES [dbo].[Leagues] ([LeagueId])
GO

ALTER TABLE [dbo].[AdditionalTeamGymnastics] CHECK CONSTRAINT [FK_League_AdditionalTeamGymanstic]
GO

ALTER TABLE [dbo].[AdditionalTeamGymnastics]  WITH NOCHECK ADD  CONSTRAINT [FK_RoutesComp_AdditionalTeamGymnastic] FOREIGN KEY([CompetitionRouteId])
REFERENCES [dbo].[CompetitionTeamRoutes] ([Id])
GO

ALTER TABLE [dbo].[AdditionalTeamGymnastics] CHECK CONSTRAINT [FK_RoutesComp_AdditionalTeamGymnastic]
GO

ALTER TABLE [dbo].[AdditionalTeamGymnastics]  WITH NOCHECK ADD  CONSTRAINT [FK_Season_AdditionalTeamGymnastic] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[AdditionalTeamGymnastics] CHECK CONSTRAINT [FK_Season_AdditionalTeamGymnastic]
GO

ALTER TABLE [dbo].[AdditionalTeamGymnastics]  WITH NOCHECK ADD  CONSTRAINT [FK_Users_AdditionalTeamGymnastic] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[AdditionalTeamGymnastics] CHECK CONSTRAINT [FK_Users_AdditionalTeamGymnastic]
GO


