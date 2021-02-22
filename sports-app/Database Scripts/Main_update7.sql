IF NOT EXISTS(
	SELECT  1
	  FROM  INFORMATION_SCHEMA.COLUMNS			C
	 WHERE  C.TABLE_NAME						= 'Events'
	   AND  C.COLUMN_NAME						= 'ClubId'
	)
BEGIN
	PRINT('Add "ClubId" field to the "Events" table.');
	ALTER TABLE dbo.[Events] ALTER COLUMN LeagueId int NULL;
	ALTER TABLE dbo.[Events] ADD ClubId int NULL;
	EXECUTE('
	ALTER TABLE dbo.[Events] WITH CHECK ADD CONSTRAINT FK_Events_Clubs FOREIGN KEY([ClubId])
		REFERENCES dbo.Clubs (ClubId);');
	EXECUTE('
	ALTER TABLE dbo.[Events] ADD CONSTRAINT CHK_FK_Event_To_League_Or_Club
		CHECK ((LeagueId IS NULL AND ClubId IS NOT NULL) 
			OR (LeagueId IS NOT NULL AND ClubId IS NULL));');
END;

IF NOT EXISTS(
	SELECT  1
	  FROM  INFORMATION_SCHEMA.COLUMNS			C
	 WHERE  C.TABLE_NAME						= 'Teams'
	   AND  C.COLUMN_NAME						= 'IsUnderAdult'
	)
BEGIN
	PRINT('Add "IsUnderAdult" and "IsReserved" columns to "Teams" table');
	ALTER TABLE dbo.Teams ADD IsUnderAdult bit NULL;
	ALTER TABLE dbo.Teams ADD IsReserved bit NULL;
END;
