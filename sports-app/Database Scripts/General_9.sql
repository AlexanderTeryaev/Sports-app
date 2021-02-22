GO
  IF NOT EXISTS (SELECT [AgeId] FROM [dbo].[Ages] WHERE [AgeId] = 13)
	BEGIN
		INSERT INTO [dbo].[Ages] ([Title]) VALUES ('מעורב')
	END
GO
  IF NOT EXISTS (SELECT [GenderId] FROM [dbo].[Genders] WHERE [GenderId] = 3)
	BEGIN
		INSERT INTO [dbo].[Genders] ([GenderId],[Title],[TitleMany]) VALUES (3,'All','All')
	END
GO

ALTER TABLE [dbo].[Leagues] ADD LeagueStartDate DATETIME NULL
ALTER TABLE [dbo].[Leagues] ADD LeagueEndDate DATETIME NULL
ALTER TABLE [dbo].[Leagues] ADD EndRegistrationDate DATETIME NULL

CREATE TABLE [dbo].[CompetitionRoutes] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [DisciplineId] INT NOT NULL,
    [SeasonId]     INT NOT NULL,
    [RouteId]      INT NOT NULL,
    [RankId]       INT NOT NULL
    CONSTRAINT [PK_CompetitionRoute] PRIMARY KEY CLUSTERED ([Id] ASC)
);
select * from RouteRanks
ALTER TABLE [dbo].[CompetitionRoutes] WITH NOCHECK
    ADD CONSTRAINT [FK_Discipline_Competition] FOREIGN KEY ([DisciplineId]) REFERENCES [dbo].[Disciplines] ([DisciplineId]);

ALTER TABLE [dbo].[CompetitionRoutes] WITH NOCHECK
    ADD CONSTRAINT [FK_Season_Competition] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[CompetitionRoutes] WITH NOCHECK
    ADD CONSTRAINT [FK_Route_Competition] FOREIGN KEY ([RouteId]) REFERENCES [dbo].[DisciplineRoutes] ([Id]);

ALTER TABLE [dbo].[CompetitionRoutes] WITH NOCHECK
    ADD CONSTRAINT [FK_Rank_Competition] FOREIGN KEY ([RankId]) REFERENCES [dbo].[RouteRanks] ([Id]);

ALTER TABLE [dbo].[CompetitionRoutes] ADD LeagueId INT NOT NULL
ALTER TABLE [dbo].[CompetitionRoutes] WITH NOCHECK
    ADD CONSTRAINT [FK_League_Competition] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);


CREATE TABLE [dbo].[CompetitionRegistrations] (
    [Id]       INT IDENTITY (1, 1) NOT NULL,
    [UserId]   INT NOT NULL,
    [SeasonId] INT NOT NULL,
    [LeagueId] INT NOT NULL,
    [ClubId]   INT NOT NULL,
    [CompetitionRouteId]      INT NOT NULL,
    CONSTRAINT [PK_CompetitionRegistration] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[CompetitionRegistrations] WITH NOCHECK
    ADD CONSTRAINT [FK_League_CompetitionRegistration] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[CompetitionRegistrations] WITH NOCHECK
    ADD CONSTRAINT [FK_Season_CompetitionRegistration] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[CompetitionRegistrations] WITH NOCHECK
    ADD CONSTRAINT [FK_RoutesComp_CompetitionRegistration] FOREIGN KEY ([CompetitionRouteId]) REFERENCES [dbo].[CompetitionRoutes] ([Id]);

ALTER TABLE [dbo].[CompetitionRegistrations] WITH NOCHECK
    ADD CONSTRAINT [FK_Users_CompetitionRegistration] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[CompetitionRegistrations] WITH NOCHECK
    ADD CONSTRAINT [FK_Clubs_CompetitionRegistration] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);