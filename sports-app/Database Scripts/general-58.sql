Alter table [dbo].[Unions]
Add
	ApprovePlayerByClubManagerFirst bit null Default(0)

Alter table [dbo].[TeamsPlayers]
Add 
	IsApprovedByClubManager bit null Default(0)


ALTER TABLE [dbo].[ActivityFormsSubmittedData] 
Add 
	PreviousClubId INT NULL,
	IsSeenByClubManager bit NULL Default(0)


ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD CONSTRAINT [FK_ActivityFormsSubmittedData_ClubsPrevious] FOREIGN KEY ([PreviousClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);