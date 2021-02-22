ALTER TABLE [dbo].[ActivityFormsSubmittedData]
    ADD [ApprovalDate] DATETIME NULL;

ALTER TABLE [dbo].[PlayerHistory]
    ADD [OldTeamId] INT NULL;

ALTER TABLE [dbo].[TeamsPlayers]
    ADD [ApprovalDate] DATETIME NULL;

ALTER TABLE [dbo].[PlayerHistory] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerHistory_OldTeams] FOREIGN KEY ([OldTeamId]) REFERENCES [dbo].[Teams] ([TeamId]);

ALTER TABLE [dbo].[PlayerHistory] WITH CHECK CHECK CONSTRAINT [FK_PlayerHistory_OldTeams];