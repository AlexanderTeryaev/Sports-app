CREATE TABLE [dbo].[PlayersBlockade] (
	[Id] [INT] IDENTITY(1,1) NOT NULL,
	[UserId] [INT] NOT NULL,
	[StartDate] [DATETIME] NOT NULL,
	[EndDate] [DATETIME] NOT NULL,
	[IsActive] [BIT] NOT NULL DEFAULT 0,
	[SeasonId] [INT],
	CONSTRAINT PK_PlayersBlockade PRIMARY KEY (Id),
	CONSTRAINT FK_UsersBlockade FOREIGN KEY (UserId)
    REFERENCES Users(UserId),
	CONSTRAINT FK_SeasonBlockade FOREIGN KEY (SeasonId)
    REFERENCES Seasons(Id)
)
ALTER TABLE [dbo].[Users]
    ADD BlockadeId INT,
    FOREIGN KEY(BlockadeId) REFERENCES PlayersBlockade(Id);

CREATE TABLE [dbo].[BlockadeNotifications] (
	[Id] [INT] IDENTITY (1,1) NOT NULL,
	[ManagerId] [INT] NOT NULL,
	[BlockadeId] [INT] NOT NULL,
	[IsShown] [BIT] NOT NULL
	CONSTRAINT PK_BlockadeNotification PRIMARY KEY (Id),
	CONSTRAINT FK_ManagerIdValue FOREIGN KEY (ManagerId)
    REFERENCES Users(UserId)
)