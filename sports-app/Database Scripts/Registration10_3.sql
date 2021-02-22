ALTER TABLE [dbo].[ActivityFormsSubmittedData] DROP CONSTRAINT [FK_ActivityFormsSubmittedData_Leagues];

ALTER TABLE [dbo].[PlayerDiscounts] DROP CONSTRAINT [FK_PlayerDiscounts_Leagues];

ALTER TABLE [dbo].[ActivityFormsSubmittedData] ALTER COLUMN [LeagueId] INT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [RegistrationPaid]   DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_RegistrationPaid] DEFAULT ((0)) NOT NULL,
        [ClubId]             INT             NULL,
        [ParticipationPrice] DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_ParticipationPrice] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[PlayerDiscounts] ALTER COLUMN [LeagueId] INT NULL;

ALTER TABLE [dbo].[PlayerDiscounts]
    ADD [ClubId] INT NULL;

ALTER TABLE [dbo].[Teams]
    ADD [MinimumAge] DATETIME NULL,
        [MaximumAge] DATETIME NULL;

ALTER TABLE [dbo].[Users]
    ADD [ParentName] NVARCHAR (MAX) NULL;

CREATE TABLE [dbo].[ClubTeamPrices] (
    [Id]        INT             IDENTITY (1, 1) NOT NULL,
    [ClubId]    INT             NULL,
    [TeamId]    INT             NULL,
    [PriceType] INT             NULL,
    [Price]     DECIMAL (18, 2) NULL,
    [StartDate] DATETIME        NULL,
    [EndDate]   DATETIME        NULL,
    CONSTRAINT [PK_ClubTeamPrices] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[Schools] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [ClubId]      INT            NOT NULL,
    [SeasonId]    INT            NOT NULL,
    [Name]        NVARCHAR (MAX) NOT NULL,
    [CreatedBy]   INT            NOT NULL,
    [DateCreated] DATETIME       NOT NULL,
    CONSTRAINT [PK_Schools] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[SchoolTeams] (
    [Id]       INT IDENTITY (1, 1) NOT NULL,
    [SchoolId] INT NOT NULL,
    [TeamId]   INT NOT NULL,
    CONSTRAINT [PK_SchoolTeams] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_Leagues] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);
  
ALTER TABLE [dbo].[PlayerDiscounts] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDiscounts_Leagues] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[ClubTeamPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_ClubTeamPrices_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);

ALTER TABLE [dbo].[ClubTeamPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_ClubTeamPrices_Teams] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]);

ALTER TABLE [dbo].[Schools] WITH NOCHECK
    ADD CONSTRAINT [FK_Schools_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);

ALTER TABLE [dbo].[Schools] WITH NOCHECK
    ADD CONSTRAINT [FK_Schools_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[Schools] WITH NOCHECK
    ADD CONSTRAINT [FK_Schools_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[SchoolTeams] WITH NOCHECK
    ADD CONSTRAINT [FK_SchoolTeams_Teams] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]);

ALTER TABLE [dbo].[SchoolTeams] WITH NOCHECK
    ADD CONSTRAINT [FK_SchoolTeams_Schools] FOREIGN KEY ([SchoolId]) REFERENCES [dbo].[Schools] ([Id]);

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);

ALTER TABLE [dbo].[PlayerDiscounts] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDiscounts_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_Leagues];

ALTER TABLE [dbo].[PlayerDiscounts] WITH CHECK CHECK CONSTRAINT [FK_PlayerDiscounts_Leagues];

ALTER TABLE [dbo].[ClubTeamPrices] WITH CHECK CHECK CONSTRAINT [FK_ClubTeamPrices_Clubs];

ALTER TABLE [dbo].[ClubTeamPrices] WITH CHECK CHECK CONSTRAINT [FK_ClubTeamPrices_Teams];

ALTER TABLE [dbo].[Schools] WITH CHECK CHECK CONSTRAINT [FK_Schools_Clubs];

ALTER TABLE [dbo].[Schools] WITH CHECK CHECK CONSTRAINT [FK_Schools_Seasons];

ALTER TABLE [dbo].[Schools] WITH CHECK CHECK CONSTRAINT [FK_Schools_Users];

ALTER TABLE [dbo].[SchoolTeams] WITH CHECK CHECK CONSTRAINT [FK_SchoolTeams_Teams];

ALTER TABLE [dbo].[SchoolTeams] WITH CHECK CHECK CONSTRAINT [FK_SchoolTeams_Schools];

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_Clubs];

ALTER TABLE [dbo].[PlayerDiscounts] WITH CHECK CHECK CONSTRAINT [FK_PlayerDiscounts_Clubs];