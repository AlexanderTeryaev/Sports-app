ALTER TABLE [dbo].[ActivityFormsSubmittedData] DROP COLUMN [TeamsRegistrationPrice];

ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [CardComLpc]              UNIQUEIDENTIFIER NULL,
        [CardComPaymentCompleted] BIT              CONSTRAINT [DF_ActivityFormsSubmittedData_CardComPaymentCompleted] DEFAULT ((0)) NOT NULL,
        [CardComPaymentDate]      DATETIME         NULL;