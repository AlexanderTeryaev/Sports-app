ALTER TABLE [dbo].[Activities]
    ADD [DisableRegPaymentForExistingClubs] BIT CONSTRAINT [DF_Activities_DisableRegPaymentForExistingClubs] DEFAULT ((0)) NOT NULL;