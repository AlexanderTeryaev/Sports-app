GO
PRINT N'Dropping [dbo].[DF_Users_NoInsurancePayment]...';


GO
ALTER TABLE [dbo].[Users] DROP CONSTRAINT [DF_Users_NoInsurancePayment];


GO
PRINT N'Dropping unnamed constraint on [dbo].[Users]...';


GO
ALTER TABLE [dbo].[Users] DROP CONSTRAINT [DF__Users__IsAthlete__6621099A];


GO
PRINT N'Dropping unnamed constraint on [dbo].[Users]...';


GO
ALTER TABLE [dbo].[Users] DROP CONSTRAINT [DF__Users__TestResul__1699586C];


GO
PRINT N'Dropping [dbo].[DF_Users_IsCompetitiveMember]...';


GO
ALTER TABLE [dbo].[Users] DROP CONSTRAINT [DF_Users_IsCompetitiveMember];


GO
PRINT N'Dropping [dbo].[FK_ActivityForms_PublishedByUsers]...';


GO
ALTER TABLE [dbo].[ActivityForms] DROP CONSTRAINT [FK_ActivityForms_PublishedByUsers];


GO
PRINT N'Dropping [dbo].[FK_ActivityForms_UpdateUsers]...';


GO
ALTER TABLE [dbo].[ActivityForms] DROP CONSTRAINT [FK_ActivityForms_UpdateUsers];


GO
PRINT N'Dropping [dbo].[FK_UsersRoutes_Users]...';


GO
ALTER TABLE [dbo].[UsersRoutes] DROP CONSTRAINT [FK_UsersRoutes_Users];


GO
PRINT N'Dropping [dbo].[FK_ActivityStatusColumnNames_Users]...';


GO
ALTER TABLE [dbo].[ActivityStatusColumnNames] DROP CONSTRAINT [FK_ActivityStatusColumnNames_Users];


GO
PRINT N'Dropping [dbo].[FK_SentMessages_Users]...';


GO
ALTER TABLE [dbo].[SentMessages] DROP CONSTRAINT [FK_SentMessages_Users];


GO
PRINT N'Dropping [dbo].[FK_UsersRanks_Users]...';


GO
ALTER TABLE [dbo].[UsersRanks] DROP CONSTRAINT [FK_UsersRanks_Users];


GO
PRINT N'Dropping [dbo].[FK_UsersEducation_Users]...';


GO
ALTER TABLE [dbo].[UsersEducation] DROP CONSTRAINT [FK_UsersEducation_Users];


GO
PRINT N'Dropping [dbo].[FK_ActivityStatusColumnsSorting_Users]...';


GO
ALTER TABLE [dbo].[ActivityStatusColumnsSorting] DROP CONSTRAINT [FK_ActivityStatusColumnsSorting_Users];


GO
PRINT N'Dropping [dbo].[FK_ActivityFormsSubmittedData_Users]...';


GO
ALTER TABLE [dbo].[ActivityFormsSubmittedData] DROP CONSTRAINT [FK_ActivityFormsSubmittedData_Users];


GO
PRINT N'Dropping [dbo].[FKEY_REFEREEREG_USERID]...';


GO
ALTER TABLE [dbo].[RefereeCompetitionRegistrations] DROP CONSTRAINT [FKEY_REFEREEREG_USERID];


GO
PRINT N'Dropping [dbo].[FK_SportsRegistrations_Users]...';


GO
ALTER TABLE [dbo].[SportsRegistrations] DROP CONSTRAINT [FK_SportsRegistrations_Users];


GO
PRINT N'Dropping [dbo].[FK_DisplayedPaymentMessages_UserId]...';


GO
ALTER TABLE [dbo].[DisplayedPaymentMessages] DROP CONSTRAINT [FK_DisplayedPaymentMessages_UserId];


GO
PRINT N'Dropping [dbo].[FK_UsersNotifications_Users]...';


GO
ALTER TABLE [dbo].[UsersNotifications] DROP CONSTRAINT [FK_UsersNotifications_Users];


GO
PRINT N'Dropping [dbo].[FK_NotesMessages_Users]...';


GO
ALTER TABLE [dbo].[NotesMessages] DROP CONSTRAINT [FK_NotesMessages_Users];


GO
PRINT N'Dropping [dbo].[FK_ResetPasswordRequests_Users]...';


GO
ALTER TABLE [dbo].[ResetPasswordRequests] DROP CONSTRAINT [FK_ResetPasswordRequests_Users];


GO
PRINT N'Dropping [dbo].[FK_UsersJobs_Users]...';


GO
ALTER TABLE [dbo].[UsersJobs] DROP CONSTRAINT [FK_UsersJobs_Users];


GO
PRINT N'Dropping [dbo].[FK_ActivityStatusColumnsVisibility_Users]...';


GO
ALTER TABLE [dbo].[ActivityStatusColumnsVisibility] DROP CONSTRAINT [FK_ActivityStatusColumnsVisibility_Users];


GO
PRINT N'Dropping [dbo].[FK_LeaguesFans_Users]...';


GO
ALTER TABLE [dbo].[LeaguesFans] DROP CONSTRAINT [FK_LeaguesFans_Users];


GO
PRINT N'Dropping [dbo].[FK_NotesRecipients_Users]...';


GO
ALTER TABLE [dbo].[NotesRecipients] DROP CONSTRAINT [FK_NotesRecipients_Users];


GO
PRINT N'Dropping [dbo].[FK_PenaltyForExclusion_ActionUserId]...';


GO
ALTER TABLE [dbo].[PenaltyForExclusion] DROP CONSTRAINT [FK_PenaltyForExclusion_ActionUserId];


GO
PRINT N'Dropping unnamed constraint on [dbo].[PenaltyForExclusion]...';


GO
ALTER TABLE [dbo].[PenaltyForExclusion] DROP CONSTRAINT [FK__PenaltyFo__UserI__3CFEF876];


GO
PRINT N'Dropping [dbo].[FK_Schools_Users]...';


GO
ALTER TABLE [dbo].[Schools] DROP CONSTRAINT [FK_Schools_Users];


GO
PRINT N'Dropping [dbo].[FK_TennisRank_Users]...';


GO
ALTER TABLE [dbo].[TennisRank] DROP CONSTRAINT [FK_TennisRank_Users];


GO
PRINT N'Dropping [dbo].[FK_LeagueScheduleState_UserId]...';


