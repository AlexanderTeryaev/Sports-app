ALTER TABLE TeamsPlayers ADD ActionUserId INT NULL
ALTER TABLE PlayersBlockade ADD ActionUserId INT NULL
ALTER TABLE PenaltyForExclusion ADD ActionUserId INT NULL
ALTER TABLE UsersApprovalDatesHistory ADD ActionUserId INT NULL
ALTER TABLE PlayerHistory ADD ActionUserId INT NULL

ALTER TABLE [dbo].[UsersApprovalDatesHistory] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersApprovalDatesHistory_ActionUserId] FOREIGN KEY (ActionUserId) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[TeamsPlayers] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamsPlayers_ActionUserId] FOREIGN KEY (ActionUserId) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[PlayersBlockade] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayersBlockade_ActionUserId] FOREIGN KEY (ActionUserId) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[PenaltyForExclusion] WITH NOCHECK
    ADD CONSTRAINT [FK_PenaltyForExclusion_ActionUserId] FOREIGN KEY (ActionUserId) REFERENCES [dbo].[Users] ([UserId]);


ALTER TABLE [dbo].[PlayerHistory] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerHistory_ActionUserId] FOREIGN KEY (ActionUserId) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[CompetitionRegistrations] ADD IsRegisteredByExcel BIT NOT NULL DEFAULT 0
ALTER TABLE [dbo].[CompetitionRegistrations] ADD IsActive BIT NOT NULL DEFAULT 1
ALTER TABLE [dbo].[CompetitionRegistrations] ALTER COLUMN CompetitionRouteId INT NULL


CREATE TABLE [dbo].[LevelDateSettings]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompetitionId] [int] NOT NULL,
	[CompetitionLevelId] int NOT NULL,
	[QualificationStartDate] DATETIME  NULL,
	[QualificationEndDate] DATETIME  NULL,
	[FinalStartDate] DATETIME  NULL,
	[FinalEndDate] DATETIME  NULL

CONSTRAINT [PK_LevelDateSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LevelDateSettings] WITH NOCHECK
    ADD CONSTRAINT [FK_LevelDateSettings_CompetitionId] FOREIGN KEY (CompetitionId) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[LevelDateSettings] WITH NOCHECK
    ADD CONSTRAINT [FK_LevelDateSettings_CompetitionLevelId] FOREIGN KEY (CompetitionLevelId) REFERENCES [dbo].[CompetitionLevel] ([Id]);

