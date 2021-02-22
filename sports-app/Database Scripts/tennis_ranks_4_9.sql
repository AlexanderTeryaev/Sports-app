
ALTER TABLE dbo.Seasons
    ADD PreviousSeasonId int NULL,
    FOREIGN KEY(PreviousSeasonId) REFERENCES dbo.Seasons(Id);
	
	
ALTER TABLE dbo.TennisRank ADD
	AveragePoints int NULL,
	PointsToAverage int NULL,
	SeasonId int NULL,
	FOREIGN KEY(SeasonId) REFERENCES dbo.Seasons(Id);	