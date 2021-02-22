ALTER TABLE TeamsPlayers ADD IsExceptionalMoved BIT NOT NULL DEFAULT 0

ALTER TABLE [dbo].[ClubTeams] ADD DepartmentId INT NULL

ALTER TABLE [dbo].[ClubTeams]
ADD CONSTRAINT FK_DepartmentId FOREIGN KEY (DepartmentId) REFERENCES Clubs(ClubId);
GO