ALTER TABLE [dbo].[GamesCycles] ADD IsPublished BIT NOT NULL DEFAULT 0
GO 
USE [LogLig]

-- Script for LLCMS-108 "Messaging" -----------------

GO

CREATE TABLE [dbo].[SentMessages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MessageId] [int] NOT NULL,
	[TeamId] [int] NULL,
	[LeagueId] [int] NULL,
	[UnionId] [int] NULL,
	[ClubId] [int] NULL,
	[SeasonId] [int] NOT NULL,
 CONSTRAINT [PK_SentMessages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SentMessages]  WITH CHECK ADD  CONSTRAINT [FK_SentMessages_Clubs] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO

ALTER TABLE [dbo].[SentMessages] CHECK CONSTRAINT [FK_SentMessages_Clubs]
GO

ALTER TABLE [dbo].[SentMessages]  WITH CHECK ADD  CONSTRAINT [FK_SentMessages_Leagues] FOREIGN KEY([LeagueId])
REFERENCES [dbo].[Leagues] ([LeagueId])
GO

ALTER TABLE [dbo].[SentMessages] CHECK CONSTRAINT [FK_SentMessages_Leagues]
GO

ALTER TABLE [dbo].[SentMessages]  WITH CHECK ADD  CONSTRAINT [FK_SentMessages_NotesMessages] FOREIGN KEY([MessageId])
REFERENCES [dbo].[NotesMessages] ([MsgId])
GO

ALTER TABLE [dbo].[SentMessages] CHECK CONSTRAINT [FK_SentMessages_NotesMessages]
GO

ALTER TABLE [dbo].[SentMessages]  WITH CHECK ADD  CONSTRAINT [FK_SentMessages_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[SentMessages] CHECK CONSTRAINT [FK_SentMessages_Seasons]
GO

ALTER TABLE [dbo].[SentMessages]  WITH CHECK ADD  CONSTRAINT [FK_SentMessages_Teams] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] ([TeamId])
GO

ALTER TABLE [dbo].[SentMessages] CHECK CONSTRAINT [FK_SentMessages_Teams]
GO

ALTER TABLE [dbo].[SentMessages]  WITH CHECK ADD  CONSTRAINT [FK_SentMessages_Unions] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
GO

ALTER TABLE [dbo].[SentMessages] CHECK CONSTRAINT [FK_SentMessages_Unions]
GO

ALTER TABLE Seasons ADD ClubId INT NULL

GO

ALTER TABLE Seasons
ADD CONSTRAINT FK_Seasons_ClubId FOREIGN KEY (ClubId) REFERENCES Clubs(ClubId);

GO

--DECLARE @unionId INT = (SELECT TOP 1 UnionId FROM Seasons ORDER BY Id ASC)
--DECLARE @seasonId INT = (SELECT TOP 1 Id FROM Seasons ORDER BY Id ASC)

--INSERT INTO [dbo].[SentMessages]
--           ([MessageId]
--		   ,[UnionId]
--		   ,[SeasonId])
--(SELECT [MsgId], @unionId as UnionId, @seasonId as SeasonId FROM TeamsMessages )

--GO

----------------------------------------------------

ALTER TABLE Seasons
ALTER COLUMN UnionId INT NULL

GO

ALTER TABLE [dbo].[Leagues] ADD MaximumHandicapScoreValue INT NOT NULL DEFAULT 0

GO

INSERT INTO [dbo].[Seasons] (Name,Description,StartDate,EndDate,UnionId,ClubId) SELECT '2016-2017', 'Initial season', CAST(N'2016-09-01 00:00:00' AS DateTime), CAST(N'2017-06-30 00:00:00' AS DateTime),NULL,ClubId FROM [dbo].Clubs;

GO

ALTER TABLE [dbo].[Users] ADD TestResults INT NOT NULL DEFAULT 0

GO

ALTER TABLE [dbo].[Unions] ADD IsHadicapEnabled BIT NOT NULL DEFAULT 0

GO

ALTER TABLE [dbo].[TeamsPlayers] ADD IsPlayereInTeamLessThan3year BIT NOT NULL DEFAULT 0

GO

ALTER TABLE [dbo].[TeamsPlayers] ADD HandicapLevel INT NOT NULL DEFAULT 1

GO

ALTER TABLE UsersJobs ADD SeasonId INT NULL

GO

ALTER TABLE UsersJobs ADD CONSTRAINT FK_Seasons_UserJobs FOREIGN KEY (SeasonId) REFERENCES Seasons(Id);

GO