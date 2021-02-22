delete from [LogLig].[dbo].[TeamsPlayers] where TeamId = 12257 and SeasonId = 1135 and UserId in (Select UserId FROM [LogLig].[dbo].[TeamsPlayers] Where TeamId != 12257 and SeasonId = 1135 and UserId in (SELECT [UserId]
  FROM [LogLig].[dbo].[TeamsPlayers]
  Where TeamId=12257 and SeasonId = 1135))