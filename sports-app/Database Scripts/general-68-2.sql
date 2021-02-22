Alter table [dbo].[Unions]
Add 
	UnionForeignName nvarchar(MAX)


Alter table [dbo].[FriendshipsTypes]
	Add Hierarchy int null 

GO

-- Create table InsuranceTypes

IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[InsuranceTypes]') AND type IN ('U'))
	DROP TABLE [dbo].[InsuranceTypes]
GO

CREATE TABLE [dbo].[InsuranceTypes] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [Name] nvarchar(MAX)
)
GO

ALTER TABLE [dbo].[InsuranceTypes] SET (LOCK_ESCALATION = TABLE)
GO

-- ----------------------------
-- Primary Key structure for table InsuranceTypes
-- ----------------------------
ALTER TABLE [dbo].[InsuranceTypes] ADD CONSTRAINT [PK__InsType] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


INSERT INTO [dbo].[InsuranceTypes]
           ([Name])
     VALUES
           ('Monthly paid – minimum wage'),
		   ('Activity support by work'),
		   ('School student'),
		   ('Activity with no payments'),
		   ('None')
GO


Alter table [dbo].[Users]
Add InsuranceTypeId int null
GO

ALTER TABLE [dbo].[Users] ADD CONSTRAINT [FKEY_USER_INSTYPE] FOREIGN KEY ([InsuranceTypeId]) REFERENCES [dbo].[InsuranceTypes] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

