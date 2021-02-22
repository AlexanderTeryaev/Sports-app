ALTER TABLE [dbo].[Activities]
    ADD [NoPriceOnBuiltInRegistration] BIT CONSTRAINT [DF_Activities_NoPriceOnBuiltInRegistration] DEFAULT ((0)) NOT NULL;