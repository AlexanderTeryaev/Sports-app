ALTER TABLE [dbo].[Activities]
    ADD [AllowNoRegistrationPayment] BIT CONSTRAINT [DF_Activities_AllowNoRegistrationPayment] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [DisableRegistrationPayment] BIT CONSTRAINT [DF_ActivityFormsSubmittedData_DisableRegistrationPayment] DEFAULT ((0)) NOT NULL;