ALTER TABLE dbo.Users 
ADD 
	IsNationalSportsman bit NULL DEFAULT(0),
	ShoesSize int NULL,
	ArmyDraftDate datetime NULL,
	MedicalInformation nvarchar(MAX) NULL,
	AuditoriumId int NULL

ALTER TABLE [dbo].[Users] ADD CONSTRAINT [FK_Users_Auditorium] FOREIGN KEY ([AuditoriumId]) REFERENCES [dbo].[Auditoriums] ([AuditoriumId]) ON DELETE NO ACTION ON UPDATE NO ACTION
GO