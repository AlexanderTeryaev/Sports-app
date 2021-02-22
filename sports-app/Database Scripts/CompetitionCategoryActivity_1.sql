ALTER TABLE [dbo].[Activities]
    ADD [RegistrationsByCompetitionsCategory] BIT CONSTRAINT [DF_Activities_RegistrationsByCompetitionsCategory] DEFAULT ((0)) NOT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [CompetitionDisciplineId] INT NULL;

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_CompetitionDisciplines] FOREIGN KEY ([CompetitionDisciplineId]) REFERENCES [dbo].[CompetitionDisciplines] ([Id]);

ALTER TABLE [dbo].[ActivityFormsSubmittedData] WITH CHECK CHECK CONSTRAINT [FK_ActivityFormsSubmittedData_CompetitionDisciplines];