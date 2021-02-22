CREATE TABLE  InitialApprovalDates(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[InitialApprovalDate] [datetime] NOT NULL,
	[UnionId] [int] NOT NULL
 CONSTRAINT [PK_InitialApprovalDates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[InitialApprovalDates]  WITH CHECK ADD  CONSTRAINT [FK_InitialApprovalDates_Unions] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
GO

ALTER TABLE [dbo].[InitialApprovalDates]  WITH CHECK ADD  CONSTRAINT [FK_InitialApprovalDates_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO

ALTER TABLE [dbo].[Ages] ADD [FromAge] INT NULL
ALTER TABLE [dbo].[Ages] ADD [ToAge] INT NULL

ALTER TABLE [dbo].[Users] ADD [FirstName] VARCHAR(255) NULL
ALTER TABLE [dbo].[Users] ADD [LastName] VARCHAR(255) NULL


CREATE TABLE UsersApprovalDatesHistory(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] int NOT NULL,
	[TeamName] varchar(255) NOT NULL,
	[ManagerApprovalDate] [datetime] NOT NULL,
 CONSTRAINT [PK_UsersApprovalDates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[UsersApprovalDatesHistory]  WITH CHECK ADD  CONSTRAINT [FK_UsersApprovalDatesHistory_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO