ALTER TABLE Games ADD BestOfSets INT NULL
ALTER TABLE Games ADD NumberOfGames INT NULL
ALTER TABLE Games ADD PairsAsLastGame BIT NOT NULL DEFAULT 0

CREATE TABLE [dbo].[TennisLeagueGames]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CycleId] [int] NOT NULL,
	[GameNumber] [int] NOT NULL,
	[HomePlayerId] [int] NOT NULL,
	[GuestPlayerId] [int] NOT NULL,
	[TechnicalWinnerId] [int] NULL
CONSTRAINT [PK_TennisLeagueGames] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TennisLeagueGames]  WITH CHECK ADD  CONSTRAINT [FK_TennisLeagueGame_GameCycle] FOREIGN KEY([CycleId])
REFERENCES [dbo].[GamesCycles] ([CycleId])
GO

ALTER TABLE [dbo].[TennisLeagueGames]  WITH CHECK ADD  CONSTRAINT [FK_TennisLeagueGame_HomePLayer] FOREIGN KEY([HomePlayerId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[TennisLeagueGames]  WITH CHECK ADD  CONSTRAINT [FK_TennisLeagueGame_GuestPlayer] FOREIGN KEY([GuestPlayerId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[TennisLeagueGames]  WITH CHECK ADD  CONSTRAINT [FK_TennisLeagueGame_TechnicalWinner] FOREIGN KEY([TechnicalWinnerId])
REFERENCES [dbo].[Users] ([UserId])
GO

CREATE TABLE [dbo].[TennisLeagueGameScores]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GameId] [int] NOT NULL,
	[HomeScore] [int] NOT NULL DEFAULT 0,
	[GuestScore] [int] NOT NULL DEFAULT 0,

CONSTRAINT [PK_TennisLeagueGameScores] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TennisLeagueGameScores]  WITH CHECK ADD  CONSTRAINT [FK_TennisLeagueGameScores_Game] FOREIGN KEY([GameId])
REFERENCES [dbo].[TennisLeagueGames] ([Id])
GO

ALTER TABLE [dbo].[TennisLeagueGames] ADD IsEnded BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].[TennisLeagueGames] ADD HomePairPlayerId INT NULL
ALTER TABLE [dbo].[TennisLeagueGames] ADD GuestPairPlayerId INT NULL

ALTER TABLE [dbo].[TennisLeagueGames]  WITH CHECK ADD  CONSTRAINT [FK_TennisLeagueGame_HomePairPlayer] FOREIGN KEY([HomePairPlayerId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[TennisLeagueGames]  WITH CHECK ADD  CONSTRAINT [FK_TennisLeagueGame_GuestPairPlayer] FOREIGN KEY([GuestPairPlayerId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[TennisLeagueGameScores] ADD IsPairScores BIT NOT NULL DEFAULT 0

