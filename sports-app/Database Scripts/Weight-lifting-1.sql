ALTER TABLE [dbo].[CompetitionAges] ALTER COLUMN from_weight DECIMAL(20,1) 
ALTER TABLE [dbo].[CompetitionAges] ALTER COLUMN to_weight DECIMAL(20,1) 

ALTER TABLE [dbo].[CompetitionDisciplineRegistrations] Add IsApproved bit null
ALTER TABLE [dbo].[CompetitionDisciplineRegistrations] Add IsCharged bit null

ALTER TABLE [dbo].[ClubBalances] Add LeagueId int null

