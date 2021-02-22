ALTER TABLE [dbo].[Activities]
    ADD [ParticipationPrice]  BIT CONSTRAINT [DF_Activities_ParticipationPrice] DEFAULT ((0)) NOT NULL,
        [CustomPricesEnabled] BIT CONSTRAINT [DF_Activities_CustomPricesEnabled] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[ActivityFormsDetails]
    ADD [CustomPriceId] INT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [CustomPrices] NVARCHAR (MAX) NULL;

CREATE TABLE [dbo].[ActivityCustomPrices] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [ActivityId]  INT             NOT NULL,
    [TitleEng]    NVARCHAR (MAX)  NOT NULL,
    [TitleHeb]    NVARCHAR (MAX)  NOT NULL,
    [Price]       DECIMAL (18, 2) NOT NULL,
    [MaxQuantity] INT             NOT NULL,
    CONSTRAINT [PK_ActivityCustomPrices] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[ActivityCustomPrices]
    ADD CONSTRAINT [DF_ActivityCustomPrices_Price] DEFAULT ((0)) FOR [Price];

ALTER TABLE [dbo].[ActivityCustomPrices]
    ADD CONSTRAINT [DF_ActivityCustomPrices_MaxQuantity] DEFAULT ((1)) FOR [MaxQuantity];

ALTER TABLE [dbo].[ActivityCustomPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityCustomPrices_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]) ON DELETE CASCADE;

ALTER TABLE [dbo].[ActivityFormsDetails] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsDetails_ActivityCustomPrices] FOREIGN KEY ([CustomPriceId]) REFERENCES [dbo].[ActivityCustomPrices] ([Id]);

ALTER TABLE [dbo].[ActivityCustomPrices] WITH CHECK CHECK CONSTRAINT [FK_ActivityCustomPrices_Activities];

ALTER TABLE [dbo].[ActivityFormsDetails] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsDetails_ActivityCustomPrices];