ALTER TABLE [dbo].[Activities]
    ADD [AdjustRegistrationPriceByDate]  BIT CONSTRAINT [DF_Activities_AdjustRegistrationPriceByDate] DEFAULT ((0)) NOT NULL,
        [AdjustParticipationPriceByDate] BIT CONSTRAINT [DF_Activities_AdjustParticipationPriceByDate] DEFAULT ((0)) NOT NULL,
        [AdjustInsurancePriceByDate]     BIT CONSTRAINT [DF_Activities_AdjustInsurancePriceByDate] DEFAULT ((0)) NOT NULL;