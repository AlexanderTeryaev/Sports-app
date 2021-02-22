/*    ==Scripting Parameters==

    Source Server Version : SQL Server 2016 (13.0.4001)
    Source Database Engine Edition : Microsoft SQL Server Enterprise Edition
    Source Database Engine Type : Standalone SQL Server

    Target Server Version : SQL Server 2016
    Target Database Engine Edition : Microsoft SQL Server Enterprise Edition
    Target Database Engine Type : Standalone SQL Server
*/

USE [LogLig]
GO

/****** Object:  Table [dbo].[Statistics]    Script Date: 15.09.2017 20:35:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Statistics](
	[Id] [nvarchar](255) NOT NULL,
	[Abbreviation] [nvarchar](255) NULL,
	[Category] [nvarchar](255) NULL,
	[GameId] [int] NOT NULL,
	[Point_x] [float] NOT NULL,
	[Point_y] [float] NOT NULL,
	[Note] [nvarchar](255) NULL,
	[PlayerId] [int] NOT NULL,
	[ReporterId] [nvarchar](255) NULL,
	[SegmentTimeStamp] [int] NULL,
	[StatisticTypeId] [nvarchar](255) NULL,
	[SyncStatus] [nvarchar](255) NULL,
	[TeamId] [int] NULL,
	[TimeSegment] [int] NULL,
	[TimeSegmentName] [nvarchar](255) NULL,
	[Timestamp] [datetime2](7) NOT NULL,
	[GameTime] [bigint] NOT NULL,
 CONSTRAINT [PK_Statistics] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Statistics]  WITH CHECK ADD  CONSTRAINT [StatisticsTeamPlayers] FOREIGN KEY([PlayerId])
REFERENCES [dbo].[TeamsPlayers] ([Id])
GO

ALTER TABLE [dbo].[Statistics] CHECK CONSTRAINT [StatisticsTeamPlayers]
GO

