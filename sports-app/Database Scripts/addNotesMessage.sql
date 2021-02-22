ALTER TABLE [dbo].[NotesMessages] ADD [img]  varchar(500) NULL
GO

ALTER TABLE [dbo].[NotesMessages] ADD [video]  varchar(500) NULL
GO

ALTER TABLE [dbo].[NotesMessages] ADD [parent]  int NULL
GO

ALTER TABLE [dbo].[NotesMessages] ADD [Sender]  int NULL
GO


ALTER TABLE [dbo].[NotesMessages]  WITH CHECK ADD  CONSTRAINT [FK_NotesMessages_NotesMessages] FOREIGN KEY([parent])
REFERENCES [dbo].[NotesMessages] ([MsgId])
GO

ALTER TABLE [dbo].[NotesMessages] CHECK CONSTRAINT [FK_NotesMessages_NotesMessages]
GO

ALTER TABLE [dbo].[NotesMessages]  WITH CHECK ADD  CONSTRAINT [FK_NotesMessages_Users] FOREIGN KEY([Sender])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[NotesMessages] CHECK CONSTRAINT [FK_NotesMessages_Users]
GO

