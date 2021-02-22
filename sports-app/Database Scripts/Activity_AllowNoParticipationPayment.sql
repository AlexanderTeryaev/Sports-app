ALTER TABLE [dbo].[Activities]
    ADD [AllowNoParticipationPayment] BIT CONSTRAINT [DF_Activities_AllowNoParticipationPayment] DEFAULT ((0)) NOT NULL;
ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [DisableParticipationPayment] BIT CONSTRAINT [DF_ActivityFormsSubmittedData_DisableParticipationPayment] DEFAULT ((0)) NOT NULL;