ALTER TABLE dbo.CompetitionDisciplines ADD
	NumberOfWhoPassesToNextStage int NULL
	
ALTER TABLE dbo.CompetitionResults ADD
	Records nvarchar(50) NULL
	
ALTER TABLE dbo.CompetitionResults ADD
	LiveSplitOrder int NULL