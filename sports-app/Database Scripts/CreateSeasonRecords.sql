USE [LogLig]
GO

CREATE TABLE [dbo].[SeasonRecords] (
    [Id]       INT IDENTITY (1, 1) NOT NULL,
    [SeasonId]   INT NOT NULL,
	[DisciplineRecordsId] INT NOT NULL,
    [SeasonRecord] nvarchar(50) NULL
    CONSTRAINT [PK_SeasonRecords] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SeasonRecords]  WITH CHECK ADD  CONSTRAINT [FK_SeasonRecords_Seasons] FOREIGN KEY([SeasonId])
REFERENCES [dbo].[Seasons] ([Id])
GO

ALTER TABLE [dbo].[SeasonRecords] CHECK CONSTRAINT [FK_SeasonRecords_Seasons]
GO

ALTER TABLE [dbo].[SeasonRecords]  WITH CHECK ADD  CONSTRAINT [FK_SeasonRecords_DisciplineRecords] FOREIGN KEY([DisciplineRecordsId])
REFERENCES [dbo].[DisciplineRecords] ([Id])
GO

ALTER TABLE [dbo].[SeasonRecords] CHECK CONSTRAINT [FK_SeasonRecords_DisciplineRecords]
GO

INSERT INTO [dbo].[SeasonRecords] ([SeasonId], [DisciplineRecordsId], [SeasonRecord])
SELECT 1106, dr.Id, dr.SeasonRecord FROM [DisciplineRecords] dr


ALTER TABLE [LogLig].[dbo].[DisciplineRecords] DROP COLUMN SeasonRecord;
GO
