ALTER TABLE [dbo].[BicycleFriendshipPayments]
    ADD [Discarded]   BIT      CONSTRAINT [DF_BicycleFriendshipPayments_Discarded] DEFAULT ((0)) NOT NULL,
        [DiscardedBy] INT      NULL,
        [DiscardDate] DATETIME NULL;

ALTER TABLE [dbo].[Users] DROP COLUMN [TotalPrice];

ALTER TABLE [dbo].[BicycleFriendshipPayments] WITH NOCHECK
    ADD CONSTRAINT [FK_BicycleFriendshipPayments_DiscardUsers] FOREIGN KEY ([DiscardedBy]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[BicycleFriendshipPayments] WITH CHECK CHECK CONSTRAINT [FK_BicycleFriendshipPayments_DiscardUsers];