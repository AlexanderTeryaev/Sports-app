ALTER TABLE [dbo].[ActivityFormsSubmittedData] DROP CONSTRAINT [FK_ActivityFormsSubmittedData_Activities];

ALTER TABLE [dbo].[ActivityFormsSubmittedData] DROP CONSTRAINT [FK_ActivityFormsSubmittedData_Users];

BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [dbo].[tmp_ms_xx_ActivityFormsSubmittedData] (
    [Id]                     INT            IDENTITY (1, 1) NOT NULL,
    [ActivityId]             INT            NOT NULL,
    [PlayerId]               INT            NOT NULL,
    [PlayerFullName]         NVARCHAR (MAX) NOT NULL,
    [PlayerEmail]            NVARCHAR (MAX) NOT NULL,
    [PlayerPhone]            NVARCHAR (MAX) NULL,
    [PlayerAddress]          NVARCHAR (MAX) NULL,
    [PlayerBirthDate]        DATETIME       NULL,
    [TeamId]                 INT            NOT NULL,
    [LeagueId]               INT            NOT NULL,
    [NameForInvoice]         NVARCHAR (MAX) NULL,
    [IsPaymentByBenefactor]  BIT            NOT NULL,
    [PaymentByBenefactor]    NVARCHAR (MAX) NULL,
    [Document]               NVARCHAR (MAX) NULL,
    [MedicalCert]            NVARCHAR (MAX) NULL,
    [InsuranceCert]          NVARCHAR (MAX) NULL,
    [Comments]               NVARCHAR (MAX) NULL,
    [NeedShirts]             BIT            NOT NULL,
    [SelfInsurance]          BIT            NOT NULL,
    [TeamsRegistrationPrice] INT            NOT NULL,
    [DateSubmitted]          DATETIME       NOT NULL,
    [IsActive]               BIT            CONSTRAINT [DF_ActivityFormsSubmittedData_IsActive] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_ActivityFormsSubmittedData1] PRIMARY KEY CLUSTERED ([Id] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [dbo].[ActivityFormsSubmittedData])
    BEGIN
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_ActivityFormsSubmittedData] ON;
        INSERT INTO [dbo].[tmp_ms_xx_ActivityFormsSubmittedData] ([Id], [ActivityId], [PlayerId], [PlayerFullName], [PlayerEmail], [PlayerPhone], [PlayerAddress], [PlayerBirthDate], [NameForInvoice], [PaymentByBenefactor], [Document], [MedicalCert], [InsuranceCert], [Comments], [NeedShirts], [SelfInsurance], [TeamsRegistrationPrice])
        SELECT   [Id],
                 [ActivityId],
                 [PlayerId],
                 [PlayerFullName],
                 [PlayerEmail],
                 [PlayerPhone],
                 [PlayerAddress],
                 [PlayerBirthDate],
                 [NameForInvoice],
                 [PaymentByBenefactor],
                 [Document],
                 [MedicalCert],
                 [InsuranceCert],
                 [Comments],
                 [NeedShirts],
                 [SelfInsurance],
                 [TeamsRegistrationPrice]
        FROM     [dbo].[ActivityFormsSubmittedData]
        ORDER BY [Id] ASC;
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_ActivityFormsSubmittedData] OFF;
    END

DROP TABLE [dbo].[ActivityFormsSubmittedData];

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_ActivityFormsSubmittedData]', N'ActivityFormsSubmittedData';

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_constraint_PK_ActivityFormsSubmittedData1]', N'PK_ActivityFormsSubmittedData', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]);

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_Users] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);


ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_Teams] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]);

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_Leagues] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_Activities];

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_Users];

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_Teams];

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_Leagues];
