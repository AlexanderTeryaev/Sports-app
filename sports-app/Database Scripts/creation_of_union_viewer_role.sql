GO
  IF NOT EXISTS (SELECT [RoleId] FROM [dbo].[JobsRoles] WHERE [RoleId] = 13)
	BEGIN
		INSERT INTO [dbo].[JobsRoles] 
		([RoleId], [Title], [RoleName], [Priority])
		VALUES (13, 'Union viewer', 'unionviewer', 5)
	END
GO