GO
ALTER TABLE [dbo].[LeagueScheduleState] DROP CONSTRAINT [FK_LeagueScheduleState_UserId];


GO
PRINT N'Dropping [dbo].[FK_UsersDvices_Users]...';


GO
ALTER TABLE [dbo].[UsersDvices] DROP CONSTRAINT [FK_UsersDvices_Users];


GO
PRINT N'Dropping [dbo].[FK_WallThreads_Users]...';


GO
ALTER TABLE [dbo].[WallThreads] DROP CONSTRAINT [FK_WallThreads_Users];


GO
PRINT N'Dropping [dbo].[FK_PlayerDisciplines_Users]...';


GO
ALTER TABLE [dbo].[PlayerDisciplines] DROP CONSTRAINT [FK_PlayerDisciplines_Users];


GO
PRINT N'Dropping [dbo].[FK_Messages_Users]...';


GO
ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_Messages_Users];


GO
PRINT N'Dropping [dbo].[FK_TeamsRanks_Users]...';


GO
ALTER TABLE [dbo].[TeamsRanks] DROP CONSTRAINT [FK_TeamsRanks_Users];


GO
PRINT N'Dropping [dbo].[FK_TeamsRoutes_Users]...';


GO
ALTER TABLE [dbo].[TeamsRoutes] DROP CONSTRAINT [FK_TeamsRoutes_Users];


GO
PRINT N'Dropping [dbo].[FK_Users_AdditionalTeamGymnastic]...';


GO
ALTER TABLE [dbo].[AdditionalTeamGymnastics] DROP CONSTRAINT [FK_Users_AdditionalTeamGymnastic];


GO
PRINT N'Dropping [dbo].[FK_MedicalCertApprovements_Users]...';


GO
ALTER TABLE [dbo].[MedicalCertApprovements] DROP CONSTRAINT [FK_MedicalCertApprovements_Users];


GO
PRINT N'Dropping [dbo].[FK_ActivityBranchesState_Users]...';


GO
ALTER TABLE [dbo].[ActivityBranchesState] DROP CONSTRAINT [FK_ActivityBranchesState_Users];


GO
PRINT N'Dropping [dbo].[FK_PlayerAchievements_Users]...';


GO
ALTER TABLE [dbo].[PlayerAchievements] DROP CONSTRAINT [FK_PlayerAchievements_Users];


GO
PRINT N'Dropping [dbo].[FK_ClubGamesFans_Users]...';


GO
ALTER TABLE [dbo].[ClubGamesFans] DROP CONSTRAINT [FK_ClubGamesFans_Users];


GO
PRINT N'Dropping [dbo].[FK_TennisLeagueGame_HomePLayer]...';


GO
ALTER TABLE [dbo].[TennisLeagueGames] DROP CONSTRAINT [FK_TennisLeagueGame_HomePLayer];


GO
PRINT N'Dropping [dbo].[FK_TennisLeagueGame_GuestPlayer]...';


GO
ALTER TABLE [dbo].[TennisLeagueGames] DROP CONSTRAINT [FK_TennisLeagueGame_GuestPlayer];


GO
PRINT N'Dropping [dbo].[FK_TennisLeagueGame_TechnicalWinner]...';


GO
ALTER TABLE [dbo].[TennisLeagueGames] DROP CONSTRAINT [FK_TennisLeagueGame_TechnicalWinner];


GO
PRINT N'Dropping [dbo].[FK_TennisLeagueGame_HomePairPlayer]...';


GO
ALTER TABLE [dbo].[TennisLeagueGames] DROP CONSTRAINT [FK_TennisLeagueGame_HomePairPlayer];


GO
PRINT N'Dropping [dbo].[FK_TennisLeagueGame_GuestPairPlayer]...';


GO
ALTER TABLE [dbo].[TennisLeagueGames] DROP CONSTRAINT [FK_TennisLeagueGame_GuestPairPlayer];


GO
PRINT N'Dropping [dbo].[FK_UsersFriends_Users]...';


GO
ALTER TABLE [dbo].[UsersFriends] DROP CONSTRAINT [FK_UsersFriends_Users];


GO
PRINT N'Dropping [dbo].[FK_UsersFriends_Friends]...';


GO
ALTER TABLE [dbo].[UsersFriends] DROP CONSTRAINT [FK_UsersFriends_Friends];


GO
PRINT N'Dropping [dbo].[FK_TeamsFans_Users]...';


GO
ALTER TABLE [dbo].[TeamsFans] DROP CONSTRAINT [FK_TeamsFans_Users];


GO
PRINT N'Dropping [dbo].[FK_InitialApprovalDates_Users]...';


GO
ALTER TABLE [dbo].[InitialApprovalDates] DROP CONSTRAINT [FK_InitialApprovalDates_Users];


GO
PRINT N'Dropping [dbo].[FK_GamesFans_Users]...';


GO
ALTER TABLE [dbo].[GamesFans] DROP CONSTRAINT [FK_GamesFans_Users];


GO
PRINT N'Dropping [dbo].[FK_UsersApprovalDatesHistory_ActionUserId]...';


GO
ALTER TABLE [dbo].[UsersApprovalDatesHistory] DROP CONSTRAINT [FK_UsersApprovalDatesHistory_ActionUserId];


GO
PRINT N'Dropping [dbo].[FK_UsersApprovalDatesHistory_Users]...';


GO
ALTER TABLE [dbo].[UsersApprovalDatesHistory] DROP CONSTRAINT [FK_UsersApprovalDatesHistory_Users];


GO
PRINT N'Dropping [dbo].[FK_TeamPlayers_Users]...';


GO
ALTER TABLE [dbo].[TeamsPlayers] DROP CONSTRAINT [FK_TeamPlayers_Users];


GO
PRINT N'Dropping [dbo].[FK_TeamsPlayers_ActionUserId]...';


GO
ALTER TABLE [dbo].[TeamsPlayers] DROP CONSTRAINT [FK_TeamsPlayers_ActionUserId];


GO
PRINT N'Dropping [dbo].[FK_TeamsPlayers_Users]...';


GO
ALTER TABLE [dbo].[TeamsPlayers] DROP CONSTRAINT [FK_TeamsPlayers_Users];


GO
PRINT N'Dropping [dbo].[FK_RefereeSalaryReports_Users]...';


GO
ALTER TABLE [dbo].[RefereeSalaryReports] DROP CONSTRAINT [FK_RefereeSalaryReports_Users];


