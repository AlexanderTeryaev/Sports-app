ALTER TABLE [dbo].[Unions] ADD ReportSettings VARCHAR(255) NULL
ALTER TABLE [dbo].[Clubs] ADD ReportSettings VARCHAR(255) NULL

CREATE TABLE [dbo].[PenaltyForExclusion]
(
	Id INT IDENTITY(1,1) NOT NULL,
	PlayerId INT NOT NULL,
	DateOfExclusion DATETIME NOT NULL,
	ExclusionNumber INT NOT NULL,
	IsEnded BIT NOT NULL DEFAULT 0,
	IsCanceled BIT NOT NULL DEFAULT 0
 CONSTRAINT [PK_PenaltyForExclusion] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[PenaltyForExclusion]  WITH CHECK ADD  CONSTRAINT [FK_PenaltyForExclusion_PlayerId] FOREIGN KEY(PlayerId)
REFERENCES [dbo].[TeamsPlayers] ([Id])
GO

ALTER TABLE [dbo].[UsersJobs] ADD IsBlocked BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].[PenaltyForExclusion] DROP CONSTRAINT [FK_PenaltyForExclusion_PlayerId]

ALTER TABLE [dbo].[PenaltyForExclusion] DROP COLUMN PlayerId

DELETE from [dbo].[PenaltyForExclusion]

ALTER TABLE [dbo].[PenaltyForExclusion]
    ADD UserId INT NOT NULL,
    FOREIGN KEY(UserId) REFERENCES Users(UserId);