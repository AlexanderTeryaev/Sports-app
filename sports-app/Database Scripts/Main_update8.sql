IF NOT EXISTS(
	SELECT  1
	  FROM	INFORMATION_SCHEMA.COLUMNS		C
	 WHERE  C.TABLE_NAME					= 'GamesCycles'
	   AND	C.COLUMN_NAME					= 'BracketIndex'
)
BEGIN
	PRINT 'Add "BracketIndex" field to "GamesCycles" table';
	ALTER TABLE GamesCycles
	  ADD BracketIndex INT NULL;
END;
