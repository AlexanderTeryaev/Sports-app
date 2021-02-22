ALTER TABLE [dbo].[Clubs]
    ADD [ReportRemoveTravelDistance] BIT CONSTRAINT [DF_Clubs_ReportRemoveTravelDistance] DEFAULT ((0)) NOT NULL;
ALTER TABLE [dbo].[Unions]
    ADD [ReportRemoveTravelDistance] BIT CONSTRAINT [DF_Unions_ReportRemoveTravelDistance] DEFAULT ((0)) NOT NULL;