CREATE TABLE ClubBalances (
	Id INT IDENTITY(1,1) NOT NULL,
	   CONSTRAINT PK_ClubBalances_Id PRIMARY KEY CLUSTERED (Id),
	ClubId INT NOT NULL,
	ActionUserId INT NOT NULL,
	Income DECIMAL NULL,
	Expense DECIMAL NULL,
	Comment VARCHAR(MAX) NULL
)

ALTER TABLE ClubBalances WITH NOCHECK
    ADD CONSTRAINT [FK_ClubBalances_ClubId] FOREIGN KEY (ClubId) REFERENCES [dbo].[Clubs] ([ClubId]);

ALTER TABLE ClubBalances WITH NOCHECK
    ADD CONSTRAINT [FK_ClubBalances_ActionUserId] FOREIGN KEY (ActionUserId) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE ClubBalances ADD SeasonId INT NULL 
ALTER TABLE ClubBalances WITH NOCHECK
    ADD CONSTRAINT [FK_ClubBalances_SeasonId] FOREIGN KEY (SeasonId) REFERENCES [dbo].[Seasons] ([Id]);
