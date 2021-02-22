ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [Paid]         DECIMAL (18, 2) CONSTRAINT [DF_ActivityFormsSubmittedData_Paid] DEFAULT ((0)) NOT NULL,
        [UnionComment] NVARCHAR (MAX)  NULL;