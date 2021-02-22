CREATE TABLE [dbo].[LeagueScheduleState]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LeagueId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[ElementDivId] varchar(255) NOT NULL,
	[IsHidden] [BIT] NOT NULL DEFAULT 0,
CONSTRAINT [PK_LeagueScheduleState] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LeagueScheduleState] WITH NOCHECK
    ADD CONSTRAINT [FK_LeagueScheduleState_LeagueId] FOREIGN KEY (LeagueId) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[LeagueScheduleState] WITH NOCHECK
    ADD CONSTRAINT [FK_LeagueScheduleState_UserId] FOREIGN KEY (UserId) REFERENCES [dbo].[Users] ([UserId]);