ALTER TABLE [dbo].[Activities]
    ADD [RestrictLeagues]         BIT            CONSTRAINT [DF_Activities_RestrictLeagues] DEFAULT ((0)) NOT NULL,
        [RestrictedLeagues]       NVARCHAR (MAX) NULL,
        [RegistrationPrice]       BIT            CONSTRAINT [DF_Activities_RegistrationPrice] DEFAULT ((0)) NOT NULL,
        [InsurancePrice]          BIT            CONSTRAINT [DF_Activities_InsurancePrice] DEFAULT ((0)) NOT NULL,
        [MembersFee]              BIT            CONSTRAINT [DF_Activities_MembersFee] DEFAULT ((0)) NOT NULL,
        [AllowNoInsurancePayment] BIT            CONSTRAINT [DF_Activities_AllowNoInsurancePayment] DEFAULT ((0)) NOT NULL,
        [AllowNoFeePayment]       BIT            CONSTRAINT [DF_Activities_AllowNoFeePayment] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [MembersFee]               DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_MembersFee] DEFAULT ((0)) NOT NULL,
        [MembersFeePaid]           DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_MembersFeePaid] DEFAULT ((0)) NOT NULL,
        [DisableMembersFeePayment] BIT             CONSTRAINT [DF_ActivityFormsSubmittedData_DisableMembersFeePayment] DEFAULT ((0)) NOT NULL,
        [DisableInsurancePayment]  BIT             CONSTRAINT [DF_ActivityFormsSubmittedData_DisableInsurancePayment] DEFAULT ((0)) NOT NULL;

CREATE TABLE [dbo].[MemberFees] (
    [Id]        INT             IDENTITY (1, 1) NOT NULL,
    [LeagueId]  INT             NULL,
    [StartDate] DATETIME        NOT NULL,
    [EndDate]   DATETIME        NOT NULL,
    [Amount]    DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_MemberFees] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[MemberFees] WITH NOCHECK
    ADD CONSTRAINT [FK_MemberFees_Leagues] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[MemberFees] WITH CHECK CHECK CONSTRAINT [FK_MemberFees_Leagues];