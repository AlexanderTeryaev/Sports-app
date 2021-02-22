ALTER TABLE [dbo].[Users]
    ADD [NoInsurancePayment] BIT CONSTRAINT [DF_Users_NoInsurancePayment] DEFAULT ((0)) NOT NULL;