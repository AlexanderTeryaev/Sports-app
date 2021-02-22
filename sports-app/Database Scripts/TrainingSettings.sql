USE [LogLig]
GO

/****** Object:  Table [dbo].[TrainingSettings]    Script Date: 03/28/2017 7:37:48 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[TrainingSettings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DurationTraining] [varchar](100) NOT NULL,
	[ConsiderationHolidays] [bit] NOT NULL,
	[TrainingSameDay] [bit] NOT NULL,
	[TrainingBeforeGame] [bit] NOT NULL,
	[MinNumTrainingDays] [varchar](50) NOT NULL,
	[NoTwoTraining] [bit] NOT NULL,
	[TrainingFollowDay] [bit] NOT NULL,
	[TeamID] [int] NULL,
 CONSTRAINT [PK_TrainingSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[TrainingDaysSettings](
	[TrainingSettingsId] [int] IDENTITY(1,1) NOT NULL,
	[TeamId] [int] NOT NULL,
	[AuditoriumId] [int] NOT NULL,
	[TrainingDay] [nvarchar](100) NOT NULL,
	[TrainingStartTime] [nvarchar](100) NOT NULL,
	[TrainingEndTime] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TrainingSettingsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


ALTER TABLE Teams ADD IsTrainingEnabled BIT NOT NULL DEFAULT 0;

GO


CREATE TABLE TeamTrainings 
	(Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	 Title VARCHAR(128),
	 TeamId INT NOT NULL,
	 AuditoriumId INT,
	 TrainingDate DATETIME NOT NULL,
	 Content VARCHAR(128) 
	 );
GO

ALTER TABLE TeamTrainings  
ADD CONSTRAINT TeamTrainings_TeamId_Fk FOREIGN KEY (TeamId)
REFERENCES Teams(TeamId);

GO

ALTER TABLE TeamTrainings 
ADD CONSTRAINT TeamTrainings_AuditoriumId_Fk FOREIGN KEY (AuditoriumId)
REFERENCES Auditoriums(AuditoriumId);

GO

CREATE TABLE TrainingAttendance ( 
	Id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
	TrainingId INT NOT NULL,
	PlayerId INT NOT NULL
);

GO

ALTER TABLE TrainingAttendance  
ADD CONSTRAINT TrainingAttendance_TrainingId_Fk FOREIGN KEY (TrainingId)
REFERENCES TeamTrainings(Id);

GO

ALTER TABLE TrainingAttendance  
ADD CONSTRAINT TrainingAttendance_PlayerId_Fk FOREIGN KEY (PlayerId)
REFERENCES TeamsPlayers(Id);

GO

ALTER TABLE TeamTrainings ADD isPublished BIT NOT NULL DEFAULT 0;