GO
PRINT N'Dropping [dbo].[FK_Users_CompetitionRegistration]...';


GO
ALTER TABLE [dbo].[CompetitionRegistrations] DROP CONSTRAINT [FK_Users_CompetitionRegistration];


GO
PRINT N'Dropping [dbo].[FK_PlayerDiscounts_PlayerDiscounts]...';


GO
ALTER TABLE [dbo].[PlayerDiscounts] DROP CONSTRAINT [FK_PlayerDiscounts_PlayerDiscounts];


GO
PRINT N'Dropping [dbo].[FK_PlayerDiscounts_UpdatedDiscounts]...';


GO
ALTER TABLE [dbo].[PlayerDiscounts] DROP CONSTRAINT [FK_PlayerDiscounts_UpdatedDiscounts];


GO
PRINT N'Dropping [dbo].[FK_PlayersBenefactorPrices_Users]...';


GO
ALTER TABLE [dbo].[PlayersBenefactorPrices] DROP CONSTRAINT [FK_PlayersBenefactorPrices_Users];


GO
PRINT N'Dropping [dbo].[FK_PlayersBlockade_ActionUserId]...';


GO
ALTER TABLE [dbo].[PlayersBlockade] DROP CONSTRAINT [FK_PlayersBlockade_ActionUserId];


GO
PRINT N'Dropping [dbo].[FK_UsersBlockade]...';


GO
ALTER TABLE [dbo].[PlayersBlockade] DROP CONSTRAINT [FK_UsersBlockade];


GO
PRINT N'Dropping [dbo].[FK_ActivitiesUsers_Users]...';


GO
ALTER TABLE [dbo].[ActivitiesUsers] DROP CONSTRAINT [FK_ActivitiesUsers_Users];


GO
PRINT N'Dropping [dbo].[FK_CompetitionDisciplineRegistrations_UserId]...';


GO
ALTER TABLE [dbo].[CompetitionDisciplineRegistrations] DROP CONSTRAINT [FK_CompetitionDisciplineRegistrations_UserId];


GO
PRINT N'Dropping [dbo].[FK_Users_AdditionalGymnastic]...';


GO
ALTER TABLE [dbo].[AdditionalGymnastics] DROP CONSTRAINT [FK_Users_AdditionalGymnastic];


GO
PRINT N'Dropping [dbo].[FK_ClubBalances_ActionUserId]...';


GO
ALTER TABLE [dbo].[ClubBalances] DROP CONSTRAINT [FK_ClubBalances_ActionUserId];


GO
PRINT N'Dropping [dbo].[FK_ManagerIdValue]...';


GO
ALTER TABLE [dbo].[BlockadeNotifications] DROP CONSTRAINT [FK_ManagerIdValue];


GO
PRINT N'Dropping [dbo].[FK_ActivityStatusColumnsOrder_Users]...';


GO
ALTER TABLE [dbo].[ActivityStatusColumnsOrder] DROP CONSTRAINT [FK_ActivityStatusColumnsOrder_Users];


GO
PRINT N'Dropping unnamed constraint on [dbo].[Users]...';


GO
ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK__Users__BlockadeI__74643BF9];


GO
PRINT N'Dropping [dbo].[FK_Users_Genders]...';


GO
ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK_Users_Genders];


GO
PRINT N'Dropping [dbo].[FK_Users_Users]...';


GO
ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK_Users_Users];


GO
PRINT N'Dropping [dbo].[FK_PlayerFiles_Users]...';


GO
ALTER TABLE [dbo].[PlayerFiles] DROP CONSTRAINT [FK_PlayerFiles_Users];


GO
PRINT N'Dropping [dbo].[FK_RetirementRequests_RequestUser]...';


GO
ALTER TABLE [dbo].[RetirementRequests] DROP CONSTRAINT [FK_RetirementRequests_RequestUser];


GO
PRINT N'Dropping [dbo].[FK_RetirementRequests_ApproveUser]...';


GO
ALTER TABLE [dbo].[RetirementRequests] DROP CONSTRAINT [FK_RetirementRequests_ApproveUser];


GO
PRINT N'Dropping [dbo].[FK_PlayerHistory_Users]...';


GO
ALTER TABLE [dbo].[PlayerHistory] DROP CONSTRAINT [FK_PlayerHistory_Users];


GO
PRINT N'Dropping [dbo].[FK_PlayerHistory_ActionUserId]...';


GO
ALTER TABLE [dbo].[PlayerHistory] DROP CONSTRAINT [FK_PlayerHistory_ActionUserId];


