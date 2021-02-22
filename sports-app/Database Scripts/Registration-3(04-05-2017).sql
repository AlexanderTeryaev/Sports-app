INSERT INTO [dbo].[JobsRoles]
           ([RoleId]
           ,[Title]
           ,[RoleName]
           ,[Priority])
     VALUES
           (6,'Activity manager'
           ,'activitymanager'
           ,120)
GO

CREATE TABLE dbo.Activities
	(
	ActivityId int NOT NULL IDENTITY (1, 1),
	IsPublished bit NOT NULL,
	Date datetime NOT NULL,
	Name nvarchar(500) NOT NULL,
	Description nvarchar(MAX) NULL,
	Type nvarchar(50) NOT NULL,
	StartDate datetime NOT NULL,
	EndDate datetime NOT NULL,
	PaymentDescription nvarchar(MAX) NULL,
	Price decimal(18, 2) NULL,
    FormPayment nvarchar(50) NOT NULL,
	ByBenefactor bit NOT NULL,
	AttachDocuments bit NOT NULL,
	MedicalCertificate bit NOT NULL,
	InsuranceCertificate bit NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE dbo.Activities ADD CONSTRAINT
	PK_Activities PRIMARY KEY CLUSTERED 
	(
	ActivityId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

CREATE TABLE dbo.ActivitiesUsers
	(
	ActivityId int NOT NULL,
	UserId int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.ActivitiesUsers ADD CONSTRAINT
	PK_ActivitiesUsers PRIMARY KEY CLUSTERED 
	(
	ActivityId,
	UserId
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


CREATE TABLE dbo.ActivitiesPrices
	(
	ActivityPeriodId int NOT NULL IDENTITY (1, 1),
	ActivityId int NOT NULL,
	StartDate datetime NOT NULL,
	EndDate datetime NOT NULL,
	Price decimal(18, 2) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.ActivitiesPrices ADD CONSTRAINT
	PK_ActivitiesPrices PRIMARY KEY CLUSTERED 
	(
	ActivityPeriodId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.ActivitiesPrices ADD CONSTRAINT
	FK_ActivitiesPrices_Activities FOREIGN KEY
	(
	ActivityId
	) REFERENCES dbo.Activities
	(
	ActivityId
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO


alter table Activities add UnionId int not null
GO

alter table Activities add SeasonId int null
GO                                                   

alter table Activities alter column StartDate datetime null
GO               

alter table Activities alter column EndDate datetime null
GO               



-- 10-05-2017

alter table ActivitiesPrices add PaymentDescription nvarchar(max)
GO


ALTER TABLE dbo.Activities ADD CONSTRAINT
	FK_Activities_Unions FOREIGN KEY
	(
	UnionId
	) REFERENCES dbo.Unions
	(
	UnionId
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.Activities ADD CONSTRAINT
	FK_Activities_Seasons FOREIGN KEY
	(
	SeasonId
	) REFERENCES dbo.Seasons
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  SET NULL 
	
GO



-- 11-05-2017

declare @JobRoleId int
set @JobRoleId = (select top 1 RoleId from JobsRoles where RoleName = 'activitymanager')

INSERT INTO [dbo].[Jobs]
           ([SectionId]
           ,[RoleId]
           ,[JobName]
           ,[IsArchive])
     SELECT [SectionId], @JobRoleId, 'Activity manager', 0
  FROM [dbo].[Sections]

GO



-- 15-05-2017
update JobsRoles set [Priority] = 5 where RoleName = 'activitymanager'