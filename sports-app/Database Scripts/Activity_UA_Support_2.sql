ALTER TABLE [dbo].[Users] DROP CONSTRAINT [DF__Users__TestResul__1699586C];

ALTER TABLE [dbo].[Users] DROP CONSTRAINT [DF__Users__IsAthlete__6621099A];

ALTER TABLE [dbo].[PenaltyForExclusion] DROP CONSTRAINT [FK__PenaltyFo__UserI__3CFEF876];

ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK__Users__BlockadeI__74643BF9];

ALTER TABLE [dbo].[Users] ALTER COLUMN [IdentNum] NVARCHAR (50) NULL;

ALTER TABLE [dbo].[Users]
    ADD CONSTRAINT [DF__Users__IsAthlete__6621099A] DEFAULT ((0)) FOR [IsAthleteNumberProduced];

ALTER TABLE [dbo].[Users]
    ADD CONSTRAINT [DF__Users__TestResul__1699586C] DEFAULT ((0)) FOR [TestResults];

ALTER TABLE [dbo].[PenaltyForExclusion] WITH NOCHECK
    ADD CONSTRAINT [FK__PenaltyFo__UserI__3CFEF876] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[Users] WITH NOCHECK
    ADD CONSTRAINT [FK__Users__BlockadeI__74643BF9] FOREIGN KEY ([BlockadeId]) REFERENCES [dbo].[PlayersBlockade] ([Id]);

ALTER TABLE [dbo].[PenaltyForExclusion] WITH CHECK CHECK CONSTRAINT [FK__PenaltyFo__UserI__3CFEF876];

ALTER TABLE [dbo].[Users] WITH CHECK CHECK CONSTRAINT [FK__Users__BlockadeI__74643BF9];