GO
PRINT N'Starting rebuilding table [dbo].[Users]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [dbo].[tmp_ms_xx_Users] (
    [UserId]                   INT            IDENTITY (1, 1) NOT NULL,
    [TypeId]                   INT            NOT NULL,
    [FbId]                     VARCHAR (50)   NULL,
    [UserName]                 NVARCHAR (250) NULL,
    [Email]                    NVARCHAR (250) NULL,
    [DeprecatedFullName]       NVARCHAR (150) NULL,
    [IdentNum]                 NVARCHAR (20)   NULL,
    [IdentCard]                NVARCHAR (20)  NULL,
    [IsActive]                 BIT            NOT NULL,
    [Password]                 NVARCHAR (150) NULL,
    [PassHint]                 NVARCHAR (50)  NULL,
    [TriesNum]                 INT            NOT NULL,
    [IsBlocked]                BIT            NOT NULL,
    [SessionId]                NVARCHAR (50)  NULL,
    [BirthDay]                 DATETIME       NULL,
    [City]                     NVARCHAR (150) NULL,
    [Image]                    NVARCHAR (500) NULL,
    [Height]                   INT            NULL,
    [GenderId]                 INT            NULL,
    [IsArchive]                BIT            NOT NULL,
    [Telephone]                NVARCHAR (50)  NULL,
    [MedicalCertificate]       BIT            NULL,
    [Insurance]                BIT            NULL,
    [LangId]                   INT            NULL,
    [TestResults]              INT            NOT NULL,
    [InsuranceFile]            NVARCHAR (500) NULL,
    [MedicalCertificateFile]   NVARCHAR (500) NULL,
    [ShirtSize]                NVARCHAR (3)   NULL,
    [Weight]                   INT            NULL,
    [WeightUnits]              NVARCHAR (2)   NULL,
    [WeightDate]               DATETIME       NULL,
    [IsStartAlert]             BIT            NULL,
    [IsTimeChange]             BIT            NULL,
    [IsGameScores]             BIT            NULL,
    [ParentName]               NVARCHAR (MAX) NULL,
    [NoInsurancePayment]       BIT            NOT NULL,
    [PassportNum]              VARCHAR (20)   NULL,
    [IDFile]                   VARCHAR (MAX)  NULL,
    [BlockadeId]               INT            NULL,
    [Address]                  NVARCHAR (500) NULL,
    [ForeignFirstName]         VARCHAR (255)  NULL,
    [DateOfInsurance]          DATETIME       NULL,
    [TenicardValidity]         DATETIME       NULL,
    [MedExamDate]              DATETIME       NULL,
    [IsCompetitiveMember]      BIT            NOT NULL,
    [CompetitiveLicenseNumber] VARCHAR (255)  NULL,
    [LicenseValidity]          DATETIME       NULL,
    [LicenseLevel]             VARCHAR (255)  NULL,
    [FirstName]                NVARCHAR (255) NULL,
    [LastName]                 NVARCHAR (255) NULL,
    [PassportFile]             VARCHAR (255)  NULL,
    [AthleteNumber]            INT            NULL,
    [ArgometricTestValidity]   DATETIME       NULL,
    [RefereeCertificate]       NVARCHAR (50)  NULL,
    [IsAthleteNumberProduced]  BIT            NOT NULL,
    [SeasonIdOfCreation]       INT            NULL,
    [ClassS]                   INT            NULL,
    [ClassSB]                  INT            NULL,
    [ClassSM]                  INT            NULL,
    [MiddleName]               NVARCHAR (255) NULL,
    [MotherName]               NVARCHAR (MAX) NULL,
    [ParentPhone]              NVARCHAR (50)  NULL,
    [ParentEmail]              NVARCHAR (255) NULL,
    [ForeignLastName]          NVARCHAR (255) NULL,
    [UciId]                    INT            NULL,
    [ChipNumber]               NVARCHAR (50)  NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_Admins1] PRIMARY KEY CLUSTERED ([UserId] ASC) WITH (FILLFACTOR = 90)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [dbo].[Users])
    BEGIN
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_Users] ON;
        INSERT INTO [dbo].[tmp_ms_xx_Users] ([UserId], [TypeId], [FbId], [UserName], [Email], [DeprecatedFullName], [IdentNum], [IdentCard], [IsActive], [Password], [PassHint], [TriesNum], [IsBlocked], [SessionId], [BirthDay], [City], [Image], [Height], [GenderId], [IsArchive], [Telephone], [MedicalCertificate], [Insurance], [LangId], [TestResults], [InsuranceFile], [MedicalCertificateFile], [ShirtSize], [Weight], [WeightUnits], [WeightDate], [IsStartAlert], [IsTimeChange], [IsGameScores], [ParentName], [NoInsurancePayment], [PassportNum], [IDFile], [BlockadeId], [Address], [ForeignFirstName], [DateOfInsurance], [TenicardValidity], [MedExamDate], [IsCompetitiveMember], [CompetitiveLicenseNumber], [LicenseValidity], [LicenseLevel], [FirstName], [LastName], [PassportFile], [AthleteNumber], [ArgometricTestValidity], [RefereeCertificate], [IsAthleteNumberProduced], [SeasonIdOfCreation], [ClassS], [ClassSB], [ClassSM], [UciId], [ChipNumber])
        SELECT   [UserId],
                 [TypeId],
                 [FbId],
                 [UserName],
                 [Email],
				 [FullName],
                 [IdentNum],
                 [IdentCard],
                 [IsActive],
                 [Password],
                 [PassHint],
                 [TriesNum],
                 [IsBlocked],
                 [SessionId],
                 [BirthDay],
                 [City],
                 [Image],
                 [Height],
                 [GenderId],
                 [IsArchive],
                 [Telephone],
                 [MedicalCertificate],
                 [Insurance],
                 [LangId],
                 [TestResults],
                 [InsuranceFile],
                 [MedicalCertificateFile],
                 [ShirtSize],
                 [Weight],
                 [WeightUnits],
                 [WeightDate],
                 [IsStartAlert],
                 [IsTimeChange],
                 [IsGameScores],
                 [ParentName],
                 [NoInsurancePayment],
                 [PassportNum],
                 [IDFile],
                 [BlockadeId],
                 [Address],
                 [ForeignName],
                 [DateOfInsurance],
                 [TenicardValidity],
                 [MedExamDate],
                 [IsCompetitiveMember],
                 [CompetitiveLicenseNumber],
                 [LicenseValidity],
                 [LicenseLevel],
                 [FirstName],
                 [LastName],
                 [PassportFile],
                 [AthleteNumber],
                 [ArgometricTestValidity],
                 [RefereeCertificate],
                 [IsAthleteNumberProduced],
                 [SeasonIdOfCreation],
                 [ClassS],
                 [ClassSB],
                 [ClassSM],
                 [UciId],
                 [ChipNumber]
        FROM     [dbo].[Users]
        ORDER BY [UserId] ASC;
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_Users] OFF;
    END

DROP TABLE [dbo].[Users];

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_Users]', N'Users';

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_constraint_PK_Admins1]', N'PK_Admins', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Creating [dbo].[FK_ActivityForms_PublishedByUsers]...';


GO
ALTER TABLE [dbo].[ActivityForms] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityForms_PublishedByUsers] FOREIGN KEY ([PublishedBy]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_ActivityForms_UpdateUsers]...';


GO
ALTER TABLE [dbo].[ActivityForms] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityForms_UpdateUsers] FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_UsersRoutes_Users]...';


GO
ALTER TABLE [dbo].[UsersRoutes] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersRoutes_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_ActivityStatusColumnNames_Users]...';


GO
ALTER TABLE [dbo].[ActivityStatusColumnNames] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnNames_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_SentMessages_Users]...';


GO
ALTER TABLE [dbo].[SentMessages] WITH NOCHECK
    ADD CONSTRAINT [FK_SentMessages_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_UsersRanks_Users]...';


GO
ALTER TABLE [dbo].[UsersRanks] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersRanks_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_UsersEducation_Users]...';


GO
ALTER TABLE [dbo].[UsersEducation] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersEducation_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_ActivityStatusColumnsSorting_Users]...';


