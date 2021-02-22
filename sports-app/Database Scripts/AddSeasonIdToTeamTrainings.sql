ALTER TABLE [dbo].[TeamTrainings] ADD SeasonId INT NULL
GO

ALTER TABLE [dbo].[TeamTrainings]
ADD CONSTRAINT FK_SeasonId_TeamTrainings FOREIGN KEY (SeasonId) REFERENCES Seasons(Id);
GO

-- Filling seasonId to be last season of club (read from ClubTeams table)
DECLARE @i int
DECLARE @teamId int
DECLARE @numrows int


-- enumerate the table
SET @i = 1
SET @numrows = (SELECT COUNT(*) FROM TeamTrainings)
IF @numrows > 0
    WHILE (@i <= (SELECT MAX(Id) FROM TeamTrainings))
    BEGIN

        SET @teamId = (SELECT TeamId FROM TeamTrainings WHERE Id = @i)
		    
			 UPDATE TeamTrainings SET SeasonId =
  (SELECT MAX (ClubTeams.SeasonId)
  FROM ClubTeams WHERE ClubTeams.TeamId = @teamId
  GROUP BY ClubTeams.TeamId)
  WHERE TeamTrainings.TeamId = @teamId
  
        -- increment counter for next team
        SET @i = @i + 1
    END