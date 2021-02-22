USE [LogLig]
GO

/****** Object:  Table [dbo].[AdvBanner]    Script Date: 3/27/2018 9:53:39 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AdvBanner](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[count] [int] NOT NULL,
	[linkurl] [text] NOT NULL,
	[image] [text] NOT NULL,
	[leagueId] [int] NULL,
	[clubId] [int] NULL,
 CONSTRAINT [PK_AdvBanner] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[AdvBanner]  WITH CHECK ADD  CONSTRAINT [FK_AdvBanner_Clubs] FOREIGN KEY([clubId])
REFERENCES [dbo].[Clubs] ([ClubId])
ON UPDATE CASCADE
GO

ALTER TABLE [dbo].[AdvBanner] CHECK CONSTRAINT [FK_AdvBanner_Clubs]
GO

ALTER TABLE [dbo].[AdvBanner]  WITH CHECK ADD  CONSTRAINT [FK_AdvBanner_Leagues] FOREIGN KEY([leagueId])
REFERENCES [dbo].[Unions] ([UnionId])
ON UPDATE CASCADE
GO

ALTER TABLE [dbo].[AdvBanner] CHECK CONSTRAINT [FK_AdvBanner_Leagues]
GO

