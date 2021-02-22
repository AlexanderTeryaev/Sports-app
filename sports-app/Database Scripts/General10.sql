ALTER TABLE [dbo].[LeagueOfficialsSettings] ADD RateAPerGame DECIMAL(18,2) NULL
ALTER TABLE [dbo].[LeagueOfficialsSettings] ADD RateBPerGame DECIMAL(18,2) NULL
ALTER TABLE [dbo].[LeagueOfficialsSettings] ADD RateAForTravel DECIMAL(18,2) NULL
ALTER TABLE [dbo].[LeagueOfficialsSettings] ADD RateBForTravel DECIMAL(18,2) NULL

CREATE TABLE [dbo].[UsersEducation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Education] [nvarchar](255) NULL,
	[PlaceOfEducation] [nvarchar](255) NULL,
	[DateOfEdIssue] [datetime] NULL,
	[EducationCert] [nvarchar](max) NULL

 CONSTRAINT [PK_UsersEducation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[UsersEducation]  WITH CHECK ADD  CONSTRAINT [FK_UsersEducation_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO

CREATE TABLE [dbo].[KarateRefereesRanks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RefereeId] [int] NOT NULL,
	[Type] [varchar](255) NULL,
	[Date] [datetime] NULL

 CONSTRAINT [PK_KarateReferees] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[KarateRefereesRanks]  WITH CHECK ADD  CONSTRAINT [FK_KarateRefereesRanks_JobsReferee] FOREIGN KEY([RefereeId])
REFERENCES [dbo].[UsersJobs] ([Id])
GO

ALTER TABLE [dbo].[UsersJobs] ADD RateType VARCHAR(255) NULL
ALTER TABLE [dbo].[UsersJobs] ADD ConnectedClubId INT NULL

ALTER TABLE [dbo].[UsersJobs] WITH CHECK ADD CONSTRAINT [FK_UsersJobs_ConnectedClubId] FOREIGN KEY([ConnectedClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO


ALTER TABLE [dbo].[Users] ADD ForeignName VARCHAR(255) NULL
ALTER TABLE [dbo].[Users] ADD DateOfInsurance DATETIME NULL
ALTER TABLE [dbo].[Users] ADD TenicardValidity DATETIME NULL


CREATE TABLE [dbo].[KarateUnionPayment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UnionId] [int] NOT NULL,
	[SeasonId] [int] NOT NULL,
	[FromNumber] [int] NULL,
	[ToNumber] [int] NULL,
	[Price] [decimal](18,2) NULL

 CONSTRAINT [PK_KarateUnionPayment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[KarateUnionPayment]  WITH CHECK ADD  CONSTRAINT [FK_KarateUnionPayment_UnionId] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
GO

ALTER TABLE [dbo].[KarateUnionPayment]  WITH CHECK ADD  CONSTRAINT [FK_KarateUnionPayment_SeasonId] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[KarateUnionPayment] ADD IsShown BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].[NotesMessages] ADD SentByEmail BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].[NotesRecipients] ADD IsEmailSent BIT NOT NULL DEFAULT 0

CREATE TABLE [dbo].[DisplayedPaymentMessages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PaymentId] [int] NOT NULL,
	[UserId] [int] NOT NULL

 CONSTRAINT [PK_DisplayedPaymentMessages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DisplayedPaymentMessages]  WITH CHECK ADD  CONSTRAINT [FK_DisplayedPaymentMessages_PaymentId] FOREIGN KEY([PaymentId])
REFERENCES [dbo].[KarateUnionPayment] ([Id])
GO

ALTER TABLE [dbo].[DisplayedPaymentMessages]  WITH CHECK ADD  CONSTRAINT [FK_DisplayedPaymentMessages_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO