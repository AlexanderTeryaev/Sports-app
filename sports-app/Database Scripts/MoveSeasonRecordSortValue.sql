USE [LogLig]
GO

ALTER TABLE [dbo].[SeasonRecords] ADD SeasonRecordSortValue bigint NULL
GO

UPDATE [dbo].[SeasonRecords] set [SeasonRecordSortValue] = (SELECT dr.SeasonRecordSortValue FROM [DisciplineRecords] dr
where SeasonRecords.DisciplineRecordsId = dr.Id)
GO

ALTER TABLE [LogLig].[dbo].[DisciplineRecords] DROP COLUMN SeasonRecordSortValue;
GO
