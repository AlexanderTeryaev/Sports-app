UPDATE [LogLig].[dbo].[Users] SET IsActive = 1 Where 
[UserId] In (SELECT DISTINCT [UserId]
  FROM [LogLig].[dbo].[TeamsPlayers]
  Where SeasonId = 1135)