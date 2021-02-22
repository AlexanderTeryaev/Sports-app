Alter table [dbo].[CompetitionAges]
Add 
	IsIsraelChampionship bit null DEFAULT(0)


-- Add Competition Level in CompetitionExpertiesHeats

Alter table [dbo].[CompetitionExpertiesHeats] 
Add CompetitionLevelId int null
Go

Alter table [dbo].[CompetitionExpertiesHeats] ADD CONSTRAINT [FKEY_COMPEXPHEAT_COMPLVL] FOREIGN KEY ([CompetitionLevelId]) REFERENCES [dbo].[CompetitionLevel] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- Create CompetitionExpertiesHeatsAges

IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[CompetitionExpertiesHeatsAges]') AND type IN ('U'))
	DROP TABLE [dbo].[CompetitionExpertiesHeatsAges]
GO

CREATE TABLE [dbo].[CompetitionExpertiesHeatsAges] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [CompetitionExpertiesHeatId] int,
  [CompetitionAgeId] int
)
GO

ALTER TABLE [dbo].[CompetitionExpertiesHeatsAges] SET (LOCK_ESCALATION = TABLE)
GO

-- ----------------------------
-- Primary Key structure for table CompetitionExpertiesHeatsAges
-- ----------------------------
ALTER TABLE [dbo].[CompetitionExpertiesHeatsAges] ADD CONSTRAINT [PK__CompExptHeatsAges] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

-- ----------------------------
-- Foreign Keys structure for table CompetitionExpertiesHeatsAges
-- ----------------------------
ALTER TABLE [dbo].[CompetitionExpertiesHeatsAges] ADD CONSTRAINT [FKEY_COMPEXPHEATAGES_COMPEXPHEAT] FOREIGN KEY ([CompetitionExpertiesHeatId]) REFERENCES [dbo].[CompetitionExpertiesHeats] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[CompetitionExpertiesHeatsAges] ADD CONSTRAINT [FKEY_COMPEXPDISCHEAT_COMPAGE] FOREIGN KEY ([CompetitionAgeId]) REFERENCES [dbo].[CompetitionAges] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO