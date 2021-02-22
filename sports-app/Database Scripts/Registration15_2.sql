ALTER TABLE [dbo].[Unions]
    ADD [EnablePaymentsForPlayerClubRegistrations] BIT CONSTRAINT [DF_Unions_EnablePaymentsForPlayerClubRegistrations] DEFAULT ((0)) NOT NULL;