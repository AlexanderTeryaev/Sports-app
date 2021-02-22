ALTER TABLE [dbo].[AdditionalGymnastics] drop constraint DF__Additiona__IsReg__7E42ABEE
GO
ALTER TABLE [dbo].[AdditionalGymnastics] ALTER COLUMN [IsRegisteredInSecondComposition] INT
GO
ALTER TABLE [dbo].[AdditionalGymnastics] add constraint DF__Additiona__IsReg__7E42ABEE default 0 for [IsRegisteredInSecondComposition]
GO
EXEC sp_rename 'dbo.AdditionalGymnastics.IsRegisteredInSecondComposition', 'CompositionNumber' , 'COLUMN';


ALTER TABLE [dbo].[CompetitionRoutes] Add ThirdComposition int Null
ALTER TABLE [dbo].[CompetitionRoutes] Add FourthComposition int Null
ALTER TABLE [dbo].[CompetitionRoutes] Add FifthComposition int Null
ALTER TABLE [dbo].[CompetitionRoutes] Add SixthComposition int Null
ALTER TABLE [dbo].[CompetitionRoutes] Add SeventhComposition int Null