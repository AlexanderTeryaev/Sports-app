ALTER TABLE [dbo].[Clubs]
    ADD [NumberOfCourts] INT CONSTRAINT [DF_Clubs_NumberOfCourts] DEFAULT ((0)) NOT NULL;