USE [LogLig]
GO

/****** Object:  Table [dbo].[CompetitionResults]    Script Date: 11/19/2018 11:03:22 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CompetitionResults](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompetitionRegistrationId] [int] NOT NULL,
	[Heat] [nvarchar](50) NULL,
	[Lane] [int] NULL,
	[Result] [nvarchar](50) NULL,
	[Wind] [float] NULL,
	[Rank] [int] NULL,
	[SortValue] [bigint] NULL,
 CONSTRAINT [PK_CompetitionResults] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CompetitionResults]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionResults_CompetitionDisciplineRegistrations] FOREIGN KEY([CompetitionRegistrationId])
REFERENCES [dbo].[CompetitionDisciplineRegistrations] ([Id])
GO

ALTER TABLE [dbo].[CompetitionResults] CHECK CONSTRAINT [FK_CompetitionResults_CompetitionDisciplineRegistrations]
GO


