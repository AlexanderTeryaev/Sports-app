ALTER TABLE [dbo].[Clubs] DROP COLUMN [DisciplineIds];
ALTER TABLE [dbo].[Users] DROP COLUMN [DisciplineIds];

CREATE TABLE [dbo].[ClubDisciplines] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [ClubId]       INT NOT NULL,
    [DisciplineId] INT NOT NULL,
    [SeasonId]     INT NOT NULL,
    CONSTRAINT [PK_ClubDisciplines] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[PlayerDisciplines] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [PlayerId]     INT NOT NULL,
    [DisciplineId] INT NOT NULL,
    [ClubId]       INT NOT NULL,
    [SeasonId]     INT NOT NULL,
    CONSTRAINT [PK_PlayerDisciplines] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[TeamDisciplines] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [TeamId]       INT NOT NULL,
    [ClubId]       INT NOT NULL,
    [SeasonId]     INT NOT NULL,
    [DisciplineId] INT NOT NULL,
    CONSTRAINT [PK_TeamDisciplines] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[ClubDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_ClubDisciplines_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);

ALTER TABLE [dbo].[ClubDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_ClubDisciplines_Disciplines] FOREIGN KEY ([DisciplineId]) REFERENCES [dbo].[Disciplines] ([DisciplineId]);

ALTER TABLE [dbo].[ClubDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_ClubDisciplines_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[PlayerDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDisciplines_Users] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[PlayerDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDisciplines_Disciplines] FOREIGN KEY ([DisciplineId]) REFERENCES [dbo].[Disciplines] ([DisciplineId]);

ALTER TABLE [dbo].[PlayerDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDisciplines_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);

ALTER TABLE [dbo].[PlayerDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDisciplines_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[TeamDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamDisciplines_Teams] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]);

ALTER TABLE [dbo].[TeamDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamDisciplines_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);

ALTER TABLE [dbo].[TeamDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamDisciplines_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[TeamDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamDisciplines_Disciplines] FOREIGN KEY ([DisciplineId]) REFERENCES [dbo].[Disciplines] ([DisciplineId]);

ALTER TABLE [dbo].[ClubDisciplines] WITH CHECK CHECK CONSTRAINT [FK_ClubDisciplines_Clubs];

ALTER TABLE [dbo].[ClubDisciplines] WITH CHECK CHECK CONSTRAINT [FK_ClubDisciplines_Disciplines];

ALTER TABLE [dbo].[ClubDisciplines] WITH CHECK CHECK CONSTRAINT [FK_ClubDisciplines_Seasons];

ALTER TABLE [dbo].[PlayerDisciplines] WITH CHECK CHECK CONSTRAINT [FK_PlayerDisciplines_Users];

ALTER TABLE [dbo].[PlayerDisciplines] WITH CHECK CHECK CONSTRAINT [FK_PlayerDisciplines_Disciplines];

ALTER TABLE [dbo].[PlayerDisciplines] WITH CHECK CHECK CONSTRAINT [FK_PlayerDisciplines_Clubs];

ALTER TABLE [dbo].[PlayerDisciplines] WITH CHECK CHECK CONSTRAINT [FK_PlayerDisciplines_Seasons];

ALTER TABLE [dbo].[TeamDisciplines] WITH CHECK CHECK CONSTRAINT [FK_TeamDisciplines_Teams];

ALTER TABLE [dbo].[TeamDisciplines] WITH CHECK CHECK CONSTRAINT [FK_TeamDisciplines_Clubs];

ALTER TABLE [dbo].[TeamDisciplines] WITH CHECK CHECK CONSTRAINT [FK_TeamDisciplines_Seasons];

ALTER TABLE [dbo].[TeamDisciplines] WITH CHECK CHECK CONSTRAINT [FK_TeamDisciplines_Disciplines];