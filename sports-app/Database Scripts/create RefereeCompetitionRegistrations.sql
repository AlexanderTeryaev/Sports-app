/*
 Navicat Premium Data Transfer

 Source Server         : Local_mssql
 Source Server Type    : SQL Server
 Source Server Version : 14001000
 Source Host           : localhost:1433
 Source Catalog        : LogLig
 Source Schema         : dbo

 Target Server Type    : SQL Server
 Target Server Version : 14001000
 File Encoding         : 65001

 Date: 02/02/2019 05:22:16
*/


-- ----------------------------
-- Table structure for RefereeCompetitionRegistrations
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[RefereeCompetitionRegistrations]') AND type IN ('U'))
	DROP TABLE [dbo].[RefereeCompetitionRegistrations]
GO

CREATE TABLE [dbo].[RefereeCompetitionRegistrations] (
  [Id] int  IDENTITY(1,1) NOT NULL,
  [UserId] int  NOT NULL,
  [SeasonId] int  NOT NULL,
  [UnionId] int  NOT NULL,
  [LeagueId] int  NULL,
  [SessionId] int  NULL
)
GO

ALTER TABLE [dbo].[RefereeCompetitionRegistrations] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Primary Key structure for table RefereeCompetitionRegistrations
-- ----------------------------
ALTER TABLE [dbo].[RefereeCompetitionRegistrations] ADD CONSTRAINT [PK__RefereeC__3214EC07A6F3F542] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Foreign Keys structure for table RefereeCompetitionRegistrations
-- ----------------------------
ALTER TABLE [dbo].[RefereeCompetitionRegistrations] ADD CONSTRAINT [FKEY_REFEREEREG_USERID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[RefereeCompetitionRegistrations] ADD CONSTRAINT [FKEY_REFEREEREG_UNIONID] FOREIGN KEY ([UnionId]) REFERENCES [dbo].[Unions] ([UnionId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[RefereeCompetitionRegistrations] ADD CONSTRAINT [FKEY_REFEREEREG_LEAGUEID] FOREIGN KEY ([LeagueId]) REFERENCES [dbo].[Leagues] ([LeagueId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

