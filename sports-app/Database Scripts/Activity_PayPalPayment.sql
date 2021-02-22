ALTER TABLE [dbo].[Activities]
    ADD [PaymentMethod] INT CONSTRAINT [DF_Activities_PaymentMethod] DEFAULT ((0)) NOT NULL;


ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [PayPalLogLigId]  UNIQUEIDENTIFIER NULL,
        [PayPalPaymentId] NVARCHAR (250)   NULL,
        [PayPalPayerId]   NVARCHAR (250)   NULL;