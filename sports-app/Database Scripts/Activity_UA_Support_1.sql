ALTER TABLE [dbo].[ActivityCustomPrices] DROP CONSTRAINT [DF_ActivityCustomPrices_Price];

ALTER TABLE [dbo].[ActivityCustomPrices] DROP CONSTRAINT [DF_ActivityCustomPrices_MaxQuantity];

ALTER TABLE [dbo].[ActivityFormsDetails] DROP CONSTRAINT [DF_ActivityFormsDetails_CanBeRemoved];

ALTER TABLE [dbo].[ActivityFormsDetails] DROP CONSTRAINT [DF_ActivityFormsDetails_HasOptions];

ALTER TABLE [dbo].[ActivityFormsDetails] DROP CONSTRAINT [FK_ActivityFormsDetails_ActivityCustomPrices];

ALTER TABLE [dbo].[ActivityCustomPrices] DROP CONSTRAINT [FK_ActivityCustomPrices_Activities];

ALTER TABLE [dbo].[ActivityFormsDetails] DROP CONSTRAINT [FK_ActivityFormsDetails_ActivityForms];

BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [dbo].[tmp_ms_xx_ActivityCustomPrices] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [ActivityId]       INT             NULL,
    [TitleEng]         NVARCHAR (MAX)  NOT NULL,
    [TitleHeb]         NVARCHAR (MAX)  NOT NULL,
    [TitleUk]          NVARCHAR (MAX)  NULL,
    [Price]            DECIMAL (18, 2) CONSTRAINT [DF_ActivityCustomPrices_Price] DEFAULT ((0)) NOT NULL,
    [MaxQuantity]      INT             CONSTRAINT [DF_ActivityCustomPrices_MaxQuantity] DEFAULT ((1)) NOT NULL,
    [CardComProductId] NVARCHAR (MAX)  NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_ActivityCustomPrices2] PRIMARY KEY CLUSTERED ([Id] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [dbo].[ActivityCustomPrices])
    BEGIN
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_ActivityCustomPrices] ON;
        INSERT INTO [dbo].[tmp_ms_xx_ActivityCustomPrices] ([Id], [ActivityId], [TitleEng], [TitleHeb], [Price], [MaxQuantity], [CardComProductId])
        SELECT   [Id],
                 [ActivityId],
                 [TitleEng],
                 [TitleHeb],
                 [Price],
                 [MaxQuantity],
                 [CardComProductId]
        FROM     [dbo].[ActivityCustomPrices]
        ORDER BY [Id] ASC;
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_ActivityCustomPrices] OFF;
    END

DROP TABLE [dbo].[ActivityCustomPrices];

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_ActivityCustomPrices]', N'ActivityCustomPrices';

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_constraint_PK_ActivityCustomPrices2]', N'PK_ActivityCustomPrices', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [dbo].[tmp_ms_xx_ActivityFormsDetails] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [FormId]               INT            NOT NULL,
    [PropertyName]         NVARCHAR (MAX) NOT NULL,
    [Type]                 NVARCHAR (MAX) NOT NULL,
    [LabelTextEn]          NVARCHAR (MAX) NOT NULL,
    [LabelTextHeb]         NVARCHAR (MAX) NOT NULL,
    [LabelTextUk]          NVARCHAR (MAX) NULL,
    [IsDisabled]           BIT            NOT NULL,
    [IsRequired]           BIT            NOT NULL,
    [IsReadOnly]           BIT            NOT NULL,
    [CanBeRequired]        BIT            NOT NULL,
    [CanBeDisabled]        BIT            NOT NULL,
    [FieldNote]            NVARCHAR (MAX) NULL,
    [CustomDropdownValues] NVARCHAR (MAX) NULL,
    [CanBeRemoved]         BIT            CONSTRAINT [DF_ActivityFormsDetails_CanBeRemoved] DEFAULT ((0)) NOT NULL,
    [HasOptions]           BIT            CONSTRAINT [DF_ActivityFormsDetails_HasOptions] DEFAULT ((0)) NOT NULL,
    [CustomPriceId]        INT            NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_ActivityFormsDetails2] PRIMARY KEY CLUSTERED ([Id] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [dbo].[ActivityFormsDetails])
    BEGIN
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_ActivityFormsDetails] ON;
        INSERT INTO [dbo].[tmp_ms_xx_ActivityFormsDetails] ([Id], [FormId], [PropertyName], [Type], [LabelTextEn], [LabelTextHeb], [IsDisabled], [IsRequired], [IsReadOnly], [CanBeRequired], [CanBeDisabled], [FieldNote], [CustomDropdownValues], [CanBeRemoved], [HasOptions], [CustomPriceId])
        SELECT   [Id],
                 [FormId],
                 [PropertyName],
                 [Type],
                 [LabelTextEn],
                 [LabelTextHeb],
                 [IsDisabled],
                 [IsRequired],
                 [IsReadOnly],
                 [CanBeRequired],
                 [CanBeDisabled],
                 [FieldNote],
                 [CustomDropdownValues],
                 [CanBeRemoved],
                 [HasOptions],
                 [CustomPriceId]
        FROM     [dbo].[ActivityFormsDetails]
        ORDER BY [Id] ASC;
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_ActivityFormsDetails] OFF;
    END

DROP TABLE [dbo].[ActivityFormsDetails];

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_ActivityFormsDetails]', N'ActivityFormsDetails';

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_constraint_PK_ActivityFormsDetails2]', N'PK_ActivityFormsDetails', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

ALTER TABLE [dbo].[ActivityFormsDetails] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsDetails_ActivityCustomPrices] FOREIGN KEY ([CustomPriceId]) REFERENCES [dbo].[ActivityCustomPrices] ([Id]) ON DELETE CASCADE;

ALTER TABLE [dbo].[ActivityCustomPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityCustomPrices_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]) ON DELETE CASCADE;

ALTER TABLE [dbo].[ActivityFormsDetails] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsDetails_ActivityForms] FOREIGN KEY ([FormId]) REFERENCES [dbo].[ActivityForms] ([Id]);

ALTER TABLE [dbo].[ActivityFormsDetails] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsDetails_ActivityCustomPrices];

ALTER TABLE [dbo].[ActivityCustomPrices] WITH CHECK CHECK CONSTRAINT [FK_ActivityCustomPrices_Activities];

ALTER TABLE [dbo].[ActivityFormsDetails] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsDetails_ActivityForms];