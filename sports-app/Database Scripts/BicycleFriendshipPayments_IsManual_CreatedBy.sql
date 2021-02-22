ALTER TABLE [dbo].[BicycleFriendshipPayments]
    ADD [IsManual]  BIT CONSTRAINT [DF_BicycleFriendshipPayments_IsManual] DEFAULT ((0)) NOT NULL,
        [CreatedBy] INT NULL;

ALTER TABLE [dbo].[BicycleFriendshipPayments] WITH NOCHECK
    ADD CONSTRAINT [FK_BicycleFriendshipPayments_CreatedByUsers] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[BicycleFriendshipPayments] WITH CHECK CHECK CONSTRAINT [FK_BicycleFriendshipPayments_CreatedByUsers];