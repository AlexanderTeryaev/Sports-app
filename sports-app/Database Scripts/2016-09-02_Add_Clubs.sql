USE [LogLig]
GO

CREATE TABLE [dbo].[Clubs](
	[ClubId] [int] IDENTITY(1,1) NOT NULL,
	[SectionId] [int] NOT NULL CONSTRAINT [DF_Clubs_SectionId]  DEFAULT ((1)),
	[Name] [nvarchar](50) NULL,
	[IsArchive] [bit] NOT NULL,
	[Logo] [nvarchar](50) NULL,
	[PrimaryImage] [nvarchar](50) NULL,
	[Description] [nvarchar](250) NULL,
	[IndexImage] [nvarchar](50) NULL,
	[IndexAbout] [nvarchar](2000) NULL,
	[TermsCondition] [nvarchar](50) NULL,
	[ContactPhone] [nvarchar](20) NULL,
	[Address] [nvarchar](250) NULL,
	[Email] [nvarchar](50) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_Club] PRIMARY KEY CLUSTERED 
(
	[ClubId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Clubs]  WITH CHECK ADD  CONSTRAINT [FK_Clubs_Sections] FOREIGN KEY([SectionId])
REFERENCES [dbo].[Sections] ([SectionId])
GO

ALTER TABLE [dbo].[Clubs] CHECK CONSTRAINT [FK_Clubs_Sections]
GO

CREATE TABLE [dbo].[ClubTeams](
	[ClubId] [int] NOT NULL,
	[TeamId] [int] NOT NULL,
	[SeasonId] [int] NULL,
 CONSTRAINT [PK_ClubTeams] PRIMARY KEY CLUSTERED 
(
	[ClubId] ASC,
	[TeamId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ClubTeams]  WITH CHECK ADD  CONSTRAINT [FK_ClubTeams_Clubs] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ClubTeams] CHECK CONSTRAINT [FK_ClubTeams_Clubs]
GO

ALTER TABLE [dbo].[ClubTeams]  WITH CHECK ADD  CONSTRAINT [FK_ClubTeams_Teams] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Teams] ([TeamId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ClubTeams] CHECK CONSTRAINT [FK_ClubTeams_Teams]
GO

ALTER TABLE [dbo].[ClubTeams]  WITH CHECK ADD  CONSTRAINT [FK_ClubTeams_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[ClubTeams] CHECK CONSTRAINT [FK_ClubTeams_Seasons]
GO