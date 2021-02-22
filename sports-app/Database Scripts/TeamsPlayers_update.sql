--Update season id
DECLARE @ExistingSeasonId int
DECLARE @ExistingTeamId int
DECLARE @ExistingUserId int
DECLARE @ExistingPosId int
DECLARE @ExistingShirtNum int
DECLARE @ExistingIsActive bit

DECLARE @NewSeasonId int

DECLARE MY_CURSOR CURSOR 
  LOCAL STATIC READ_ONLY FORWARD_ONLY
FOR 
SELECT  TeamId,UserId,PosId,ShirtNum,IsActive,SeasonId 
FROM [LogLig].[dbo].[TeamsPlayers]

OPEN MY_CURSOR
FETCH NEXT FROM MY_CURSOR INTO @ExistingTeamId, @ExistingUserId, @ExistingPosId, @ExistingShirtNum, @ExistingIsActive, @ExistingSeasonId
WHILE @@FETCH_STATUS = 0
BEGIN 
    --Find newest SeasonId
	SELECT TOP 1 @NewSeasonId=SeasonId from [LogLig].[dbo].[LeagueTeams] WHERE TeamId = @ExistingTeamId ORDER BY SeasonId DESC

	IF NOT EXISTS(SELECT Id FROM [LogLig].[dbo].[TeamsPlayers] WHERE TeamId=@ExistingTeamId AND UserId=@ExistingUserId AND SeasonId=@NewSeasonId)
	BEGIN
      INSERT INTO [LogLig].[dbo].[TeamsPlayers] VALUES (@ExistingTeamId, @ExistingUserId, @ExistingPosId, @ExistingShirtNum, @ExistingIsActive, @NewSeasonId)
	END
    PRINT @ExistingUserId
    PRINT @ExistingSeasonId
    FETCH NEXT FROM MY_CURSOR INTO @ExistingTeamId, @ExistingUserId, @ExistingPosId, @ExistingShirtNum, @ExistingIsActive, @ExistingSeasonId
END
CLOSE MY_CURSOR
DEALLOCATE MY_CURSOR
