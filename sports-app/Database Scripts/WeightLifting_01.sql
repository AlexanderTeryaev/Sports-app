--	Created by Alexander Levinson (alexander.levinson.70@gmail.com) 2 Nov 2018
IF NOT EXISTS (
	SELECT  1
	  FROM	INFORMATION_SCHEMA.COLUMNS		C
	 WHERE	C.TABLE_NAME					= 'CompetitionAges'
	   AND  C.COLUMN_NAME					= 'from_weight'
)
BEGIN
	PRINT 'Adding "from_weight" column to "CompetitionAges" table';
	ALTER TABLE CompetitionAges ADD from_weight INT NULL;
END;

IF NOT EXISTS (
	SELECT  1
	  FROM	INFORMATION_SCHEMA.COLUMNS		C
	 WHERE	C.TABLE_NAME					= 'CompetitionAges'
	   AND  C.COLUMN_NAME					= 'to_weight'
)
BEGIN
	PRINT 'Adding "to_weight" column to "CompetitionAges" table';
	ALTER TABLE CompetitionAges ADD to_weight INT NULL;
END;

IF EXISTS (
	SELECT  1
	  FROM  INFORMATION_SCHEMA.COLUMNS		C
	 WHERE  C.TABLE_NAME					= 'CompetitionDisciplines'
	   AND  C.COLUMN_NAME					= 'DisciplineId'
	   AND  C.IS_NULLABLE					= 'NO'
)
BEGIN
	PRINT 'Making "CompetitionDisciplines.DisciplineId" column nullable.';
	ALTER TABLE CompetitionDisciplines ALTER COLUMN DisciplineId INT NULL;
END;

IF NOT EXISTS (
	SELECT  *
	  FROM  INFORMATION_SCHEMA.COLUMNS		C
     WHERE  C.TABLE_NAME					= 'CompetitionDisciplineRegistrations'
	   AND  C.COLUMN_NAME					= 'WeightDeclaration'
)
BEGIN
	PRINT 'Adding "WeightDeclaration" column to CompetitionDisciplineRegistrations table.';
	ALTER TABLE CompetitionDisciplineRegistrations ADD WeightDeclaration DECIMAL(12,2) NULL;
END;
