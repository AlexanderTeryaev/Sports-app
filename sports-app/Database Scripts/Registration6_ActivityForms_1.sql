CREATE TABLE [dbo].[ActivityForms] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [ActivityId]    INT            NOT NULL,
    [IsPublished]   BIT            NOT NULL,
    [PublishedBy]   INT            NULL,
    [DatePublished] DATETIME       NULL,
    [DateCreated]   DATETIME       NOT NULL,
    [DateUpdated]   DATETIME       NOT NULL,
    [UpdatedBy]     INT            NOT NULL,
    [ImageFile]     NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_ActivityForms] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[ActivityFormsDetails] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [FormId]        INT            NOT NULL,
    [PropertyName]  NVARCHAR (MAX) NOT NULL,
    [Type]          NVARCHAR (MAX) NOT NULL,
    [LabelTextEn]   NVARCHAR (MAX) NOT NULL,
    [LabelTextHeb]  NVARCHAR (MAX) NOT NULL,
    [IsDisabled]    BIT            NOT NULL,
    [IsRequired]    BIT            NOT NULL,
    [IsReadOnly]    BIT            NOT NULL,
    [CanBeRequired] BIT            NOT NULL,
    [CanBeDisabled] BIT            NOT NULL,
    [FieldNote]     NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_ActivityFormsDetails] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[ActivityFormsSubmittedData] (
    [Id]                     INT            IDENTITY (1, 1) NOT NULL,
    [ActivityId]             INT            NOT NULL,
    [PlayerId]               INT            NOT NULL,
    [PlayerFullName]         NVARCHAR (MAX) NOT NULL,
    [PlayerEmail]            NVARCHAR (MAX) NOT NULL,
    [PlayerPhone]            NVARCHAR (MAX) NULL,
    [PlayerAddress]          NVARCHAR (MAX) NULL,
    [PlayerBirthDate]        DATETIME       NULL,
    [PlayerTeam]             NVARCHAR (MAX) NOT NULL,
    [PlayerLeague]           NVARCHAR (MAX) NOT NULL,
    [NameForInvoice]         NVARCHAR (MAX) NULL,
    [PaymentByBenefactor]    NVARCHAR (MAX) NULL,
    [Document]               NVARCHAR (MAX) NULL,
    [MedicalCert]            NVARCHAR (MAX) NULL,
    [InsuranceCert]          NVARCHAR (MAX) NULL,
    [Comments]               NVARCHAR (MAX) NULL,
    [NeedShirts]             BIT            NOT NULL,
    [SelfInsurance]          BIT            NOT NULL,
    [TeamsRegistrationPrice] INT            NOT NULL,
    CONSTRAINT [PK_ActivityFormsSubmittedData] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[ActivityForms] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityForms_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]);

ALTER TABLE [dbo].[ActivityForms] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityForms_PublishedByUsers] FOREIGN KEY ([PublishedBy]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[ActivityForms] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityForms_UpdateUsers] FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[ActivityFormsDetails] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsDetails_ActivityForms] FOREIGN KEY ([FormId]) REFERENCES [dbo].[ActivityForms] ([Id]);

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]);

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_Users] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[ActivityForms] WITH CHECK CHECK CONSTRAINT [FK_ActivityForms_Activities];

ALTER TABLE [dbo].[ActivityForms] WITH CHECK CHECK CONSTRAINT [FK_ActivityForms_PublishedByUsers];

ALTER TABLE [dbo].[ActivityForms] WITH CHECK CHECK CONSTRAINT [FK_ActivityForms_UpdateUsers];

ALTER TABLE [dbo].[ActivityFormsDetails] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsDetails_ActivityForms];

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_Activities];

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_Users];