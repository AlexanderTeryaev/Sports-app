ALTER TABLE Leagues ADD PlaceOfCompetition Varchar(255) NULL

CREATE TABLE [dbo].[CompetitionDisciplines]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] varchar(255) NOT NULl,
	[CompetitionId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL,
	[MaxSportsmen] [int] NOT NULL,
	[MinResult] [float] NOT NULL,

CONSTRAINT [PK_CompetitionDisciplines] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CompetitionDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_CompetitionDisciplines_CompetitionId] FOREIGN KEY (CompetitionId) REFERENCES [dbo].[Leagues] ([LeagueId]);

ALTER TABLE [dbo].[CompetitionDisciplines] WITH NOCHECK
    ADD CONSTRAINT [FK_CompetitionDisciplines_Category] FOREIGN KEY (CategoryId) REFERENCES [dbo].[CompetitionAges] ([id]);

ALTER TABLE [dbo].[CompetitionDisciplines] ADD IsDeleted BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].[Leagues] ADD StartRegistrationDate DATETIME NULL

CREATE TABLE [dbo].[CompetitionDisciplineRegistrations]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[CompetitionDisciplineId] [int] NOT NULL,
	[IsArchive] [bit] NOT NULL DEFAULT 0

CONSTRAINT [PK_CompetitionDisciplineRegistrations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CompetitionDisciplineRegistrations] WITH NOCHECK
    ADD CONSTRAINT [FK_CompetitionDisciplineRegistrations_UserId] FOREIGN KEY (UserId) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[CompetitionDisciplineRegistrations] WITH NOCHECK
    ADD CONSTRAINT [FK_CompetitionDisciplineRegistrations_CompetitionDisciplines] FOREIGN KEY (CompetitionDisciplineId) REFERENCES [dbo].[CompetitionDisciplines] 
	([Id]);

ALTER TABLE CompetitionDisciplineRegistrations ADD ClubId [int]

ALTER TABLE [dbo].[CompetitionDisciplineRegistrations] WITH NOCHECK
    ADD CONSTRAINT [FK_CompetitionDisciplineRegistrations_ClubId] FOREIGN KEY (ClubId) REFERENCES [dbo].[Clubs] 
	([ClubId]);

ALTER TABLE [dbo].[Games] ADD TechWinHomePoints INT NULL

ALTER TABLE [dbo].[Games] ADD TechWinGuestPoints INT NULL

ALTER TABLE TennisLeagueGames ALTER COLUMN HomePlayerId INT NULL
ALTER TABLE TennisLeagueGames ALTER COLUMN GuestPlayerId INT NULL

ALTER TABLE Leagues ALTER COLUMN AgeId INT NULL
ALTER TABLE Leagues ALTER COLUMN GenderId INT NULL


