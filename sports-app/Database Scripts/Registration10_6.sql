ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [ParticipationPaid] DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_ParticipationPaid] DEFAULT ((0)) NOT NULL;