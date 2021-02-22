/****** Script for Spectator JobRole Creation ******/
  GO

  IF NOT EXISTS (SELECT [RoleId] FROM [dbo].[JobsRoles] WHERE [RoleId] = 10)
	BEGIN
		INSERT INTO [dbo].[JobsRoles] 
		([RoleId], [Title], [RoleName], [Priority])
		VALUES (10, 'Spectator', 'spectator', 5)
	END
GO

ALTER TABLE [dbo].[GamesCycles]
ADD SpectatorIds nvarchar(255) null

