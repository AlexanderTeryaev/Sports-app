ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [RegistrationPrice] DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_RegistrationPrice] DEFAULT ((0)) NOT NULL,
        [InsurancePrice]    DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_InsurancePrice] DEFAULT ((0)) NOT NULL;

CREATE TABLE [dbo].[PlayerDiscounts] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [PlayerId]     INT             NOT NULL,
    [TeamId]       INT             NOT NULL,
    [LeagueId]     INT             NOT NULL,
    [SeasonId]     INT             NOT NULL,
    [DiscountType] INT             NOT NULL,
    [Amount]       DECIMAL (18, 2) NOT NULL,
    [UpdateUserId] INT             NOT NULL,
    [DateUpdated]  DATETIME        NOT NULL,
    CONSTRAINT [PK_PlayerDiscounts] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[PlayersBenefactorPrices] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [PlayerId]          INT             NOT NULL,
    [TeamId]            INT             NOT NULL,
    [LeagueId]          INT             NOT NULL,
    [SeasonId]          INT             NOT NULL,
    [BenefactorId]      INT             NOT NULL,
    [RegistrationPrice] DECIMAL (18, 2) NOT NULL,
    [InsurancePrice]    DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_PlayersBenefactorPrices] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[PlayerDiscounts] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDiscounts_PlayerDiscounts] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[PlayerDiscounts] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDiscounts_Teams] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]);

ALTER TABLE [dbo].[PlayerDiscounts] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDiscounts_Leagues] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[PlayerDiscounts] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDiscounts_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[PlayerDiscounts] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerDiscounts_UpdatedDiscounts] FOREIGN KEY ([UpdateUserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[PlayersBenefactorPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayersBenefactorPrices_Users] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[PlayersBenefactorPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayersBenefactorPrices_Teams] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]);

ALTER TABLE [dbo].[PlayersBenefactorPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayersBenefactorPrices_Leagues] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[PlayersBenefactorPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayersBenefactorPrices_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[PlayersBenefactorPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayersBenefactorPrices_TeamBenefactors] FOREIGN KEY ([BenefactorId]) REFERENCES [dbo].[TeamBenefactors] ([BenefactorId]);


ALTER TABLE [dbo].[PlayerDiscounts] WITH CHECK CHECK CONSTRAINT [FK_PlayerDiscounts_PlayerDiscounts];

ALTER TABLE [dbo].[PlayerDiscounts] WITH CHECK CHECK CONSTRAINT [FK_PlayerDiscounts_Teams];

ALTER TABLE [dbo].[PlayerDiscounts] WITH CHECK CHECK CONSTRAINT [FK_PlayerDiscounts_Leagues];

ALTER TABLE [dbo].[PlayerDiscounts] WITH CHECK CHECK CONSTRAINT [FK_PlayerDiscounts_Seasons];

ALTER TABLE [dbo].[PlayerDiscounts] WITH CHECK CHECK CONSTRAINT [FK_PlayerDiscounts_UpdatedDiscounts];

ALTER TABLE [dbo].[PlayersBenefactorPrices] WITH CHECK CHECK CONSTRAINT [FK_PlayersBenefactorPrices_Users];

ALTER TABLE [dbo].[PlayersBenefactorPrices] WITH CHECK CHECK CONSTRAINT [FK_PlayersBenefactorPrices_Teams];

ALTER TABLE [dbo].[PlayersBenefactorPrices] WITH CHECK CHECK CONSTRAINT [FK_PlayersBenefactorPrices_Leagues];

ALTER TABLE [dbo].[PlayersBenefactorPrices] WITH CHECK CHECK CONSTRAINT [FK_PlayersBenefactorPrices_Seasons];

ALTER TABLE [dbo].[PlayersBenefactorPrices] WITH CHECK CHECK CONSTRAINT [FK_PlayersBenefactorPrices_TeamBenefactors];