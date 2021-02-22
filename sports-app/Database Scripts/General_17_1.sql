ALTER TABLE [dbo].[CompetitionAges] ADD gender INT NULL

ALTER TABLE [dbo].[CompetitionAges] WITH NOCHECK
    ADD CONSTRAINT [FK_CompetitionAges_GenderId] FOREIGN KEY (gender) REFERENCES [dbo].[Genders] ([GenderId]);

CREATE TABLE [dbo].[CategoriesPlaceDates]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TeamId] [int] NOT NULL,
	[QualificationStartDate] [DATETIME] NULL,
	[QualificationEndDate] [DATETIME] NULL,
	[FinalStartDate] [DATETIME] NULL,
	[FinalEndDate] [DATETIME] NULL,
CONSTRAINT [PK_CategoriesPlaceDates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CategoriesPlaceDates] WITH NOCHECK
    ADD CONSTRAINT [FK_CategoriesPlaceDates_TeamId] FOREIGN KEY (TeamId) REFERENCES [dbo].[Teams] ([TeamId]);

ALTER TABLE [dbo].[TennisRank] WITH CHECK ADD CONSTRAINT [FK_TennisRank_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])

ALTER TABLE [dbo].[Users] ADD PassportFile VARCHAR(255) NULL 
ALTER TABlE [dbo].[Users] ADD AthleteNumber INT NULL
ALTER TABLE [dbo].[UnionPrices] ADD FromBirthday DATETIME NULL
ALTER TABLE [dbo].[UnionPrices] ADD ToBirthday DATETIME NULL
ALTER TABLE [dbo].[RefereeRegistrations] ADD IsApproved BIT NOT NULL DEFAULT 0