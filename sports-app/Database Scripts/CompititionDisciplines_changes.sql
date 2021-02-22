DELETE FROM [LogLig].[dbo].[CompetitionDisciplines]; 
DELETE FROM [LogLig].[dbo].[CompetitionDisciplineRegistrations];

ALTER TABLE [LogLig].[dbo].[CompetitionDisciplines] DROP COLUMN Name;  
ALTER TABLE [LogLig].[dbo].[CompetitionDisciplines] DROP COLUMN DisciplineId; 
ALTER TABLE [LogLig].[dbo].[CompetitionDisciplines] DROP COLUMN MinResult;
ALTER TABLE [LogLig].[dbo].[CompetitionDisciplines] ADD DisciplineId INT NOT NULL;  
ALTER TABLE [LogLig].[dbo].[CompetitionDisciplines] ADD MinResult FLOAT NULL;  
ALTER TABLE [LogLig].[dbo].[CompetitionDisciplines] DROP COLUMN MaxSportsmen;  
ALTER TABLE [LogLig].[dbo].[CompetitionDisciplines] ADD MaxSportsmen INT NULL;  
ALTER TABLE [LogLig].[dbo].[CompetitionDisciplines] DROP COLUMN StartTime;
ALTER TABLE [LogLig].[dbo].[CompetitionDisciplines] ADD StartTime DATETIME NULL; 


