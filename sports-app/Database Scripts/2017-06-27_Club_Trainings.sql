ALTER TABLE ClubTeams ADD IsBlocked BIT NOT NULL DEFAULT 0;
GO
CREATE TABLE ClubTrainingDays(
	Id int IDENTITY(1,1) NOT NULL,
	ClubId int NOT NULL,
	AuditoriumId int NOT NULL,
	TrainingDay nvarchar(100) NOT NULL,
	TrainingStartTime nvarchar(100) NOT NULL,
	TrainingEndTime nvarchar(100) NOT NULL,
	IsArchive bit NOT NULL)
GO
ALTER TABLE ClubTrainingDays
ADD CONSTRAINT PK_Club_Trainings_Id PRIMARY KEY (Id);
GO
ALTER TABLE ClubTrainingDays
ADD CONSTRAINT FK_Club_Id FOREIGN KEY (ClubId) REFERENCES Clubs(ClubId);
GO
ALTER TABLE ClubTrainingDays
ADD CONSTRAINT FK_Auditorium_Id FOREIGN KEY (AuditoriumId) REFERENCES Auditoriums(AuditoriumId);
GO
ALTER TABLE ClubTeams ADD TeamPosition INT NOT NULL DEFAULT 0
GO
ALTER TABLE TrainingSettings ADD NoDayAfterDayTrainings BIT NOT NULL DEFAULT 0
GO
ALTER TABLE Clubs ADD IsTrainingEnabled BIT NOT NULL DEFAULT 0;