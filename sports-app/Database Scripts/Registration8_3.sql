INSERT INTO [dbo].[JobsRoles]
           ([RoleId]
           ,[Title]
           ,[RoleName]
           ,[Priority])
     VALUES
           (8
           ,'Registration active'
           ,'registrationactive'
           ,5)

declare @JobRoleId int
set @JobRoleId = (select top 1 RoleId from JobsRoles where RoleName = 'registrationactive')

INSERT INTO [dbo].[Jobs]
           ([SectionId]
           ,[RoleId]
           ,[JobName]
           ,[IsArchive])
     SELECT [SectionId], @JobRoleId, 'Registration active', 0
  FROM [dbo].[Sections]
  
  