ALTER TABLE [dbo].[Activities]
    ADD [HandlingFee]               BIT CONSTRAINT [DF_Activities_HandlingFee] DEFAULT ((0)) NOT NULL,
        [AllowNoHandlingFeePayment] BIT CONSTRAINT [DF_Activities_AllowNoHandlingFeePayment] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [HandlingFee]               DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_HandlingFee] DEFAULT ((0)) NOT NULL,
        [HandlingFeePaid]           DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_HandlingFeePaid] DEFAULT ((0)) NOT NULL,
        [DisableHandlingFeePayment] BIT             CONSTRAINT [DF_ActivityFormsSubmittedData_DisableHandlingFeePayment] DEFAULT ((0)) NOT NULL;

CREATE TABLE [dbo].[HandlingFees] (
    [Id]        INT             IDENTITY (1, 1) NOT NULL,
    [LeagueId]  INT             NULL,
    [StartDate] DATETIME        NOT NULL,
    [EndDate]   DATETIME        NOT NULL,
    [Amount]    DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_HandlingFees] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[HandlingFees] WITH NOCHECK
    ADD CONSTRAINT [FK_HandlingFees_Leagues] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[HandlingFees] WITH CHECK CHECK CONSTRAINT [FK_HandlingFees_Leagues];