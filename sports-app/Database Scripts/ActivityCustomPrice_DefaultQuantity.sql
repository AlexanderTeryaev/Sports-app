ALTER TABLE [dbo].[ActivityCustomPrices]
    ADD [DefaultQuantity] INT CONSTRAINT [DF_ActivityCustomPrices_DefaultQuantity] DEFAULT ((0)) NOT NULL;