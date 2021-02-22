ALTER TABLE [dbo].[RouteRanks] ALTER COLUMN [FromAge] DATETIME NULL;

ALTER TABLE [dbo].[RouteRanks] ALTER COLUMN [ToAge] DATETIME NULL;

UPDATE [dbo].[RouteRanks] SET [FromAge] = NULL, [ToAge] = NULL;

ALTER TABLE [dbo].[RouteTeamRanks] ALTER COLUMN [FromAge] DATETIME NULL;

ALTER TABLE [dbo].[RouteTeamRanks] ALTER COLUMN [ToAge] DATETIME NULL;

UPDATE [dbo].[RouteTeamRanks] SET [FromAge] = NULL, [ToAge] = NULL;