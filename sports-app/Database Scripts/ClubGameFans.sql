/****** Object:  Table [dbo].[ClubGamesFans]    Script Date: 2017/12/14 3:42:35 ******/
CREATE TABLE [dbo].[ClubGamesFans](
	[ScrapperId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
 CONSTRAINT [PK_ClubGamesFans] PRIMARY KEY CLUSTERED 
(
	[ScrapperId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ClubGamesFans]  WITH CHECK ADD  CONSTRAINT [FK_ClubGamesFans_TeamScheduleScrapper] FOREIGN KEY([ScrapperId])
REFERENCES [dbo].[TeamScheduleScrapper] ([Id])
GO

ALTER TABLE [dbo].[ClubGamesFans] CHECK CONSTRAINT [FK_ClubGamesFans_TeamScheduleScrapper]
GO

ALTER TABLE [dbo].[ClubGamesFans]  WITH CHECK ADD  CONSTRAINT [FK_ClubGamesFans_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[ClubGamesFans] CHECK CONSTRAINT [FK_ClubGamesFans_Users]
GO

