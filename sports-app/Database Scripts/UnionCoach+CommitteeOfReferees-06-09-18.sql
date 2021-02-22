IF NOT EXISTS (SELECT [RoleId] FROM [dbo].[JobsRoles] WHERE [RoleId] = 15)
	BEGIN
		INSERT INTO [dbo].[JobsRoles] 
		([RoleId], [Title], [RoleName], [Priority])
		VALUES (15, 'Union coach', 'unioncoach', 15)
	END

IF NOT EXISTS (SELECT [RoleId] FROM [dbo].[JobsRoles] WHERE [RoleId] = 14)
	BEGIN
		INSERT INTO [dbo].[JobsRoles] 
		([RoleId], [Title], [RoleName], [Priority])
		VALUES (14, 'Committee Of Referees', 'committeeofreferees', 20)
	END

ALTER TABLE Users ADD RefereeCertificate nvarchar(50)

