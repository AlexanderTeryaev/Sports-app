USE [LogLig]
GO

/****** Object:  Table [dbo].[CompetitionHeatWinds]    Script Date: 12/20/2018 3:07:21 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CompetitionHeatWinds](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DisciplineCompetitionId] [int] NOT NULL,
	[Heat] [varchar](50) NOT NULL,
	[Wind] [float] NULL,
 CONSTRAINT [PK_CompetitionHeatWinds] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CompetitionHeatWinds]  WITH CHECK ADD  CONSTRAINT [FK_CompetitionHeatWinds_CompetitionDisciplines] FOREIGN KEY([DisciplineCompetitionId])
REFERENCES [dbo].[CompetitionDisciplines] ([Id])
GO

ALTER TABLE [dbo].[CompetitionHeatWinds] CHECK CONSTRAINT [FK_CompetitionHeatWinds_CompetitionDisciplines]
GO


