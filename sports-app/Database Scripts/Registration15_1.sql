ALTER TABLE [dbo].[Activities]
    ADD [DisableRegPaymentForExistingClubs] BIT CONSTRAINT [DF_Activities_DisableRegPaymentForExistingClubs] DEFAULT ((0)) NOT NULL,
        [RegisterToTrainingTeamsOnly]       BIT CONSTRAINT [DF_Activities_RegisterToTrainingTeamsOnly] DEFAULT ((0)) NOT NULL,
        [AllowNoCustomPricesSelected]       BIT CONSTRAINT [DF_Activities_AllowNoCustomPricesSelected] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [TenicardPrice]     DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_TenicardPrice] DEFAULT ((0)) NOT NULL,
        [TenicardPaid]      DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_TenicardPaid] DEFAULT ((0)) NOT NULL,
        [IsSchoolInsurance] BIT             CONSTRAINT [DF_ActivityFormsSubmittedData_IsSchoolInsurance] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[Users]
    ADD [MedExamDate] DATETIME NULL;

CREATE TABLE [dbo].[UnionPrices] (
    [Id]        INT             IDENTITY (1, 1) NOT NULL,
    [UnionId]   INT             NOT NULL,
    [SeasonId]  INT             NOT NULL,
    [Price]     DECIMAL (18, 2) NOT NULL,
    [StartDate] DATETIME        NOT NULL,
    [EndDate]   DATETIME        NOT NULL,
    [PriceType] INT             NOT NULL,
    CONSTRAINT [PK_UnionPrices] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[UnionPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_UnionPrices_Unions] FOREIGN KEY ([UnionId]) REFERENCES [dbo].[Unions] ([UnionId]);

ALTER TABLE [dbo].[UnionPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_UnionPrices_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[UnionPrices] WITH CHECK CHECK CONSTRAINT [FK_UnionPrices_Unions];

ALTER TABLE [dbo].[UnionPrices] WITH CHECK CHECK CONSTRAINT [FK_UnionPrices_Seasons];