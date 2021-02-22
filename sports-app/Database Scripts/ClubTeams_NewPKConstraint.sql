ALTER TABLE [dbo].[ClubTeams] DROP CONSTRAINT [DF__ClubTeams__IsBlo__36D11DD4];
ALTER TABLE [dbo].[ClubTeams] DROP CONSTRAINT [DF__ClubTeams__TeamP__3B95D2F1];

ALTER TABLE [dbo].[ClubTeams] DROP CONSTRAINT [DF__ClubTeams__IsTra__4BB72C21];
ALTER TABLE [dbo].[ClubTeams] DROP CONSTRAINT [FK_DepartmentId];
ALTER TABLE [dbo].[ClubTeams] DROP CONSTRAINT [FK_ClubTeams_Clubs];
ALTER TABLE [dbo].[ClubTeams] DROP CONSTRAINT [FK_ClubTeams_Teams];
ALTER TABLE [dbo].[ClubTeams] DROP CONSTRAINT [FK_ClubTeams_Seasons];
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [dbo].[tmp_ms_xx_ClubTeams] (
    [ClubId]         INT NOT NULL,
    [TeamId]         INT NOT NULL,
    [SeasonId]       INT NOT NULL,
    [IsBlocked]      BIT CONSTRAINT [DF__ClubTeams__IsBlo__36D11DD4] DEFAULT ((0)) NOT NULL,
    [TeamPosition]   INT CONSTRAINT [DF__ClubTeams__TeamP__3B95D2F1] DEFAULT ((0)) NOT NULL,
    [DepartmentId]   INT NULL,
    [IsTrainingTeam] BIT CONSTRAINT [DF__ClubTeams__IsTra__4BB72C21] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_ClubTeams1] PRIMARY KEY CLUSTERED ([ClubId] ASC, [TeamId] ASC, [SeasonId] ASC) WITH (FILLFACTOR = 90)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [dbo].[ClubTeams])
    BEGIN
        INSERT INTO [dbo].[tmp_ms_xx_ClubTeams] ([ClubId], [TeamId], [SeasonId], [IsBlocked], [TeamPosition], [DepartmentId], [IsTrainingTeam])
        SELECT   [ClubId],
                 [TeamId],
                 [SeasonId],
                 [IsBlocked],
                 [TeamPosition],
                 [DepartmentId],
                 [IsTrainingTeam]
        FROM     [dbo].[ClubTeams]
        ORDER BY [ClubId] ASC, [TeamId] ASC, [SeasonId] ASC;
    END

DROP TABLE [dbo].[ClubTeams];

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_ClubTeams]', N'ClubTeams';

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_constraint_PK_ClubTeams1]', N'PK_ClubTeams', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

ALTER TABLE [dbo].[ClubTeams] WITH NOCHECK
    ADD CONSTRAINT [FK_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Clubs] ([ClubId]);
ALTER TABLE [dbo].[ClubTeams] WITH NOCHECK
    ADD CONSTRAINT [FK_ClubTeams_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]) ON DELETE CASCADE;
ALTER TABLE [dbo].[ClubTeams] WITH NOCHECK
    ADD CONSTRAINT [FK_ClubTeams_Teams] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]) ON DELETE CASCADE;
ALTER TABLE [dbo].[ClubTeams] WITH NOCHECK
    ADD CONSTRAINT [FK_ClubTeams_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[ClubTeams] WITH CHECK CHECK CONSTRAINT [FK_DepartmentId];

ALTER TABLE [dbo].[ClubTeams] WITH CHECK CHECK CONSTRAINT [FK_ClubTeams_Clubs];

ALTER TABLE [dbo].[ClubTeams] WITH CHECK CHECK CONSTRAINT [FK_ClubTeams_Teams];

ALTER TABLE [dbo].[ClubTeams] WITH CHECK CHECK CONSTRAINT [FK_ClubTeams_Seasons];