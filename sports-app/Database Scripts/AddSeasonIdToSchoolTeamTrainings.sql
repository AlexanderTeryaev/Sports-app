-- Filling seasonId to be last season of club (read from ClubTeams table)
DECLARE @i int
DECLARE @teamId int
DECLARE @numrows int
DECLARE @currentSeasonId int


-- enumerate the table
SET @i = 1
SET @numrows = (SELECT COUNT(*) FROM TeamTrainings)
IF @numrows > 0
    WHILE (@i <= (SELECT MAX(Id) FROM TeamTrainings))
    BEGIN

        SET @teamId = (SELECT TeamId FROM TeamTrainings WHERE Id = @i)
		    SET @currentSeasonId =  (SELECT MAX (Schools.SeasonId)
  FROM SchoolTeams left join Schools on Schools.Id = SchoolTeams.SchoolId
    WHERE SchoolTeams.TeamId = @teamId
  GROUP BY SchoolTeams.TeamId)
  IF @currentSeasonId > 0
			 UPDATE TeamTrainings SET SeasonId =@currentSeasonId 
  WHERE TeamTrainings.TeamId = @teamId
  
        -- increment counter for next team
        SET @i = @i + 1
    END