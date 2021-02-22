ALTER TABLE [dbo].[NotesMessages] ADD SentForClubManagers BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].CompetitionRegistrations ADD InstrumentId INT NULL

ALTER TABLE [dbo].[CompetitionRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionRegistrations_Instrument] FOREIGN KEY([InstrumentId])
REFERENCES [dbo].[Instruments] ([Id])
GO

ALTER TABLE [dbo].[Leagues] ADD [Type] INT NULL


CREATE TABLE [dbo].[SportsRegistrations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[SeasonId] [int] NOT NULL,
	[LeagueId] [int] NOT NULL,
	[ClubId] [int] NOT NULL,
	[FinalScore] [float] NULL,
	[Position] [int] NULL


 CONSTRAINT [PK_SportsRegistrations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SportsRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_SportsRegistrations_Leagues] FOREIGN KEY([LeagueId])
REFERENCES [dbo].[Leagues] ([LeagueId])
GO

ALTER TABLE [dbo].[SportsRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_SportsRegistrations_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[SportsRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_SportsRegistrations_Clubs] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO

ALTER TABLE [dbo].[SportsRegistrations]  WITH CHECK ADD  CONSTRAINT [FK_SportsRegistrations_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[SportsRegistrations] ADD IsApproved BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].[GameSets] ADD IsPenalties BIT NOT NULL DEFAULT 0