ALTER TABLE [dbo].[Seasons] ADD [SeasonForAge] Int NULL;
Update [LogLig].[dbo].[Seasons] SET SeasonForAge=2020 Where UnionId in (62,64) and IsActive =1