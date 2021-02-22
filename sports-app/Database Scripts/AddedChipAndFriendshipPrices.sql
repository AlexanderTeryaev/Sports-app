USE [LogLig]
GO

CREATE TABLE [dbo].[ChipPrices](
	[ChipId] [int] IDENTITY(1,1) NOT NULL,
    [UnionId] int NOT NULL,
    [FromAge] INT NULL,
    [ToAge] INT NULL,
	[Price] INT NULL,
    [SeasonId] int  NOT NULL
 CONSTRAINT [PK_ChipPrices] PRIMARY KEY CLUSTERED 
(
	[ChipId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ChipPrices]  WITH CHECK ADD  CONSTRAINT [FK_ChipPrices_Unions] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
ON UPDATE CASCADE
GO

ALTER TABLE [dbo].[ChipPrices]  WITH CHECK ADD  CONSTRAINT [FK_ChipPrices_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
ON UPDATE CASCADE
GO

CREATE TABLE [dbo].[FriendshipPrices](
	[FriendshipPricesId] [int] IDENTITY(1,1) NOT NULL,
    [UnionId] int  NOT NULL,
    [FriendshipsTypeId] int NULL,
    [FromAge] INT NULL,
    [ToAge] INT NULL,
	[GenderId] INT NULL,
    [FriendshipPriceType] INT NULL,
    [Price] INT NULL,
    [SeasonId] int  NOT NULL
 CONSTRAINT [PK_FriendshipPrices] PRIMARY KEY CLUSTERED 
(
	[FriendshipPricesId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[FriendshipPrices]  WITH CHECK ADD  CONSTRAINT [FK_FriendshipPrices_Unions] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
ON UPDATE CASCADE
GO

ALTER TABLE [dbo].[FriendshipPrices]  WITH CHECK ADD  CONSTRAINT [FK_FriendshipPrices_FriendshipsTypes] FOREIGN KEY([FriendshipsTypeId])
REFERENCES [dbo].[FriendshipsTypes] ([FriendshipsTypesId])
GO

ALTER TABLE [dbo].[FriendshipPrices] WITH NOCHECK
    ADD CONSTRAINT [FK_FriendshipPrices_GenderId] FOREIGN KEY (GenderId) REFERENCES [dbo].[Genders] ([GenderId]);
GO

ALTER TABLE [dbo].[FriendshipPrices]  WITH CHECK ADD  CONSTRAINT [FK_FriendshipPrices_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
ON UPDATE CASCADE
GO