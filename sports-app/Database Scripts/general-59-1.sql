Alter table [dbo].[CompetitionAges]
Add from_age int null,
  to_age int null


Alter table [dbo].[TeamsPlayers]
Add 
	FriendshipTypeId int null,
	FriendshipPriceType int null, 
	RoadDisciplineId int null, 
	MountaintDisciplineId int null


ALTER TABLE [dbo].[TeamsPlayers] ADD CONSTRAINT [FKEY_TPLAYERS_FRIEND] FOREIGN KEY (FriendshipTypeId) REFERENCES [dbo].[FriendshipsTypes] ([FriendshipsTypesId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[TeamsPlayers] ADD CONSTRAINT [FKEY_TPLAYERSROAD_FRIEND] FOREIGN KEY ([RoadDisciplineId]) REFERENCES [dbo].[Disciplines] ([DisciplineId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[TeamsPlayers] ADD CONSTRAINT [FKEY_TPLAYERSMOUNT_FRIEND] FOREIGN KEY ([MountaintDisciplineId]) REFERENCES [dbo].[Disciplines] ([DisciplineId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO