-- Table alter/create srcipts start
ALTER TABLE Sections ADD IsRegionallevelEnabled bit NOT NULL DEFAULT(0);
-------------
CREATE TABLE Regionals
(
	RegionalId [int] IDENTITY(1,1) NOT NULL primary key,
	[Name] [nvarchar](100) NULL,
	[IsArchive] [bit] NOT NULL,
	[UnionId] [int] NULL,
	[SeasonId] [int] NULL,
	[Logo] [nvarchar](150) NULL,
	[PrimaryImage] [nvarchar](150) NULL,	
	[IndexImage] [nvarchar](150) NULL,
	[Address] [nvarchar](250) NULL,
	[Phone] [nvarchar](20) NULL,	
	[Email] [nvarchar](100) NULL,
	[Description] [nvarchar](250) NULL,
	[IndexAbout] [nvarchar](2000) NULL,
	[CreateDate] [datetime] NOT NULL
);

ALTER TABLE Regionals ADD CONSTRAINT DF_Constraint DEFAULT GetDate() FOR CreateDate

-------------

INSERT [dbo].[JobsRoles] ([RoleId], [Title], [RoleName], [Priority]) VALUES (17, N'Regional manager', N'Regionalmgr', 45)

-------------
-- below column added to store mapping regional and regionalmanager

ALTER TABLE UsersJobs add RegionalId INT

-- Table alter/create srcipts end


-- User defined function srcipt start

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[FN_GetRegionalManagerById]
(
	@regionalid int
)
RETURNS NVARCHAR(MAX) 
 AS
BEGIN 
	  DECLARE @regionalmgr NVARCHAR(MAX) = null
	  
	SELECT @regionalmgr = COALESCE(@regionalmgr + ', ', '') +  ISNULL(FullName, '')
	FROM Users where UserId in (select UserId from UsersJobs where RegionalId = @regionalid )	
	
	
	RETURN isnull(@regionalmgr,'')
END
GO

-- User defined function srcipt end


-- stored proc srcipt stored

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [GetRegionals] 10, 0,0
CREATE PROCEDURE [dbo].[GetRegionals]
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

	--  , s.Name as SectionName	  
	  
	  From Regionals r 
	--  left join Sections s on r.SectionID = s.SectionId
	 -- left join Unions s on r.UnionId = s.UnionId
	   Where (isnull(@regionalid,0) <= 0 OR r.[RegionalId] = @regionalid)
	   AND (isnull(@seasonid,0) <= 0 OR r.SeasonId = @seasonid)
	   AND (isnull(@unionid,0) <= 0 OR r.UnionId = @unionid)
	   AND IsArchive=0;
END
GO

-------------

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddUpdateRegionals]
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

	IF EXISTS(select RegionalID from [Regionals] where RegionalID = @regionalid 
	OR (UnionId = @unionid AND LOWER(Name) = RTRIM(LTRIM(LOWER(@name))))
	)
		BEGIN
			UPDATE [Regionals]
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
			INSERT INTO [Regionals]
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
GO

-------------

/****** Object:  StoredProcedure [dbo].[DeleteRegional]    Script Date: 02/04/2019 18:42:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteRegional]
	@regionalid int 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  Update Regionals set IsArchive = 1 where RegionalId = @regionalid;
  SELECT * FROM Regionals where RegionalId = @regionalid;

END
GO

-------------

/****** Object:  StoredProcedure [dbo].[GetRegionalById]    Script Date: 02/04/2019 18:42:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [GetRegionals] 10, 0,0
CREATE PROCEDURE [dbo].[GetRegionalById]
	@regionalid INT

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  select r.*, u.Name as UnionName, sa.Name as SeasonName,s.SectionId, s.Name as SectionName, s.Alias as SectionAlias
 from Regionals r 
left join Seasons sa on r.SeasonId = sa.Id and sa.IsActive = 1 
left join Unions u on r.UnionId = u.UnionId and u.IsArchive = 0 and r.IsArchive = 0
left join Sections s on u.SectionId = s.SectionId and s.IsRegionallevelEnabled = 1 
where RegionalId = @regionalid;

END
GO


-------------
-- stored proc srcipt end


 -- *Note -please run the scripts in same order.
  -- ** if stored proc is already exists , alter it.