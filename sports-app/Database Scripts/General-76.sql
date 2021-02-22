Alter table [dbo].[TeamsPlayers]
Add TeamForUci nvarchar(MAX)

Alter table [dbo].[Unions]
Add
	ForeignAddress nvarchar(MAX),
	UnionWebSite nvarchar(MAX)

Alter table [dbo].[CompetitionAges]
Add 
	age_foreign_name nvarchar(MAX)

Alter table [dbo].[Users]
Add 
	[HeatTypeForUciCard] int null

Alter table [dbo].[UsersJobs]
Add
	[Function] nvarchar(MAX) null