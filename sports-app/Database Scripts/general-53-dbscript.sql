ALTER TABLE dbo.Leagues 
ADD 
	StartTeamRegistrationDate datetime NULL,
	MaxParticipationAllowedForSportsman int NULL

--

ALTER TABLE dbo.CompetitionDisciplines 
ADD 
	TeamRegistration int NULL

--

-- Create CompetitionDisciplineTeams 

IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[CompetitionDisciplineTeams]') AND type IN ('U'))
	DROP TABLE [dbo].[CompetitionDisciplineTeams]
GO

CREATE TABLE [dbo].[CompetitionDisciplineTeams] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [ClubId] int  NOT NULL,
  [CompetitionDisciplineId] int  NOT NULL,
  [TeamNumber] int  NULL
)
GO

ALTER TABLE [dbo].[CompetitionDisciplineTeams] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Primary Key structure for table CompetitionDisciplineTeams
-- ----------------------------
ALTER TABLE [dbo].[CompetitionDisciplineTeams] ADD CONSTRAINT [PK__CompDiscTeams] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Foreign Keys structure for table CompetitionDisciplineTeams
-- ----------------------------
ALTER TABLE [dbo].[CompetitionDisciplineTeams] ADD CONSTRAINT [FKEY_COMPDISCTEAM_CLUBID] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[CompetitionDisciplineTeams] ADD CONSTRAINT [FKEY_COMPDISCTEAM_COMPDISC] FOREIGN KEY ([CompetitionDisciplineId]) REFERENCES [dbo].[CompetitionDisciplines] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO


-- Create CompetitionDisciplineTeams END

-- Add Foreign KEY 

ALTER TABLE dbo.CompetitionDisciplineRegistrations
ADD 
	CompetitionDisciplineTeamId int NULL

ALTER TABLE [dbo].[CompetitionDisciplineRegistrations] ADD CONSTRAINT [FK_COMPDISCREG_COMPDISCTEAM] FOREIGN KEY ([CompetitionDisciplineTeamId]) REFERENCES [dbo].[CompetitionDisciplineTeams] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

-- Add Foreign KEY END 

-- Add Coxwain flag

ALTER TABLE [dbo].[Disciplines]
ADD Coxwain bit NULL Default(0)

ALTER TABLE [dbo].[CompetitionDisciplineRegistrations]
ADD IsCoxwain bit NULL Default(0)

-- Add Coxwain flag END 


--CREATE CompetitionDisciplineClubsRegistration 

IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[CompetitionDisciplineClubsRegistration]') AND type IN ('U'))
	DROP TABLE [dbo].[CompetitionDisciplineClubsRegistration]
GO

CREATE TABLE [dbo].[CompetitionDisciplineClubsRegistration] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [ClubId] int  NOT NULL,
  [CompetitionDisciplineId] int  NOT NULL,
  [TeamRegistrations] int  NULL
)
GO

ALTER TABLE [dbo].[CompetitionDisciplineClubsRegistration] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Primary Key structure for table CompetitionDisciplineClubsRegistration
-- ----------------------------
ALTER TABLE [dbo].[CompetitionDisciplineClubsRegistration] ADD CONSTRAINT [PK__CompDiscClubReg] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Foreign Keys structure for table CompetitionDisciplineClubsRegistration
-- ----------------------------
ALTER TABLE [dbo].[CompetitionDisciplineClubsRegistration] ADD CONSTRAINT [FKEY_COMPDISCCLUBREG_CLUBID] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[CompetitionDisciplineClubsRegistration] ADD CONSTRAINT [FKEY_COMPDISCCLUBREG_COMPDISC] FOREIGN KEY ([CompetitionDisciplineId]) REFERENCES [dbo].[CompetitionDisciplines] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO
--CREATE CompetitionDisciplineClubsRegistration END 

