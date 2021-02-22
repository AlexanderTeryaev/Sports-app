/*
 Navicat Premium Data Transfer

 Source Server         : localhost_mssql
 Source Server Type    : SQL Server
 Source Server Version : 14001000
 Source Host           : localhost:1433
 Source Catalog        : LogLig
 Source Schema         : dbo

 Target Server Type    : SQL Server
 Target Server Version : 14001000
 File Encoding         : 65001

 Date: 16/11/2019 00:20:39
*/


-- ----------------------------
-- Table structure for WaterpoloStatistics
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[WaterpoloStatistics]') AND type IN ('U'))
	DROP TABLE [dbo].[WaterpoloStatistics]
GO

CREATE TABLE [dbo].[WaterpoloStatistics] (
  [Id] bigint IDENTITY(1,1) NOT NULL,
  [GameId] int  NOT NULL,
  [PlayerId] int  NOT NULL,
  [MinutesPlayed] bigint  NULL,
  [GOAL] int  NULL,
  [PGOAL] int  NULL,
  [PMISS] int  NULL,
  [AST] int  NULL,
  [STL] int  NULL,
  [BLK] int  NULL,
  [TO] int  NULL,
  [OFFS] int  NULL,
  [FOUL] int  NULL,
  [EXC] int  NULL,
  [BFOUL] int  NULL,
  [SSAVE] int  NULL,
  [YC] int  NULL,
  [RD] int  NULL,
  [EFF] float(53)  NULL,
  [DIFF] float(53)  NULL,
  [TeamId] int  NULL,
  [Miss] int  NULL
)
GO

ALTER TABLE [dbo].[WaterpoloStatistics] SET (LOCK_ESCALATION = TABLE)
GO


-- ----------------------------
-- Primary Key structure for table WaterpoloStatistics
-- ----------------------------
ALTER TABLE [dbo].[WaterpoloStatistics] ADD CONSTRAINT [PK__Waterpolo_Statistics] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

-- ----------------------------
-- Foreign Keys structure for table WaterpoloStatistics
-- ----------------------------
ALTER TABLE [dbo].[WaterpoloStatistics] ADD CONSTRAINT [WaterpoloStatisticsTeamPlayers] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[TeamsPlayers] ([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[WaterpoloStatistics] ADD CONSTRAINT [WaterpoloStatisticsGameCycle] FOREIGN KEY ([GameId]) REFERENCES [dbo].[GamesCycles] ([CycleId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[WaterpoloStatistics] ADD CONSTRAINT [WaterpoloStatisticsTeam] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO

