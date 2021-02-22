USE [LogLig]
GO

ALTER TABLE [dbo].[Disciplines] Add [RoadHeat] BIT NULL;
ALTER TABLE [dbo].[Disciplines] Add [MountainHeat] BIT NULL;
GO

CREATE TABLE [dbo].[FriendshipsTypes](
	[FriendshipsTypesId] [int] IDENTITY(1,1) NOT NULL,
    [UnionId] int  NOT NULL,
	[Name] [nvarchar](250) NULL,
    [IsArchive] [bit] NOT NULL,
    [SeasonId] int  NOT NULL
 CONSTRAINT [PK_FriendshipsTypes] PRIMARY KEY CLUSTERED 
(
	[FriendshipsTypesId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[FriendshipsTypes]  WITH CHECK ADD  CONSTRAINT [FK_FriendshipsTypes_Unions] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
ON UPDATE CASCADE
GO

ALTER TABLE [dbo].[FriendshipsTypes]  WITH CHECK ADD  CONSTRAINT [FK_FriendshipsTypes_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
ON UPDATE CASCADE
GO