GO
ALTER TABLE [dbo].[ActivityStatusColumnsSorting] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnsSorting_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_ActivityFormsSubmittedData_Users]...';


GO
ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_Users] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FKEY_REFEREEREG_USERID]...';


GO
ALTER TABLE [dbo].[RefereeCompetitionRegistrations] WITH NOCHECK
    ADD CONSTRAINT [FKEY_REFEREEREG_USERID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_SportsRegistrations_Users]...';


GO
ALTER TABLE [dbo].[SportsRegistrations] WITH NOCHECK
    ADD CONSTRAINT [FK_SportsRegistrations_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_DisplayedPaymentMessages_UserId]...';


GO
ALTER TABLE [dbo].[DisplayedPaymentMessages] WITH NOCHECK
    ADD CONSTRAINT [FK_DisplayedPaymentMessages_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_UsersNotifications_Users]...';


GO
ALTER TABLE [dbo].[UsersNotifications] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersNotifications_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE;


GO
PRINT N'Creating [dbo].[FK_NotesMessages_Users]...';


GO
ALTER TABLE [dbo].[NotesMessages] WITH NOCHECK
    ADD CONSTRAINT [FK_NotesMessages_Users] FOREIGN KEY ([Sender]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_ResetPasswordRequests_Users]...';


GO
ALTER TABLE [dbo].[ResetPasswordRequests] WITH NOCHECK
    ADD CONSTRAINT [FK_ResetPasswordRequests_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_UsersJobs_Users]...';


GO
ALTER TABLE [dbo].[UsersJobs] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersJobs_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_ActivityStatusColumnsVisibility_Users]...';


GO
ALTER TABLE [dbo].[ActivityStatusColumnsVisibility] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnsVisibility_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_LeaguesFans_Users]...';


GO
ALTER TABLE [dbo].[LeaguesFans] WITH NOCHECK
    ADD CONSTRAINT [FK_LeaguesFans_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_NotesRecipients_Users]...';


GO
ALTER TABLE [dbo].[NotesRecipients] WITH NOCHECK
    ADD CONSTRAINT [FK_NotesRecipients_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_PenaltyForExclusion_ActionUserId]...';


GO
ALTER TABLE [dbo].[PenaltyForExclusion] WITH NOCHECK
    ADD CONSTRAINT [FK_PenaltyForExclusion_ActionUserId] FOREIGN KEY ([ActionUserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating unnamed constraint on [dbo].[PenaltyForExclusion]...';


GO
ALTER TABLE [dbo].[PenaltyForExclusion] WITH NOCHECK
    ADD FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_Schools_Users]...';


GO
ALTER TABLE [dbo].[Schools] WITH NOCHECK
    ADD CONSTRAINT [FK_Schools_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_TennisRank_Users]...';


GO
ALTER TABLE [dbo].[TennisRank] WITH NOCHECK
    ADD CONSTRAINT [FK_TennisRank_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_LeagueScheduleState_UserId]...';


GO
ALTER TABLE [dbo].[LeagueScheduleState] WITH NOCHECK
    ADD CONSTRAINT [FK_LeagueScheduleState_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_UsersDvices_Users]...';


GO
ALTER TABLE [dbo].[UsersDvices] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersDvices_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_WallThreads_Users]...';


GO
ALTER TABLE [dbo].[WallThreads] WITH NOCHECK
    ADD CONSTRAINT [FK_WallThreads_Users] FOREIGN KEY ([CreaterId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_PlayerDisciplines_Users]...';


GO
ALTER TABLE [dbo].[PlayerDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDisciplines_Users] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_Messages_Users]...';


GO
ALTER TABLE [dbo].[Messages] WITH NOCHECK
    ADD CONSTRAINT [FK_Messages_Users] FOREIGN KEY ([SenderId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_TeamsRanks_Users]...';


GO
ALTER TABLE [dbo].[TeamsRanks] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamsRanks_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE ON UPDATE CASCADE;


GO
PRINT N'Creating [dbo].[FK_TeamsRoutes_Users]...';


GO
ALTER TABLE [dbo].[TeamsRoutes] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamsRoutes_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE ON UPDATE CASCADE;


GO
PRINT N'Creating [dbo].[FK_Users_AdditionalTeamGymnastic]...';


GO
ALTER TABLE [dbo].[AdditionalTeamGymnastics] WITH NOCHECK
    ADD CONSTRAINT [FK_Users_AdditionalTeamGymnastic] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_MedicalCertApprovements_Users]...';


GO
ALTER TABLE [dbo].[MedicalCertApprovements] WITH NOCHECK
    ADD CONSTRAINT [FK_MedicalCertApprovements_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_ActivityBranchesState_Users]...';


GO
ALTER TABLE [dbo].[ActivityBranchesState] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityBranchesState_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_PlayerAchievements_Users]...';


GO
ALTER TABLE [dbo].[PlayerAchievements] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerAchievements_Users] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_ClubGamesFans_Users]...';


GO
ALTER TABLE [dbo].[ClubGamesFans] WITH NOCHECK
    ADD CONSTRAINT [FK_ClubGamesFans_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_TennisLeagueGame_HomePLayer]...';


