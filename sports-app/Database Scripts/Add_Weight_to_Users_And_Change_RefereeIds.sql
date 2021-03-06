ALTER TABLE [dbo].[Users] Add [Weight] int NULL;
  GO

ALTER TABLE [dbo].[Users] Add [WeightUnits] nvarchar(2) NULL;
  GO

ALTER TABLE [dbo].[Users] Add [WeightDate] datetime NULL;
  GO

ALTER TABLE [dbo].[GamesCycles]
 DROP CONSTRAINT FK_GamesCycles_Users;
GO

ALTER TABLE [dbo].[GamesCycles]
  ALTER COLUMN RefereeId varchar(255);
GO

EXEC sp_RENAME
 '[dbo].[GamesCycles].RefereeId' , 'RefereeIds', 'COLUMN'
GO