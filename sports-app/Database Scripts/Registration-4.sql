CREATE TABLE dbo.ActivityBranches
	(
	AtivityBranchId int NOT NULL IDENTITY (1, 1),
	BranchName nvarchar(500) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.ActivityBranches ADD CONSTRAINT
	PK_ActivityBranches PRIMARY KEY CLUSTERED 
	(
	AtivityBranchId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO


alter table Activities add ActivityBranchId int not null
go

ALTER TABLE dbo.Activities ADD CONSTRAINT
	FK_Activities_ActivityBranches FOREIGN KEY
	(
	ActivityBranchId
	) REFERENCES dbo.ActivityBranches
	(
	AtivityBranchId
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO

alter table ActivityBranches add SeasonId int null
go

ALTER TABLE dbo.ActivityBranches ADD CONSTRAINT
	FK_ActivityBranches_Seasons FOREIGN KEY
	(
	SeasonId
	) REFERENCES dbo.Seasons
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO


alter table ActivityBranches add UnionId int not null
go

ALTER TABLE dbo.ActivityBranches ADD CONSTRAINT
	FK_ActivityBranches_Unions FOREIGN KEY
	(
	UnionId
	) REFERENCES dbo.Unions
	(
	UnionId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO


alter table ActivitiesUsers add UserGroup int not null
GO

Drop table ActivitiesUsers
GO

CREATE TABLE dbo.ActivitiesUsers
	(
	ActivityUserId int NOT NULL IDENTITY (1, 1),
	UserId int NOT NULL,
	ActivityId int NOT NULL,
	UserGroup int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.ActivitiesUsers ADD CONSTRAINT
	PK_ActivitiesUsers PRIMARY KEY CLUSTERED 
	(
	ActivityUserId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.ActivitiesUsers ADD CONSTRAINT
	FK_ActivitiesUsers_Activities FOREIGN KEY
	(
	ActivityId
	) REFERENCES dbo.Activities
	(
	ActivityId
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ActivitiesUsers ADD CONSTRAINT
	FK_ActivitiesUsers_Users FOREIGN KEY
	(
	UserId
	) REFERENCES dbo.Users
	(
	UserId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO


INSERT INTO [dbo].[JobsRoles]
           ([RoleId]
           ,[Title]
           ,[RoleName]
           ,[Priority])
     VALUES
           (7
           ,'Activity viewer'
           ,'activityviewer'
           ,5)
GO

declare @JobRoleId int
set @JobRoleId = (select top 1 RoleId from JobsRoles where RoleName = 'activityviewer')

INSERT INTO [dbo].[Jobs]
           ([SectionId]
           ,[RoleId]
           ,[JobName]
           ,[IsArchive])
     SELECT [SectionId], @JobRoleId, 'Activity viewer', 0
  FROM [dbo].[Sections]

GO




CREATE TABLE dbo.LeaguesPrices
	(
	LeaguePriceId int NOT NULL IDENTITY (1, 1),
	Price decimal(18, 2) NULL,
	StartDate datetime NULL,
	EndDate datetime NULL,
	PriceType int NULL,
	LeagueId int NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.LeaguesPrices ADD CONSTRAINT
	PK_LeaguesPrices PRIMARY KEY CLUSTERED 
	(
	LeaguePriceId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.LeaguesPrices ADD CONSTRAINT
	FK_LeaguesPrices_Leagues FOREIGN KEY
	(
	LeagueId
	) REFERENCES dbo.Leagues
	(
	LeagueId
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO


-- 08.06.2017
alter table Activities add IsAutomatic bit null
go


CREATE TABLE dbo.ActivitiesLeagues
	(
	ActivityLeagueId int NOT NULL IDENTITY (1, 1),
	ActivityId int NOT NULL,
	LeagueId int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.ActivitiesLeagues ADD CONSTRAINT
	PK_ActivitiesLeagues PRIMARY KEY CLUSTERED 
	(
	ActivityLeagueId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

ALTER TABLE dbo.ActivitiesLeagues ADD CONSTRAINT
	FK_ActivitiesLeagues_Activities FOREIGN KEY
	(
	ActivityId
	) REFERENCES dbo.Activities
	(
	ActivityId
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ActivitiesLeagues ADD CONSTRAINT
	FK_ActivitiesLeagues_Leagues FOREIGN KEY
	(
	LeagueId
	) REFERENCES dbo.Leagues
	(
	LeagueId
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO