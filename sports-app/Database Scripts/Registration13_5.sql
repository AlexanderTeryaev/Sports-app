ALTER TABLE [dbo].[Activities]
    ADD [UnionApprovedPlayerDiscount]    DECIMAL (18, 2) NULL,
        [UnionApprovedPlayerNoInsurance] BIT             CONSTRAINT [DF_Activities_UnionApprovedPlayerNoInsurance] DEFAULT ((1)) NOT NULL,
        [EscortDiscount]                 DECIMAL (18, 2) NULL,
        [EscortNoInsurance]              BIT             CONSTRAINT [DF_Activities_EscortNoInsurance] DEFAULT ((1)) NOT NULL,
        [AllowEscortRegistration]        BIT             CONSTRAINT [DF_Activities_AllowEscortRegistration] DEFAULT ((0)) NOT NULL,
        [RestrictCustomPricesToOneItem]  BIT             CONSTRAINT [DF_Activities_RestrictCustomPricesToOneItem] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[TeamsPlayers]
    ADD [IsEscortPlayer] BIT CONSTRAINT [DF_TeamsPlayers_IsEscortPlayer] DEFAULT ((0)) NOT NULL;