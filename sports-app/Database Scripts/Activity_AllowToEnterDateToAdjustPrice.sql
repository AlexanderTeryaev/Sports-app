ALTER TABLE [dbo].[Activities]
    ADD [AllowToEnterDateToAdjustPrices] BIT CONSTRAINT [DF_Activities_AllowToEnterDateToAdjustPrices] DEFAULT ((0)) NOT NULL;