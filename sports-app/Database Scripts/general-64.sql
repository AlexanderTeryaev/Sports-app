-- Create BicycleCompetitionDisciplines 

IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[BicycleCompetitionDisciplines]') AND type IN ('U'))
	DROP TABLE [dbo].[BicycleCompetitionDisciplines]
GO

CREATE TABLE [dbo].[BicycleCompetitionDisciplines] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [Name] nvarchar(MAX) NULL,
  [SeasonId] int NULL,
  [UnionId] int NULL
)
GO

ALTER TABLE [dbo].[BicycleCompetitionDisciplines] SET (LOCK_ESCALATION = TABLE)
GO

-- ----------------------------
-- Primary Key structure for table BicycleCompetitionDisciplines
-- ----------------------------
ALTER TABLE [dbo].[BicycleCompetitionDisciplines] ADD CONSTRAINT [PK__BicyCompDisc] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

-- ----------------------------
-- Foreign Keys structure for table CompetitionDisciplineTeams
-- ----------------------------
ALTER TABLE [dbo].[BicycleCompetitionDisciplines] ADD CONSTRAINT [FKEY_BICYCOMPDISC_SEASON] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[BicycleCompetitionDisciplines] ADD CONSTRAINT [FKEY_BICYCOMPDISC_UNION] FOREIGN KEY ([UnionId]) REFERENCES [dbo].[Unions] ([UnionId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- Create DisciplineExpertise 

IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[DisciplineExpertise]') AND type IN ('U'))
	DROP TABLE [dbo].[DisciplineExpertise]
GO

CREATE TABLE [dbo].[DisciplineExpertise] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [Name] nvarchar(MAX) NULL,
  [BicycleCompetitionDisciplineId] int NULL
)
GO

ALTER TABLE [dbo].[DisciplineExpertise] SET (LOCK_ESCALATION = TABLE)
GO

-- ----------------------------
-- Primary Key structure for table DisciplineExpertise
-- ----------------------------
ALTER TABLE [dbo].[DisciplineExpertise] ADD CONSTRAINT [PK__DiscExpt] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

-- ----------------------------
-- Foreign Keys structure for table DisciplineExpertise
-- ----------------------------
ALTER TABLE [dbo].[DisciplineExpertise] ADD CONSTRAINT [FKEY_DISCEXP_BICYCOMPDISC] FOREIGN KEY ([BicycleCompetitionDisciplineId]) REFERENCES [dbo].[BicycleCompetitionDisciplines] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO



-- Create BicycleCompetitionHeats 

IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[BicycleCompetitionHeats]') AND type IN ('U'))
	DROP TABLE [dbo].[BicycleCompetitionHeats]
GO

CREATE TABLE [dbo].[BicycleCompetitionHeats] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [Name] nvarchar(MAX) NULL,
  [SeasonId] int NULL,
  [UnionId] int NULL
)
GO

ALTER TABLE [dbo].[BicycleCompetitionHeats] SET (LOCK_ESCALATION = TABLE)
GO

-- ----------------------------
-- Primary Key structure for table BicycleCompetitionHeats
-- ----------------------------
ALTER TABLE [dbo].[BicycleCompetitionHeats] ADD CONSTRAINT [PK__BicyCompHeat] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

-- ----------------------------
-- Foreign Keys structure for table BicycleCompetitionHeats
-- ----------------------------
ALTER TABLE [dbo].[BicycleCompetitionHeats] ADD CONSTRAINT [FKEY_BICYCOMPHEAT_SEASON] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[BicycleCompetitionHeats] ADD CONSTRAINT [FKEY_BICYCOMPHEAT_UNION] FOREIGN KEY ([UnionId]) REFERENCES [dbo].[Unions] ([UnionId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- ----------------------------
-- Foreign Keys structure for table Leagues
-- ----------------------------
ALTER TABLE [dbo].[Leagues]
ADD 
	BicycleCompetitionDisciplineId int NULL,
	LevelId int NULL

ALTER TABLE [dbo].[Leagues] ADD CONSTRAINT [FKEY_LEAGUE_BICYCOMPDISC] FOREIGN KEY ([BicycleCompetitionDisciplineId]) REFERENCES [dbo].[BicycleCompetitionDisciplines] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[Leagues] ADD CONSTRAINT [FKEY_LEAGUE_LEVEL] FOREIGN KEY ([LevelId]) REFERENCES [dbo].[CompetitionLevel] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- Create CompetitionExperties 

IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[CompetitionExperties]') AND type IN ('U'))
	DROP TABLE [dbo].[CompetitionExperties]
GO

CREATE TABLE [dbo].[CompetitionExperties] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [DisciplineExpertiseId] int,
  [CompetitionId] int
)
GO

ALTER TABLE [dbo].[CompetitionExperties] SET (LOCK_ESCALATION = TABLE)
GO

-- ----------------------------
-- Primary Key structure for table CompetitionExperties
-- ----------------------------
ALTER TABLE [dbo].[CompetitionExperties] ADD CONSTRAINT [PK__CompExpt] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

-- ----------------------------
-- Foreign Keys structure for table CompetitionExperties
-- ----------------------------
ALTER TABLE [dbo].[CompetitionExperties] ADD CONSTRAINT [FKEY_COMPEXP_DISCEXP] FOREIGN KEY ([DisciplineExpertiseId]) REFERENCES [dbo].[DisciplineExpertise] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[CompetitionExperties] ADD CONSTRAINT [FKEY_COMPEXP_LEAGUE] FOREIGN KEY ([CompetitionId]) REFERENCES [dbo].[Leagues] ([LeagueId]) ON DELETE CASCADE ON UPDATE NO ACTION
GO


-- Create CompetitionExpertiesHeats

IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[CompetitionExpertiesHeats]') AND type IN ('U'))
	DROP TABLE [dbo].[CompetitionExpertiesHeats]
GO

CREATE TABLE [dbo].[CompetitionExpertiesHeats] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [CompetitionExpertiesId] int,
  [BicycleCompetitionHeatId] int,
  [HeatDisciplineId] int
)
GO

ALTER TABLE [dbo].[CompetitionExpertiesHeats] SET (LOCK_ESCALATION = TABLE)
GO

-- ----------------------------
-- Primary Key structure for table CompetitionExpertiesHeats
-- ----------------------------
ALTER TABLE [dbo].[CompetitionExpertiesHeats] ADD CONSTRAINT [PK__CompExptHeats] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

-- ----------------------------
-- Foreign Keys structure for table CompetitionExpertiesHeats
-- ----------------------------
ALTER TABLE [dbo].[CompetitionExpertiesHeats] ADD CONSTRAINT [FKEY_COMPEXPHEAT_COMPEXP] FOREIGN KEY ([CompetitionExpertiesId]) REFERENCES [dbo].[CompetitionExperties] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[CompetitionExpertiesHeats] ADD CONSTRAINT [FKEY_COMPEXPHEAT_BICYCOMPHEAT] FOREIGN KEY ([BicycleCompetitionHeatId]) REFERENCES [dbo].[BicycleCompetitionHeats] ([Id]) ON DELETE CASCADE ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[CompetitionExpertiesHeats] ADD CONSTRAINT [FKEY_COMPEXPHEAT_DISC] FOREIGN KEY ([HeatDisciplineId]) REFERENCES [dbo].[Disciplines] ([DisciplineId]) ON DELETE CASCADE ON UPDATE NO ACTION
GO



