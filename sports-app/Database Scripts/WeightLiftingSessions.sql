USE [LogLig]
GO

/****** Object:  Table [dbo].[WeightLiftingSessions]    Script Date: 12/26/2018 11:42:08 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[WeightLiftingSessions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompetitionId] [int] NOT NULL,
	[SessionNum] [int] NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[WeightStartTime] [datetime] NOT NULL,
	[WeightFinishTime] [datetime] NOT NULL,
 CONSTRAINT [PK_WeightLiftingSessions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[WeightLiftingSessions]  WITH CHECK ADD  CONSTRAINT [FK_WeightLiftingSessions_Leagues] FOREIGN KEY([CompetitionId])
REFERENCES [dbo].[Leagues] ([LeagueId])
GO

ALTER TABLE [dbo].[WeightLiftingSessions] CHECK CONSTRAINT [FK_WeightLiftingSessions_Leagues]
GO





ALTER TABLE [dbo].[CompetitionDisciplineRegistrations] Add WeightliftingSessionId int Null

ALTER TABLE [dbo].[CompetitionDisciplineRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionDisciplineRegistrations_WeightLiftingSessions] FOREIGN KEY([WeightliftingSessionId])
REFERENCES [dbo].[WeightLiftingSessions] ([Id])
GO

ALTER TABLE [dbo].[CompetitionDisciplineRegistrations] CHECK CONSTRAINT [FK_CompetitionDisciplineRegistrations_WeightLiftingSessions]
GO

