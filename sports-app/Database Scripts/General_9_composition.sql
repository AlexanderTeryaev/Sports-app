ALTER TABLE [dbo].[CompetitionRoutes] ADD Composition INT NULL

CREATE TABLE [dbo].[AdditionalGymnastics] (
    [Id]       INT IDENTITY (1, 1) NOT NULL,
    [UserId]   INT NOT NULL,
    [SeasonId] INT NOT NULL,
    [LeagueId] INT NOT NULL,
    [ClubId]   INT NOT NULL,
    [CompetitionRouteId]      INT NOT NULL,
    CONSTRAINT [PK_AdditionalGymnastic] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[AdditionalGymnastics] WITH NOCHECK
    ADD CONSTRAINT [FK_League_AdditionalGymanstic] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[AdditionalGymnastics] WITH NOCHECK
    ADD CONSTRAINT [FK_Season_AdditionalGymnastic] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[AdditionalGymnastics] WITH NOCHECK
    ADD CONSTRAINT [FK_RoutesComp_AdditionalGymnastic] FOREIGN KEY ([CompetitionRouteId]) REFERENCES [dbo].[CompetitionRoutes] ([Id]);

ALTER TABLE [dbo].[AdditionalGymnastics] WITH NOCHECK
    ADD CONSTRAINT [FK_Users_AdditionalGymnastic] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[AdditionalGymnastics] WITH NOCHECK
    ADD CONSTRAINT [FK_Clubs_AdditionalGymnastic] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);