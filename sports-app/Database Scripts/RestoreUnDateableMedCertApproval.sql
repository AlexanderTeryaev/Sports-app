UPDATE [LogLig].[dbo].[TeamsPlayers] SET IsApprovedByManager = 1
From [LogLig].[dbo].[TeamsPlayers],[LogLig].[dbo].[Users]
WHERE SeasonId In (57,1130) and IsApprovedByManager IS NULL and [LogLig].[dbo].[TeamsPlayers].ApprovalDate Is Not Null