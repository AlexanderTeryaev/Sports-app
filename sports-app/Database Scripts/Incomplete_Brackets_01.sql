IF EXISTS(
	SELECT  GR.GroupId
	  FROM	Groups			GR
	  JOIN	Stages			ST
		ON	ST.StageId		= GR.StageId
	 WHERE  GR.Name			IS NULL
	)
BEGIN
	PRINT('Fix crash situation.')
	UPDATE  Groups
	   SET  Name		= CONCAT(CONVERT(nvarchar, GroupId), '_Group_name_restored_from_null')
	 WHERE	GroupId		IN (
	SELECT  GR.GroupId
	  FROM	Groups			GR
	  JOIN	Stages			ST
		ON	ST.StageId		= GR.StageId
	 WHERE  GR.Name			IS NULL
	 );
END;

IF EXISTS(
	SELECT  1
	  FROM	INFORMATION_SCHEMA.COLUMNS		C
	 WHERE  C.TABLE_NAME					= 'Groups'
	   AND  C.COLUMN_NAME					= 'Name'
	   AND  C.IS_NULLABLE					= 'YES'
	)
BEGIN
	PRINT('Make Groups.Name column not nullable');
	ALTER TABLE Groups
		ALTER COLUMN Name NVARCHAR(120) NOT NULL;
END;

IF EXISTS(
	SELECT  1
	  FROM	GroupsTeams		GT
	 WHERE  GT.Pos			IS NULL
  )
BEGIN
  PRINT('Update NULL team positions');
  UPDATE  GroupsTeams
     SET  Pos			= TeamId
   WHERE  Pos			IS NULL;
END;

IF EXISTS(
	SELECT  1
	  FROM  INFORMATION_SCHEMA.COLUMNS			C
	 WHERE  C.TABLE_NAME						= 'GroupsTeams'
	   AND  C.COLUMN_NAME						= 'Pos'
	   AND  C.IS_NULLABLE						= 'YES'
	)
BEGIN
  PRINT('Change primary key constraint on GroupTeams table and nullability of columns');
  ALTER TABLE GroupsTeams ALTER COLUMN Pos INT NOT NULL;

  ALTER TABLE [GroupsTeams] DROP CONSTRAINT [PK_GroupsTeams];

  ALTER TABLE [GroupsTeams] ADD  CONSTRAINT [PK_GroupsTeams] PRIMARY KEY CLUSTERED 
  (
    [GroupId] ASC,
    [Pos] ASC
  );

  ALTER TABLE [GroupsTeams] ALTER COLUMN TeamId INT NULL;
END;

IF EXISTS(
  SELECT  *
    FROM  INFORMATION_SCHEMA.COLUMNS			C
   WHERE  C.TABLE_NAME							= 'GamesCycles'
     AND  C.COLUMN_NAME							= 'GroupeId'
	)
BEGIN
  PRINT 'Change GamesCycles.GroupeId field name to GroupId';
  EXEC sp_rename 'GamesCycles.GroupeId', 'GroupId', 'COLUMN';
END;

IF NOT EXISTS(
  SELECT  1
    FROM  INFORMATION_SCHEMA.COLUMNS			C
   WHERE  C.TABLE_NAME							= 'GamesCycles'
     AND  C.COLUMN_NAME							= 'GuestTeamPos'
	)
BEGIN
  PRINT('Add GuestTeamPos and HomeTeamPos columns to GamesCycles table');
  ALTER TABLE GamesCycles ADD GuestTeamPos INT NULL, HomeTeamPos INT NULL;
END;

IF NOT EXISTS(
  SELECT  *
    FROM  INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS	RC
   WHERE  RC.CONSTRAINT_NAME							= 'FK_GameSets_GamesCycles'
   )
BEGIN
  PRINT('Restore FK_GameSets_GamesCycles constraint lost by unknown reason');

  DELETE
    FROM  GameSets
   WHERE  GameCycleId	NOT IN (SELECT  CycleId FROM GamesCycles);		

  ALTER TABLE GameSets
  ADD CONSTRAINT FK_GameSets_GamesCycles FOREIGN KEY (GameCycleId)     
    REFERENCES GamesCycles(CycleId)     
    ON DELETE CASCADE    
    ON UPDATE CASCADE;
END;

IF NOT EXISTS(
	SELECT  1
	  FROM  INFORMATION_SCHEMA.COLUMNS			C
	 WHERE  C.TABLE_NAME						= 'PlayoffBrackets'
	   AND  C.COLUMN_NAME						= 'Team1GroupPosition'
	)
BEGIN
  PRINT('Add teams positions in the group to PlayoffBrackets table.');
  ALTER TABLE PlayoffBrackets ADD Team1GroupPosition INT NULL, Team2GroupPosition INT NULL;
END;
