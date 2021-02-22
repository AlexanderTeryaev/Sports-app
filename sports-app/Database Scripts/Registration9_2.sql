
ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [InsurancePaid] DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_InsurancePaid] DEFAULT ((0)) NOT NULL;