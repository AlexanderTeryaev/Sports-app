ALTER TABLE [dbo].[AdditionalTeamGymnastics] drop constraint DF__Additiona__IsReg__485B9C89
GO
ALTER TABLE [dbo].[AdditionalTeamGymnastics] ALTER COLUMN [IsRegisteredInSecondComposition] INT
GO
ALTER TABLE [dbo].[AdditionalTeamGymnastics] add constraint DF__Additiona__IsReg__485B9C89 default 0 for [IsRegisteredInSecondComposition]
GO
EXEC sp_rename 'dbo.AdditionalTeamGymnastics.IsRegisteredInSecondComposition', 'CompositionNumber' , 'COLUMN';




ALTER TABLE [dbo].[CompetitionRegistrations] ALTER COLUMN [IsRegisteredInSecondComposition] INT NULL
GO
EXEC sp_rename 'dbo.CompetitionRegistrations.IsRegisteredInSecondComposition', 'CompositionNumber' , 'COLUMN';



ALTER TABLE [dbo].[CompetitionTeamRoutes] Add ThirdComposition int Null
ALTER TABLE [dbo].[CompetitionTeamRoutes] Add FourthComposition int Null
ALTER TABLE [dbo].[CompetitionTeamRoutes] Add FifthComposition int Null
ALTER TABLE [dbo].[CompetitionTeamRoutes] Add SixthComposition int Null
ALTER TABLE [dbo].[CompetitionTeamRoutes] Add SeventhComposition int Null