GO
ALTER TABLE [dbo].[TennisLeagueGames] WITH NOCHECK
    ADD CONSTRAINT [FK_TennisLeagueGame_HomePLayer] FOREIGN KEY ([HomePlayerId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_TennisLeagueGame_GuestPlayer]...';


GO
ALTER TABLE [dbo].[TennisLeagueGames] WITH NOCHECK
    ADD CONSTRAINT [FK_TennisLeagueGame_GuestPlayer] FOREIGN KEY ([GuestPlayerId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_TennisLeagueGame_TechnicalWinner]...';


GO
ALTER TABLE [dbo].[TennisLeagueGames] WITH NOCHECK
    ADD CONSTRAINT [FK_TennisLeagueGame_TechnicalWinner] FOREIGN KEY ([TechnicalWinnerId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_TennisLeagueGame_HomePairPlayer]...';


GO
ALTER TABLE [dbo].[TennisLeagueGames] WITH NOCHECK
    ADD CONSTRAINT [FK_TennisLeagueGame_HomePairPlayer] FOREIGN KEY ([HomePairPlayerId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_TennisLeagueGame_GuestPairPlayer]...';


GO
ALTER TABLE [dbo].[TennisLeagueGames] WITH NOCHECK
    ADD CONSTRAINT [FK_TennisLeagueGame_GuestPairPlayer] FOREIGN KEY ([GuestPairPlayerId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_UsersFriends_Users]...';


GO
ALTER TABLE [dbo].[UsersFriends] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersFriends_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_UsersFriends_Friends]...';


GO
ALTER TABLE [dbo].[UsersFriends] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersFriends_Friends] FOREIGN KEY ([FriendId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_TeamsFans_Users]...';


GO
ALTER TABLE [dbo].[TeamsFans] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamsFans_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_InitialApprovalDates_Users]...';


GO
ALTER TABLE [dbo].[InitialApprovalDates] WITH NOCHECK
    ADD CONSTRAINT [FK_InitialApprovalDates_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_GamesFans_Users]...';


GO
ALTER TABLE [dbo].[GamesFans] WITH NOCHECK
    ADD CONSTRAINT [FK_GamesFans_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_UsersApprovalDatesHistory_ActionUserId]...';


GO
ALTER TABLE [dbo].[UsersApprovalDatesHistory] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersApprovalDatesHistory_ActionUserId] FOREIGN KEY ([ActionUserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_UsersApprovalDatesHistory_Users]...';


GO
ALTER TABLE [dbo].[UsersApprovalDatesHistory] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersApprovalDatesHistory_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_TeamPlayers_Users]...';


GO
ALTER TABLE [dbo].[TeamsPlayers] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamPlayers_Users] FOREIGN KEY ([PersonalCoachId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_TeamsPlayers_ActionUserId]...';


GO
ALTER TABLE [dbo].[TeamsPlayers] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamsPlayers_ActionUserId] FOREIGN KEY ([ActionUserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_TeamsPlayers_Users]...';


GO
ALTER TABLE [dbo].[TeamsPlayers] WITH NOCHECK
    ADD CONSTRAINT [FK_TeamsPlayers_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_RefereeSalaryReports_Users]...';


GO
ALTER TABLE [dbo].[RefereeSalaryReports] WITH NOCHECK
    ADD CONSTRAINT [FK_RefereeSalaryReports_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_Users_CompetitionRegistration]...';


GO
ALTER TABLE [dbo].[CompetitionRegistrations] WITH NOCHECK
    ADD CONSTRAINT [FK_Users_CompetitionRegistration] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_PlayerDiscounts_PlayerDiscounts]...';


GO
ALTER TABLE [dbo].[PlayerDiscounts] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDiscounts_PlayerDiscounts] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_PlayerDiscounts_UpdatedDiscounts]...';


GO
ALTER TABLE [dbo].[PlayerDiscounts] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDiscounts_UpdatedDiscounts] FOREIGN KEY ([UpdateUserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_PlayersBenefactorPrices_Users]...';


GO
ALTER TABLE [dbo].[PlayersBenefactorPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayersBenefactorPrices_Users] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_PlayersBlockade_ActionUserId]...';


GO
ALTER TABLE [dbo].[PlayersBlockade] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayersBlockade_ActionUserId] FOREIGN KEY ([ActionUserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_UsersBlockade]...';


GO
ALTER TABLE [dbo].[PlayersBlockade] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersBlockade] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_ActivitiesUsers_Users]...';


GO
ALTER TABLE [dbo].[ActivitiesUsers] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivitiesUsers_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_CompetitionDisciplineRegistrations_UserId]...';


GO
ALTER TABLE [dbo].[CompetitionDisciplineRegistrations] WITH NOCHECK
    ADD CONSTRAINT [FK_CompetitionDisciplineRegistrations_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_Users_AdditionalGymnastic]...';


GO
ALTER TABLE [dbo].[AdditionalGymnastics] WITH NOCHECK
    ADD CONSTRAINT [FK_Users_AdditionalGymnastic] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_ClubBalances_ActionUserId]...';


GO
ALTER TABLE [dbo].[ClubBalances] WITH NOCHECK
    ADD CONSTRAINT [FK_ClubBalances_ActionUserId] FOREIGN KEY ([ActionUserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_ManagerIdValue]...';


GO
ALTER TABLE [dbo].[BlockadeNotifications] WITH NOCHECK
    ADD CONSTRAINT [FK_ManagerIdValue] FOREIGN KEY ([ManagerId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_ActivityStatusColumnsOrder_Users]...';


GO
ALTER TABLE [dbo].[ActivityStatusColumnsOrder] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnsOrder_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating unnamed constraint on [dbo].[Users]...';


GO
ALTER TABLE [dbo].[Users] WITH NOCHECK
    ADD FOREIGN KEY ([BlockadeId]) REFERENCES [dbo].[PlayersBlockade] ([Id]);


GO
PRINT N'Creating [dbo].[FK_Users_Genders]...';


GO
ALTER TABLE [dbo].[Users] WITH NOCHECK
    ADD CONSTRAINT [FK_Users_Genders] FOREIGN KEY ([GenderId]) REFERENCES [dbo].[Genders] ([GenderId]);


GO
PRINT N'Creating [dbo].[FK_Users_Users]...';


GO
ALTER TABLE [dbo].[Users] WITH NOCHECK
    ADD CONSTRAINT [FK_Users_Users] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[UsersTypes] ([TypeId]);


GO
PRINT N'Creating [dbo].[FK_PlayerFiles_Users]...';


GO
ALTER TABLE [dbo].[PlayerFiles] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerFiles_Users] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_RetirementRequests_RequestUser]...';


GO
ALTER TABLE [dbo].[RetirementRequests] WITH NOCHECK
    ADD CONSTRAINT [FK_RetirementRequests_RequestUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_RetirementRequests_ApproveUser]...';


GO
ALTER TABLE [dbo].[RetirementRequests] WITH NOCHECK
    ADD CONSTRAINT [FK_RetirementRequests_ApproveUser] FOREIGN KEY ([ApprovedBy]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_PlayerHistory_Users]...';


GO
ALTER TABLE [dbo].[PlayerHistory] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerHistory_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Creating [dbo].[FK_PlayerHistory_ActionUserId]...';


GO
ALTER TABLE [dbo].[PlayerHistory] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerHistory_ActionUserId] FOREIGN KEY ([ActionUserId]) REFERENCES [dbo].[Users] ([UserId]);


GO
PRINT N'Checking existing data against newly created constraints';

GO
ALTER TABLE [dbo].[ActivityForms] WITH CHECK CHECK CONSTRAINT [FK_ActivityForms_PublishedByUsers];

