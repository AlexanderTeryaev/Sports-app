ALTER TABLE Clubs ADD MedicalSertificateFile NVARCHAR(MAX);
ALTER TABLE Unions ADD UnionFormTitle NVARCHAR(250);
ALTER TABLE Unions ADD UnionForm NVARCHAR(MAX);

CREATE TABLE NationalTeamInvitement (
	Id INT PRIMARY KEY IDENTITY NOT NULL,
	TeamPlayerId INT NOT NULL,
	StartDate DATETIME,
	EndDate DATETIME,
	CONSTRAINT FK_PersonOrder FOREIGN KEY (TeamPlayerId)
    REFERENCES TeamsPlayers(Id)
);

ALTER TABLE NationalTeamInvitement ADD SeasonId INT,
FOREIGN KEY (SeasonId) REFERENCES Seasons(Id); 

ALTER TABLE Users ADD PassportNum VARCHAR(20);