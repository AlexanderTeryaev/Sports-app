IF NOT EXISTS(
	SELECT  1
	  FROM  INFORMATION_SCHEMA.COLUMNS			C
	 WHERE  C.TABLE_NAME						= 'Clubs'
	   AND  C.COLUMN_NAME						= 'UnionId'
	   )
BEGIN
	PRINT('Add UnionId field to the Clubs table.');
	ALTER TABLE dbo.Clubs ALTER COLUMN SectionId int NULL;
	ALTER TABLE dbo.Clubs ADD UnionId int NULL;
	ALTER TABLE dbo.Clubs ADD SeasonId int NULL;
	ALTER TABLE dbo.Clubs DROP CONSTRAINT DF_Clubs_SectionId;
	ALTER TABLE dbo.Clubs WITH CHECK ADD  CONSTRAINT FK_Clubs_Unions FOREIGN KEY([UnionId])
		REFERENCES dbo.Unions (UnionId);
END;

IF NOT EXISTS(
	SELECT  1
	  FROM	INFORMATION_SCHEMA.CHECK_CONSTRAINTS    CHK
	 WHERE  CHK.CONSTRAINT_NAME						= 'CHK_FK_Section_Or_Union')
BEGIN
	PRINT('Add constraint checking that Club has either UnionId or SectionId filled in.');
	EXECUTE('
		ALTER TABLE dbo.Clubs WITH CHECK ADD CONSTRAINT CHK_FK_Section_Or_Union CHECK
			  ((SectionId IS NOT NULL AND UnionId IS NULL AND SeasonId IS NULL) 
			OR (SectionId IS NULL AND UnionId IS NOT NULL AND SeasonId IS NOT NULL))
	');
	ALTER TABLE dbo.Clubs ADD IsSectionClub AS (CONVERT(bit, CASE WHEN SectionId IS NOT NULL THEN 1 ELSE 0 END));
	EXECUTE('
		ALTER TABLE dbo.Clubs ADD IsUnionClub AS (CONVERT(bit, CASE WHEN UnionId IS NOT NULL THEN 1 ELSE 0 END));
	');
END;

IF NOT EXISTS(
	SELECT  *
	  FROM	INFORMATION_SCHEMA.TABLES		T
	 WHERE  T.TABLE_NAME					= 'SportCenters')
BEGIN
	PRINT('Create SportCenters table.');
	CREATE TABLE dbo.SportCenters (
		Id int NOT NULL PRIMARY KEY,
		Eng NVARCHAR(200) NOT NULL,
	    Heb NVARCHAR(200) NOT NULL
	);
	EXECUTE('INSERT INTO dbo.SportCenters (Id, Eng, Heb) VALUES (1, ''Hapoel'', ''הפועל'')');
	EXECUTE('INSERT INTO dbo.SportCenters (Id, Eng, Heb) VALUES (2, ''Maccabi'', ''מכבי'')');
	EXECUTE('INSERT INTO dbo.SportCenters (Id, Eng, Heb) VALUES (3, ''Elitzur'', ''אליצור'')');
	EXECUTE('INSERT INTO dbo.SportCenters (Id, Eng, Heb) VALUES (4, ''Beitar'', ''בית"ר'')');
	EXECUTE('INSERT INTO dbo.SportCenters (Id, Eng, Heb) VALUES (5, ''Asa'', ''אס"א'')');
	EXECUTE('INSERT INTO dbo.SportCenters (Id, Eng, Heb) VALUES (6, ''otzma'', ''עוצמה'')');
	EXECUTE('INSERT INTO dbo.SportCenters (Id, Eng, Heb) VALUES (7, ''Independent'', ''עצמאי'')');
	EXECUTE('INSERT INTO dbo.SportCenters (Id, Eng, Heb) VALUES (8, ''Disabled'', ''נכים'')');
	EXECUTE('INSERT INTO dbo.SportCenters (Id, Eng, Heb) VALUES (9, ''Special Olympics'', ''ספיישל אולימפיק'')');
END;

IF NOT EXISTS(
	SELECT  1
	  FROM  INFORMATION_SCHEMA.COLUMNS			C
	 WHERE  C.TABLE_NAME						= 'Clubs'
	   AND  C.COLUMN_NAME						= 'SportCenterId'
	   )
BEGIN
  PRINT('Add SportCenterId to Clubs table.');
  ALTER TABLE Clubs ADD SportCenterId int NULL;
  ALTER TABLE Clubs ADD CONSTRAINT FK_SPORT_CENTER FOREIGN KEY (SportCenterId)
      REFERENCES dbo.SportCenters (Id);
END;

IF NOT EXISTS(
	SELECT  1
	  FROM  INFORMATION_SCHEMA.COLUMNS			C
	 WHERE  C.TABLE_NAME						= 'Clubs'
	   AND  C.COLUMN_NAME						= 'NGO_Number'
	   )
BEGIN
  PRINT('Add NGO_Number to Clubs table.');
  ALTER TABLE Clubs ADD NGO_Number int NULL;
END;

IF	NOT EXISTS(
	SELECT  1
	  FROM	JobsRoles		JR
	 WHERE  JR.RoleName		= 'clubmgr'
	)
BEGIN
    PRINT('Add "Club Manager" role');
	INSERT INTO JobsRoles(RoleId, Title, RoleName, [Priority])
		VALUES (5, 'מנהל מועדון', 'clubmgr', 100);
END;

IF NOT EXISTS(
	SELECT  1
	  FROM	INFORMATION_SCHEMA.CHECK_CONSTRAINTS    CHK
	 WHERE  CHK.CONSTRAINT_NAME						= 'CHK_FK_Club_Or_Union')
BEGIN
	PRINT('Add constraint checking that Auditoriums (Arena) has either UnionId or ClubId filled in.');
	ALTER TABLE dbo.Auditoriums ALTER COLUMN UnionId int NULL;
	ALTER TABLE dbo.Auditoriums WITH CHECK ADD CONSTRAINT CHK_FK_Club_Or_Union CHECK
			((UnionId IS NOT NULL AND ClubId IS NULL) 
		OR (UnionId IS NULL AND ClubId IS NOT NULL));
END;

