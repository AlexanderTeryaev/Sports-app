USE [LogLig]
GO

CREATE TABLE Disciplines(
	[DisciplineId] [int] IDENTITY(1,1) NOT NULL,
	[UnionId] [int] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[IsArchive] [bit] NOT NULL,
	[Logo] [nvarchar](50) NULL,
	[PrimaryImage] [nvarchar](50) NULL,
	[Description] [nvarchar](250) NULL,
	[IndexImage] [nvarchar](50) NULL,
	[IndexAbout] [nvarchar](2000) NULL,
	[TermsCondition] [nvarchar](50) NULL,
 CONSTRAINT [PK_Disciplines] PRIMARY KEY CLUSTERED 
(
	[DisciplineId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Disciplines]  WITH CHECK ADD  CONSTRAINT [FK_Disciplines_Unions] FOREIGN KEY([UnionId])
REFERENCES [dbo].[Unions] ([UnionId])
GO

INSERT INTO [JobsRoles] VALUES (9, 'Discipline manager', 'disciplinemgr', 5)
GO

ALTER TABLE [dbo].[UsersJobs] ADD DisciplineId [int] NULL
GO
ALTER TABLE [dbo].[UsersJobs]  WITH CHECK ADD  CONSTRAINT [FK_UsersJobs_Disciplines] FOREIGN KEY([DisciplineId])
REFERENCES [dbo].[Disciplines] ([DisciplineId])
GO

-- add docs
CREATE TABLE [dbo].[DisciplinesDocs](
	[DocId] [int] IDENTITY(1,1) NOT NULL,
	[DisciplineId] [int] NOT NULL,
	[FileName] [nvarchar](150) NULL,
	[DocFile] [varbinary](max) NULL,
	[IsArchive] [bit] NOT NULL,
 CONSTRAINT [PK_DisciplinesDocs] PRIMARY KEY CLUSTERED 
(
	[DocId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[DisciplinesDocs]  WITH CHECK ADD  CONSTRAINT [FK_DisciplinesDocs_Disciplines] FOREIGN KEY([DisciplineId])
REFERENCES [dbo].[Disciplines] ([DisciplineId])
GO

ALTER TABLE [dbo].[DisciplinesDocs] CHECK CONSTRAINT [FK_DisciplinesDocs_Disciplines]
GO

-- 
alter table [dbo].[Leagues]
add DisciplineId int null

ALTER TABLE [dbo].[Leagues]  WITH CHECK ADD  CONSTRAINT [FK_Leagues_Disciplines] FOREIGN KEY([DisciplineId])
REFERENCES [dbo].[Disciplines] ([DisciplineId])
GO

alter table [dbo].[SentMessages]
add DisciplineId int null

ALTER TABLE [dbo].[SentMessages]  WITH CHECK ADD  CONSTRAINT [FK_SentMessages_Disciplines] FOREIGN KEY([DisciplineId])
REFERENCES [dbo].[Disciplines] ([DisciplineId])
GO