GO
PRINT N'Altering [dbo].[Games]...';


GO
ALTER TABLE [dbo].[Games]
    ADD [SeasonId] INT NULL;


GO
PRINT N'Altering [dbo].[Groups]...';


GO
ALTER TABLE [dbo].[Groups]
    ADD [SeasonId] INT NULL;


GO
PRINT N'Altering [dbo].[GroupsTeams]...';


GO
ALTER TABLE [dbo].[GroupsTeams]
    ADD [SeasonId] INT NULL;


GO
PRINT N'Altering [dbo].[Leagues]...';


GO
ALTER TABLE [dbo].[Leagues]
    ADD [SortOrder] SMALLINT CONSTRAINT [DF_Leagues_SortOrder] DEFAULT ((0)) NOT NULL,
        [SeasonId]  INT      NULL;


GO
PRINT N'Altering [dbo].[LeaguesDocs]...';


GO
ALTER TABLE [dbo].[LeaguesDocs]
    ADD [SeasonId] INT NULL;


GO
PRINT N'Altering [dbo].[PlayoffBrackets]...';


GO
ALTER TABLE [dbo].[PlayoffBrackets]
    ADD [SeasonId] INT NULL;


GO
PRINT N'Altering [dbo].[Teams]...';


GO
ALTER TABLE [dbo].[Teams]
    ADD [SeasonId] INT NULL;


GO
PRINT N'Altering [dbo].[TeamsPlayers]...';


GO
ALTER TABLE [dbo].[TeamsPlayers]
    ADD [SeasonId] INT NULL;


GO
PRINT N'Creating [dbo].[Seasons]...';


GO
CREATE TABLE [dbo].[Seasons] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (80)  NOT NULL,
    [Description] NVARCHAR (200) NULL,
    [StartDate]   DATETIME       NOT NULL,
    [EndDate]     DATETIME       NOT NULL,
    CONSTRAINT [PK_Seasons] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
PRINT N'Creating [dbo].[FK_GroupsTeams_Groups]...';


GO
ALTER TABLE [dbo].[GroupsTeams] WITH NOCHECK
    ADD CONSTRAINT [FK_GroupsTeams_Groups] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]) ON DELETE CASCADE;


GO
PRINT N'Creating [dbo].[FK_Games_Seasons]...';


GO
ALTER TABLE [dbo].[Games] WITH NOCHECK
    ADD CONSTRAINT [FK_Games_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


GO
PRINT N'Creating [dbo].[FK_Groups_Seasons]...';


GO
ALTER TABLE [dbo].[Groups] WITH NOCHECK
    ADD CONSTRAINT [FK_Groups_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


GO
PRINT N'Creating [dbo].[FK_GroupsTeams_Seasons]...';


GO
ALTER TABLE [dbo].[GroupsTeams] WITH NOCHECK
    ADD CONSTRAINT [FK_GroupsTeams_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


GO
PRINT N'Creating [dbo].[FK_Leagues_Seasons]...';


GO
ALTER TABLE [dbo].[Leagues] WITH NOCHECK
    ADD CONSTRAINT [FK_Leagues_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


GO
PRINT N'Creating [dbo].[FK_LeaguesDocs_Seasons]...';


GO
ALTER TABLE [dbo].[LeaguesDocs] WITH NOCHECK
    ADD CONSTRAINT [FK_LeaguesDocs_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


GO
PRINT N'Creating [dbo].[FK_PlayoffBrackets_Seasons]...';


GO
ALTER TABLE [dbo].[PlayoffBrackets] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayoffBrackets_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


GO
PRINT N'Creating [dbo].[FK_Teams_Seasons]...';


GO
ALTER TABLE [dbo].[Teams] WITH NOCHECK
    ADD CONSTRAINT [FK_Teams_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


GO
PRINT N'Creating [dbo].[FK_TeamsPlayers_Seasons]...';


GO
ALTER TABLE [dbo].[TeamsPlayers] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamsPlayers_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


GO
PRINT N'Checking existing data against newly created constraints';


GO
ALTER TABLE [dbo].[GroupsTeams] WITH CHECK CHECK CONSTRAINT [FK_GroupsTeams_Groups];

ALTER TABLE [dbo].[Games] WITH CHECK CHECK CONSTRAINT [FK_Games_Seasons];

ALTER TABLE [dbo].[Groups] WITH CHECK CHECK CONSTRAINT [FK_Groups_Seasons];

ALTER TABLE [dbo].[GroupsTeams] WITH CHECK CHECK CONSTRAINT [FK_GroupsTeams_Seasons];

ALTER TABLE [dbo].[Leagues] WITH CHECK CHECK CONSTRAINT [FK_Leagues_Seasons];

ALTER TABLE [dbo].[LeaguesDocs] WITH CHECK CHECK CONSTRAINT [FK_LeaguesDocs_Seasons];

ALTER TABLE [dbo].[PlayoffBrackets] WITH CHECK CHECK CONSTRAINT [FK_PlayoffBrackets_Seasons];

ALTER TABLE [dbo].[Teams] WITH CHECK CHECK CONSTRAINT [FK_Teams_Seasons];

ALTER TABLE [dbo].[TeamsPlayers] WITH CHECK CHECK CONSTRAINT [FK_TeamsPlayers_Seasons];


GO
PRINT N'Update complete.';


GO