ALTER TABLE [dbo].[ActivityForms] WITH CHECK CHECK CONSTRAINT [FK_ActivityForms_UpdateUsers];

ALTER TABLE [dbo].[UsersRoutes] WITH CHECK CHECK CONSTRAINT [FK_UsersRoutes_Users];

ALTER TABLE [dbo].[ActivityStatusColumnNames] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnNames_Users];

ALTER TABLE [dbo].[SentMessages] WITH CHECK CHECK CONSTRAINT [FK_SentMessages_Users];

ALTER TABLE [dbo].[UsersRanks] WITH CHECK CHECK CONSTRAINT [FK_UsersRanks_Users];

ALTER TABLE [dbo].[UsersEducation] WITH CHECK CHECK CONSTRAINT [FK_UsersEducation_Users];

ALTER TABLE [dbo].[ActivityStatusColumnsSorting] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnsSorting_Users];

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_Users];

ALTER TABLE [dbo].[RefereeCompetitionRegistrations] WITH CHECK CHECK CONSTRAINT [FKEY_REFEREEREG_USERID];

ALTER TABLE [dbo].[SportsRegistrations] WITH CHECK CHECK CONSTRAINT [FK_SportsRegistrations_Users];

ALTER TABLE [dbo].[DisplayedPaymentMessages] WITH CHECK CHECK CONSTRAINT [FK_DisplayedPaymentMessages_UserId];

ALTER TABLE [dbo].[UsersNotifications] WITH CHECK CHECK CONSTRAINT [FK_UsersNotifications_Users];

ALTER TABLE [dbo].[NotesMessages] WITH CHECK CHECK CONSTRAINT [FK_NotesMessages_Users];

ALTER TABLE [dbo].[ResetPasswordRequests] WITH CHECK CHECK CONSTRAINT [FK_ResetPasswordRequests_Users];

ALTER TABLE [dbo].[UsersJobs] WITH CHECK CHECK CONSTRAINT [FK_UsersJobs_Users];

ALTER TABLE [dbo].[ActivityStatusColumnsVisibility] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnsVisibility_Users];

ALTER TABLE [dbo].[LeaguesFans] WITH CHECK CHECK CONSTRAINT [FK_LeaguesFans_Users];

ALTER TABLE [dbo].[NotesRecipients] WITH CHECK CHECK CONSTRAINT [FK_NotesRecipients_Users];

ALTER TABLE [dbo].[Schools] WITH CHECK CHECK CONSTRAINT [FK_Schools_Users];

ALTER TABLE [dbo].[TennisRank] WITH CHECK CHECK CONSTRAINT [FK_TennisRank_Users];

ALTER TABLE [dbo].[LeagueScheduleState] WITH CHECK CHECK CONSTRAINT [FK_LeagueScheduleState_UserId];

ALTER TABLE [dbo].[UsersDvices] WITH CHECK CHECK CONSTRAINT [FK_UsersDvices_Users];

ALTER TABLE [dbo].[WallThreads] WITH CHECK CHECK CONSTRAINT [FK_WallThreads_Users];

ALTER TABLE [dbo].[PlayerDisciplines] WITH CHECK CHECK CONSTRAINT [FK_PlayerDisciplines_Users];

ALTER TABLE [dbo].[Messages] WITH CHECK CHECK CONSTRAINT [FK_Messages_Users];

ALTER TABLE [dbo].[TeamsRanks] WITH CHECK CHECK CONSTRAINT [FK_TeamsRanks_Users];

ALTER TABLE [dbo].[TeamsRoutes] WITH CHECK CHECK CONSTRAINT [FK_TeamsRoutes_Users];

ALTER TABLE [dbo].[AdditionalTeamGymnastics] WITH CHECK CHECK CONSTRAINT [FK_Users_AdditionalTeamGymnastic];

ALTER TABLE [dbo].[MedicalCertApprovements] WITH CHECK CHECK CONSTRAINT [FK_MedicalCertApprovements_Users];

ALTER TABLE [dbo].[ActivityBranchesState] WITH CHECK CHECK CONSTRAINT [FK_ActivityBranchesState_Users];

ALTER TABLE [dbo].[PlayerAchievements] WITH CHECK CHECK CONSTRAINT [FK_PlayerAchievements_Users];

ALTER TABLE [dbo].[ClubGamesFans] WITH CHECK CHECK CONSTRAINT [FK_ClubGamesFans_Users];

ALTER TABLE [dbo].[TennisLeagueGames] WITH CHECK CHECK CONSTRAINT [FK_TennisLeagueGame_HomePLayer];

ALTER TABLE [dbo].[TennisLeagueGames] WITH CHECK CHECK CONSTRAINT [FK_TennisLeagueGame_GuestPlayer];

ALTER TABLE [dbo].[TennisLeagueGames] WITH CHECK CHECK CONSTRAINT [FK_TennisLeagueGame_TechnicalWinner];

ALTER TABLE [dbo].[TennisLeagueGames] WITH CHECK CHECK CONSTRAINT [FK_TennisLeagueGame_HomePairPlayer];

ALTER TABLE [dbo].[TennisLeagueGames] WITH CHECK CHECK CONSTRAINT [FK_TennisLeagueGame_GuestPairPlayer];

ALTER TABLE [dbo].[UsersFriends] WITH CHECK CHECK CONSTRAINT [FK_UsersFriends_Users];

ALTER TABLE [dbo].[UsersFriends] WITH CHECK CHECK CONSTRAINT [FK_UsersFriends_Friends];

ALTER TABLE [dbo].[TeamsFans] WITH CHECK CHECK CONSTRAINT [FK_TeamsFans_Users];

ALTER TABLE [dbo].[InitialApprovalDates] WITH CHECK CHECK CONSTRAINT [FK_InitialApprovalDates_Users];

ALTER TABLE [dbo].[GamesFans] WITH CHECK CHECK CONSTRAINT [FK_GamesFans_Users];

ALTER TABLE [dbo].[UsersApprovalDatesHistory] WITH CHECK CHECK CONSTRAINT [FK_UsersApprovalDatesHistory_ActionUserId];

ALTER TABLE [dbo].[UsersApprovalDatesHistory] WITH CHECK CHECK CONSTRAINT [FK_UsersApprovalDatesHistory_Users];

ALTER TABLE [dbo].[TeamsPlayers] WITH CHECK CHECK CONSTRAINT [FK_TeamPlayers_Users];

