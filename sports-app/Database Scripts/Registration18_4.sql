ALTER TABLE [dbo].[ActivityCustomPrices]
    ADD [CardComProductId] NVARCHAR (MAX) NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [RegistrationCardComProductId]  NVARCHAR (MAX) NULL,
        [InsuranceCardComProductId]     NVARCHAR (MAX) NULL,
        [ParticipationCardComProductId] NVARCHAR (MAX) NULL,
        [MembersFeeCardComProductId]    NVARCHAR (MAX) NULL,
        [HandlingFeeCardComProductId]   NVARCHAR (MAX) NULL,
        [TenicardCardComProductId]      NVARCHAR (MAX) NULL;

ALTER TABLE [dbo].[ClubTeamPrices]
    ADD [CardComProductId] NVARCHAR (MAX) NULL;

ALTER TABLE [dbo].[HandlingFees]
    ADD [CardComProductId] NVARCHAR (MAX) NULL;

ALTER TABLE [dbo].[LeaguesPrices]
    ADD [CardComProductId] NVARCHAR (MAX) NULL;

ALTER TABLE [dbo].[MemberFees]
    ADD [CardComProductId] NVARCHAR (MAX) NULL;

ALTER TABLE [dbo].[UnionPrices]
    ADD [CardComProductId] NVARCHAR (MAX) NULL;