ALTER TABLE [dbo].[ActivityForms]
    ADD [Name]        NVARCHAR (MAX) CONSTRAINT [DF_ActivityForms_Name] DEFAULT ('') NOT NULL,
        [Description] NVARCHAR (MAX) NULL;