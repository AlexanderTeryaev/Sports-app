ALTER TABLE [dbo].[PlayerDiscounts] ADD FinalParticipationPrice [decimal] NOT NULL DEFAULT 0
ALTER TABLE [dbo].[TeamsPlayers] ADD Comment varchar(255) NULL
ALTER TABLE [dbo].[TeamsPlayers] ADD Paid [decimal] NOT NULL DEFAULT 0
