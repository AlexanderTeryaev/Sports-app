ALTER TABLE Unions ADD IsRegionallevelEnabled bit NOT NULL DEFAULT(0);

-----------

GO
CREATE FUNCTION [dbo].[FN_GetRegionalManagerEmailById]
(
	@regionalid int
)
RETURNS NVARCHAR(MAX) 
 AS
BEGIN 
	  DECLARE @regionalmgremail NVARCHAR(MAX) = null
	  
	SELECT @regionalmgremail = COALESCE(@regionalmgremail + ', ', '') +  ISNULL(Email, '')
	FROM Users where UserId in (select UserId from UsersJobs where RegionalId = @regionalid )
	
	RETURN isnull(@regionalmgremail,'')
END

-----------

GO
ALTER PROCEDURE [dbo].[GetRegionals]
	@regionalid int = null,  
	@unionid int = null,
	@seasonid int = null

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   SELECT [RegionalId]
      ,r.[Name] 
      ,[IsArchive]
      ,[UnionId]
      ,[SeasonId]
      ,[Logo]
      ,[PrimaryImage]
      ,[IndexImage]
      ,[Address]
      ,[Phone]
      ,[Email]
      ,[Description]
      ,[IndexAbout]
      ,[CreateDate]
      , ISNULL(dbo.FN_GetRegionalManagerById(RegionalId),'') as RegionalManager
      , ISNULL(dbo.FN_GetRegionalManagerEmailById(RegionalId),'') as RegionalManagerEmail
      

	--  , s.Name as SectionName	  
	  
	  From Regionals r 
	--  left join Sections s on r.SectionID = s.SectionId
	 -- left join Unions s on r.UnionId = s.UnionId
	   Where (isnull(@regionalid,0) <= 0 OR r.[RegionalId] = @regionalid)
	   AND (isnull(@seasonid,0) <= 0 OR r.SeasonId = @seasonid)
	   AND (isnull(@unionid,0) <= 0 OR r.UnionId = @unionid)
	   AND IsArchive=0;
END


-----------
GO
-- GetRegionalById 10
ALTER PROCEDURE [dbo].[GetRegionalById]
	@regionalid INT

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  select r.*, u.Name as UnionName, sa.Name as SeasonName,s.SectionId, s.Name as SectionName, s.Alias as SectionAlias
 from Regionals r 
left join Seasons sa on r.SeasonId = sa.Id and sa.IsActive = 1 
left join Unions u on r.UnionId = u.UnionId and u.IsArchive = 0 and r.IsArchive = 0 and u.IsRegionallevelEnabled = 1 
left join Sections s on u.SectionId = s.SectionId  
where RegionalId = @regionalid;

END
-----------

GO
ALTER PROCEDURE [dbo].[DeleteRegional]
	@regionalid int 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  Update Regionals set IsArchive = 1 where RegionalId = @regionalid;
  SELECT * FROM Regionals where RegionalId = @regionalid;

END

-----------

GO
ALTER PROCEDURE [dbo].[AddUpdateRegionals]
	@regionalid int = null,  
	@name nvarchar(100),
	@isarchive bit = 0,
	@unionid int = null,
	@seasonid int = null,
	@logo nvarchar(150) = null,
	@primaryimage nvarchar(150) = null,
	@indeximage nvarchar(150) = null,
	@address nvarchar(250) = null,
	@phone nvarchar(20) = null,
	@email nvarchar(150) = null,
	@description nvarchar(250) = null,
	@indexabout nvarchar(2000) = null,
	@createdate datetime = null
	

AS
BEGIN

	IF EXISTS(select RegionalID from Regionals where RegionalID = @regionalid 
	OR (UnionId = @unionid AND LOWER(Name) = RTRIM(LTRIM(LOWER(@name))))
	)
		BEGIN
			UPDATE Regionals
		   SET [Name] = isnull(@name,[Name])
			  ,[IsArchive] = isnull(@isarchive,[IsArchive])
			  ,[UnionId] = isnull(@unionid,[UnionId])
			  ,[SeasonId] = isnull(@seasonid,[SeasonId])
			  ,[Logo] = isnull(@logo,[Logo])
			  ,[PrimaryImage] = isnull(@primaryimage,[PrimaryImage])
			  ,[IndexImage] = isnull(@indeximage,[IndexImage])
			  ,[Address] = isnull(@address,[Address])
			  ,[Phone] = isnull(@phone,[Phone])
			  ,[Email] = isnull(@email,[Email])
			  ,[Description] = isnull(@description,[Description])
			  ,[IndexAbout] = isnull(@indexabout,[IndexAbout])
		 WHERE RegionalID = @regionalid
		 OR (UnionId = @unionid AND LOWER(Name) = RTRIM(LTRIM(LOWER(@name))))
		
		END
	ELSE
		BEGIN
			INSERT INTO Regionals
				   (
						[Name]
					   ,[IsArchive]
					   ,[UnionId]
					   ,[SeasonId]
					   ,[Logo]
					   ,[PrimaryImage]
					   ,[IndexImage]
					   ,[Address]
					   ,[Phone]
					   ,[Email]
					   ,[Description]
					   ,[IndexAbout]
				   )
			 VALUES
				   (			
						@name ,
						@IsArchive ,
						@UnionId ,
						@SeasonId ,
						@Logo ,
						@PrimaryImage ,
						@IndexImage ,
						@Address ,
						@phone ,
						@email ,
						@Description ,
						@IndexAbout 
				   )
				  
		END       
END
