CREATE TABLE [dbo].[PlayerAchievements] (
    [Id]            INT      IDENTITY (1, 1) NOT NULL,
    [PlayerId]      INT      NOT NULL,
    [RankId]        INT      NOT NULL,
    [DueDate]       DATETIME NULL,
    [DateCompleted] DATETIME NULL,
    [Score]         INT      NOT NULL,
    CONSTRAINT [PK_PlayerAchievements] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[SportRanks] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [SportId]     INT           NOT NULL,
    [RankName]    NVARCHAR (50) NOT NULL,
    [RankNameHeb] NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_SportRanks] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[PlayerAchievements] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerAchievements_Users] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[PlayerAchievements] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerAchievements_SportRanks] FOREIGN KEY ([RankId]) REFERENCES [dbo].[SportRanks] ([Id]);

ALTER TABLE [dbo].[SportRanks] WITH NOCHECK
    ADD CONSTRAINT [FK_SportRanks_Sports] FOREIGN KEY ([SportId]) REFERENCES [dbo].[Sports] ([Id]);

ALTER TABLE [dbo].[PlayerAchievements] WITH CHECK CHECK CONSTRAINT [FK_PlayerAchievements_Users];

ALTER TABLE [dbo].[PlayerAchievements] WITH CHECK CHECK CONSTRAINT [FK_PlayerAchievements_SportRanks];

ALTER TABLE [dbo].[SportRanks] WITH CHECK CHECK CONSTRAINT [FK_SportRanks_Sports];