Alter table [dbo].[FriendshipPrices]
Add 
	PriceForNew int null


Alter table [dbo].[Users]
Add 
	TotalPrice int null,
	Nationality nvarchar(MAX) null


Alter table [dbo].[Unions]
Add	
	ContentForFriendshipCard nvarchar(MAX) null


Alter table [dbo].[CompetitionAges]
Add 
	IsUCICategory bit null Default(0)

Alter table [dbo].[Clubs]
Add 
	ForeignName nvarchar(MAX) null

Alter table [dbo].[TeamsDetails]
Add 
	TeamForeignName nvarchar(MAX) null