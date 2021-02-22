ALTER TABLE [dbo].[TennisPointsSettings] ADD [PointsForPairs] INT NULL
ALTER TABLE [dbo].[Clubs] ADD [Comment] VARCHAR(MAX) NULL
ALTER TABLE [dbo].[Clubs] ADD [IsClubApproved] BIT NULL 

CREATE TABLE [dbo].[ClubPayments]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClubId] [int] NOT NULL,
	[Paid] [decimal] NOT NULL,
	[DateOfPayment] [datetime] NOT NULL
CONSTRAINT [PK_ClubPayments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ClubPayments] WITH NOCHECK
    ADD CONSTRAINT [FK_ClubPayments_ClubId] FOREIGN KEY (ClubId) REFERENCES [dbo].[Clubs] ([ClubId]);


CREATE TABLE [dbo].[NotesAttachedFiles]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FilePath] varchar(255) NOT NULL,
	[NoteMessageId] [int] NOT NULL
CONSTRAINT [PK_NotesAttachedFiles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[NotesAttachedFiles] WITH NOCHECK
    ADD CONSTRAINT [FK_NotesAttachedFiles_NoteMessageId] FOREIGN KEY (NoteMessageId) REFERENCES [dbo].[NotesMessages] ([MsgId]);

ALTER TABLE TennisGroups ADD IsPairs BIT NOT NULL DEFAULT 0

ALTER TABLE TennisGroupTeams ADD PairPlayerId INT NULL

ALTER TABLE [dbo].[TennisGroupTeams] WITH NOCHECK
    ADD CONSTRAINT [FK_TennisGroupTeams_PairPlayerId] FOREIGN KEY (PairPlayerId) REFERENCES [dbo].[TeamsPlayers] ([Id]);

ALTER TABLE TennisGameCycles ADD FirstPlayerPairId INT NULL
ALTER TABLE [dbo].[TennisGameCycles] WITH NOCHECK
    ADD CONSTRAINT [FK_TennisGameCycles_FirstPlayerPairId] FOREIGN KEY (FirstPlayerPairId) REFERENCES [dbo].[TeamsPlayers] ([Id]);

ALTER TABLE TennisGameCycles ADD SecondPlayerPairId INT NULL
ALTER TABLE [dbo].[TennisGameCycles] WITH NOCHECK
    ADD CONSTRAINT [FK_TennisGameCycles_SecondPlayerPairId] FOREIGN KEY (SecondPlayerPairId) REFERENCES [dbo].[TeamsPlayers] ([Id]);

ALTER TABLE TennisPlayoffBrackets ADD FirstPlayerPairId INT NULL
ALTER TABLE TennisPlayoffBrackets ADD SecondPlayerPairId INT NULL

ALTER TABLE TennisPlayoffBrackets ADD WinnerPlayerPairId INT NULL
ALTER TABLE TennisPlayoffBrackets ADD LoserPlayerPairId INT NULL

ALTER TABLE TennisRank ADD AgeId INT NULL
ALTER TABLE [dbo].[TennisRank] WITH NOCHECK
    ADD CONSTRAINT [FK_TennisRank_AgeId] FOREIGN KEY (AgeId) REFERENCES [dbo].[CompetitionAges] ([id]);

ALTER TABLE [dbo].[GamesCycles] ADD [RoundNum] INT NULL

ALTER TABLE [dbo].[TennisGameCycles] ADD [RoundNum] INT NULL
ALTER TABLE [dbo].[TennisGames] ADD [RoundStartCycle] INT NULL

ALTER TABLE [dbo].[Games] ADD [RoundStartCycle] INT NULL
ALTER TABLE TennisRank ADD IsUpdated BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].[Games] ADD [ShowCyclesOnExternal] INT NULL
ALTER TABLE [dbo].[TennisGames] ADD [ShowCyclesOnExternal] INT NULL




