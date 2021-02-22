ALTER TABLE [dbo].[Activities]
    ADD [PostponeParticipationPayment] BIT CONSTRAINT [DF_Activities_PostponeParticipationPayment] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [PostponeParticipationPayment] BIT CONSTRAINT [DF_ActivityFormsSubmittedData_PostponeParticipationPayment] DEFAULT ((0)) NOT NULL;