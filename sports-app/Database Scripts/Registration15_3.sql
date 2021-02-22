ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [DoNotPayTenicardPrice] BIT CONSTRAINT [DF_ActivityFormsSubmittedData_DoNotPayTenicardPrice] DEFAULT ((0)) NOT NULL;