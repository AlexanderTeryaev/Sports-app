Alter table [dbo].[CompetitionExpertiesHeats] drop constraint [FKEY_COMPEXPHEAT_DISC]
Go

Alter table [dbo].[CompetitionExpertiesHeats] drop column HeatDisciplineId
Go

-- Create CompetitionExpertiesDisciplineHeats

IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[CompetitionExpertiesDisciplineHeats]') AND type IN ('U'))
	DROP TABLE [dbo].[CompetitionExpertiesDisciplineHeats]
GO

CREATE TABLE [dbo].[CompetitionExpertiesDisciplineHeats] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [CompetitionExpertiesHeatId] int,
  [HeatDisciplineId] int
)
GO

ALTER TABLE [dbo].[CompetitionExpertiesDisciplineHeats] SET (LOCK_ESCALATION = TABLE)
GO

-- ----------------------------
-- Primary Key structure for table CompetitionExpertiesDisciplineHeats
-- ----------------------------
ALTER TABLE [dbo].[CompetitionExpertiesDisciplineHeats] ADD CONSTRAINT [PK__CompExptDiscHeats] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

-- ----------------------------
-- Foreign Keys structure for table CompetitionExpertiesDisciplineHeats
-- ----------------------------
ALTER TABLE [dbo].[CompetitionExpertiesDisciplineHeats] ADD CONSTRAINT [FKEY_COMPEXPDISCHEAT_COMPEXPHEAT] FOREIGN KEY ([CompetitionExpertiesHeatId]) REFERENCES [dbo].[CompetitionExpertiesHeats] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[CompetitionExpertiesDisciplineHeats] ADD CONSTRAINT [FKEY_COMPEXPDISCHEAT_DISC] FOREIGN KEY ([HeatDisciplineId]) REFERENCES [dbo].[Disciplines] ([DisciplineId]) ON DELETE CASCADE ON UPDATE NO ACTION
GO


-- Create BicycleDisciplineRegistrations

IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[BicycleDisciplineRegistrations]') AND type IN ('U'))
	DROP TABLE [dbo].[BicycleDisciplineRegistrations]
GO

CREATE TABLE [dbo].[BicycleDisciplineRegistrations] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [CompetitionExpertiesHeatId] int,
  [UserId] int,
  [IsArchive] bit,
  [ClubId] int NULL,
)
GO

ALTER TABLE [dbo].[BicycleDisciplineRegistrations] SET (LOCK_ESCALATION = TABLE)
GO

-- ----------------------------
-- Primary Key structure for table BicycleDisciplineRegistrations
-- ----------------------------
ALTER TABLE [dbo].[BicycleDisciplineRegistrations] ADD CONSTRAINT [PK__BicyDiscReg] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

-- ----------------------------
-- Foreign Keys structure for table BicycleDisciplineRegistrations
-- ----------------------------
ALTER TABLE [dbo].[BicycleDisciplineRegistrations] ADD CONSTRAINT [FKEY_BICYDISCREG_COMPEXPHEAT] FOREIGN KEY ([CompetitionExpertiesHeatId]) REFERENCES [dbo].[CompetitionExpertiesHeats] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[BicycleDisciplineRegistrations] ADD CONSTRAINT [FKEY_BICYDISCREG_USER] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[BicycleDisciplineRegistrations] ADD CONSTRAINT [FKEY_BICYDISCREG_CLUB] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

Delete [dbo].[CompetitionExpertiesHeats]
GO