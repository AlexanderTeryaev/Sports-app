USE [LogLig]
GO

ALTER TABLE CompetitionAges ADD DisciplineId INT NULL
GO

ALTER TABLE [dbo].[CompetitionAges]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionAges_Disciplines] FOREIGN KEY([DisciplineId])
REFERENCES [dbo].[Disciplines] ([DisciplineId])
GO

ALTER TABLE CompetitionAges ADD FriendshipTypeId INT NULL
GO

ALTER TABLE [dbo].[CompetitionAges]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionAges_FriendshipsTypes] FOREIGN KEY([FriendshipTypeId])
REFERENCES [dbo].[FriendshipsTypes] ([FriendshipsTypesId])
GO