ALTER TABLE [dbo].[TeamsPlayers] WITH CHECK CHECK CONSTRAINT [FK_TeamsPlayers_ActionUserId];

ALTER TABLE [dbo].[TeamsPlayers] WITH CHECK CHECK CONSTRAINT [FK_TeamsPlayers_Users];

ALTER TABLE [dbo].[RefereeSalaryReports] WITH CHECK CHECK CONSTRAINT [FK_RefereeSalaryReports_Users];

ALTER TABLE [dbo].[CompetitionRegistrations] WITH CHECK CHECK CONSTRAINT [FK_Users_CompetitionRegistration];

ALTER TABLE [dbo].[PlayerDiscounts] WITH CHECK CHECK CONSTRAINT [FK_PlayerDiscounts_PlayerDiscounts];

ALTER TABLE [dbo].[PlayerDiscounts] WITH CHECK CHECK CONSTRAINT [FK_PlayerDiscounts_UpdatedDiscounts];

ALTER TABLE [dbo].[PlayersBenefactorPrices] WITH CHECK CHECK CONSTRAINT [FK_PlayersBenefactorPrices_Users];

ALTER TABLE [dbo].[PlayersBlockade] WITH CHECK CHECK CONSTRAINT [FK_PlayersBlockade_ActionUserId];

ALTER TABLE [dbo].[PlayersBlockade] WITH CHECK CHECK CONSTRAINT [FK_UsersBlockade];

ALTER TABLE [dbo].[ActivitiesUsers] WITH CHECK CHECK CONSTRAINT [FK_ActivitiesUsers_Users];

ALTER TABLE [dbo].[CompetitionDisciplineRegistrations] WITH CHECK CHECK CONSTRAINT [FK_CompetitionDisciplineRegistrations_UserId];

ALTER TABLE [dbo].[AdditionalGymnastics] WITH CHECK CHECK CONSTRAINT [FK_Users_AdditionalGymnastic];

ALTER TABLE [dbo].[ClubBalances] WITH CHECK CHECK CONSTRAINT [FK_ClubBalances_ActionUserId];

ALTER TABLE [dbo].[BlockadeNotifications] WITH CHECK CHECK CONSTRAINT [FK_ManagerIdValue];

ALTER TABLE [dbo].[ActivityStatusColumnsOrder] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnsOrder_Users];

ALTER TABLE [dbo].[PlayerFiles] WITH CHECK CHECK CONSTRAINT [FK_PlayerFiles_Users];

ALTER TABLE [dbo].[RetirementRequests] WITH CHECK CHECK CONSTRAINT [FK_RetirementRequests_RequestUser];

ALTER TABLE [dbo].[RetirementRequests] WITH CHECK CHECK CONSTRAINT [FK_RetirementRequests_ApproveUser];

ALTER TABLE [dbo].[PlayerHistory] WITH CHECK CHECK CONSTRAINT [FK_PlayerHistory_Users];

ALTER TABLE [dbo].[PlayerHistory] WITH CHECK CHECK CONSTRAINT [FK_PlayerHistory_ActionUserId];


GO
CREATE TABLE [#__checkStatus] (
    id           INT            IDENTITY (1, 1) PRIMARY KEY CLUSTERED,
    [Schema]     NVARCHAR (256),
    [Table]      NVARCHAR (256),
    [Constraint] NVARCHAR (256)
);

SET NOCOUNT ON;

DECLARE tableconstraintnames CURSOR LOCAL FORWARD_ONLY
    FOR SELECT SCHEMA_NAME([schema_id]),
               OBJECT_NAME([parent_object_id]),
               [name],
               0
        FROM   [sys].[objects]
        WHERE  [parent_object_id] IN (OBJECT_ID(N'dbo.PenaltyForExclusion'), OBJECT_ID(N'dbo.Users'))
               AND [type] IN (N'F', N'C')
                   AND [object_id] IN (SELECT [object_id]
                                       FROM   [sys].[check_constraints]
                                       WHERE  [is_not_trusted] <> 0
                                              AND [is_disabled] = 0
                                       UNION
                                       SELECT [object_id]
                                       FROM   [sys].[foreign_keys]
                                       WHERE  [is_not_trusted] <> 0
                                              AND [is_disabled] = 0);

DECLARE @schemaname AS NVARCHAR (256);

DECLARE @tablename AS NVARCHAR (256);

DECLARE @checkname AS NVARCHAR (256);

DECLARE @is_not_trusted AS INT;

DECLARE @statement AS NVARCHAR (1024);

BEGIN TRY
    OPEN tableconstraintnames;
    FETCH tableconstraintnames INTO @schemaname, @tablename, @checkname, @is_not_trusted;
    WHILE @@fetch_status = 0
        BEGIN
            PRINT N'Checking constraint: ' + @checkname + N' [' + @schemaname + N'].[' + @tablename + N']';
            SET @statement = N'ALTER TABLE [' + @schemaname + N'].[' + @tablename + N'] WITH ' + CASE @is_not_trusted WHEN 0 THEN N'CHECK' ELSE N'NOCHECK' END + N' CHECK CONSTRAINT [' + @checkname + N']';
            BEGIN TRY
                EXECUTE [sp_executesql] @statement;
            END TRY
            BEGIN CATCH
                INSERT  [#__checkStatus] ([Schema], [Table], [Constraint])
                VALUES                  (@schemaname, @tablename, @checkname);
            END CATCH
            FETCH tableconstraintnames INTO @schemaname, @tablename, @checkname, @is_not_trusted;
        END
END TRY
BEGIN CATCH
    PRINT ERROR_MESSAGE();
END CATCH

IF CURSOR_STATUS(N'LOCAL', N'tableconstraintnames') >= 0
    CLOSE tableconstraintnames;

IF CURSOR_STATUS(N'LOCAL', N'tableconstraintnames') = -1
    DEALLOCATE tableconstraintnames;

SELECT N'Constraint verification failed:' + [Schema] + N'.' + [Table] + N',' + [Constraint]
FROM   [#__checkStatus];

IF @@ROWCOUNT > 0
    BEGIN
        DROP TABLE [#__checkStatus];
        RAISERROR (N'An error occurred while verifying constraints', 16, 127);
    END

SET NOCOUNT OFF;

DROP TABLE [#__checkStatus];


GO
PRINT N'Update complete.';


GO
