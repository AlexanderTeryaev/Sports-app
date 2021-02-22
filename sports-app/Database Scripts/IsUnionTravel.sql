ALTER TABLE [LogLig].[dbo].[UsersJobs] ADD IsUnionTravel Bit Not Null Default(0)

ALTER TABLE [LogLig].[dbo].[Unions] ADD SaturdaysTariffFromTime datetime Null
ALTER TABLE [LogLig].[dbo].[Unions] ADD SaturdaysTariffToTime datetime Null