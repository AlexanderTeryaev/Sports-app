ALTER TABLE TeamsPlayers ADD PersonalCoachId INT NULL
GO

ALTER TABLE [dbo].[TeamsPlayers]  WITH CHECK ADD  CONSTRAINT [FK_TeamPlayers_Users] FOREIGN KEY([PersonalCoachId])
REFERENCES [dbo].[Users] ([UserId])
GO