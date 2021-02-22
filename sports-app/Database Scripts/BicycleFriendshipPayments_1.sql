CREATE TABLE [dbo].[BicycleFriendshipPayments] (
    [Id]                      INT              IDENTITY (1, 1) NOT NULL,
    [UserId]                  INT              NOT NULL,
    [ClubId]                  INT              NULL,
    [TeamId]                  INT              NOT NULL,
    [SeasonId]                INT              NULL,
    [FriendshipPrice]         DECIMAL (18, 2)  NOT NULL,
    [ChipPrice]               DECIMAL (18, 2)  NOT NULL,
    [UciPrice]                DECIMAL (18, 2)  NOT NULL,
    [LogLigPaymentId]         UNIQUEIDENTIFIER NOT NULL,
    [OfficeGuyCustomerId]     BIGINT           NULL,
    [OfficeGuyPaymentId]      BIGINT           NULL,
    [OfficeGuyDocumentNumber] INT              NULL,
    [IsPaid]                  BIT              NOT NULL,
    [DateCreated]             DATETIME         NOT NULL,
    [DatePaid]                DATETIME         NULL,
    CONSTRAINT [PK_BicycleFriendshipPayments] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[BicycleFriendshipPayments]
    ADD CONSTRAINT [DF_BicycleFriendshipPayments_IsPaid] DEFAULT ((0)) FOR [IsPaid];

ALTER TABLE [dbo].[BicycleFriendshipPayments] WITH NOCHECK
    ADD CONSTRAINT [FK_BicycleFriendshipPayments_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[BicycleFriendshipPayments] WITH NOCHECK
    ADD CONSTRAINT [FK_BicycleFriendshipPayments_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);

ALTER TABLE [dbo].[BicycleFriendshipPayments] WITH NOCHECK
    ADD CONSTRAINT [FK_BicycleFriendshipPayments_Teams] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]);

ALTER TABLE [dbo].[BicycleFriendshipPayments] WITH NOCHECK
    ADD CONSTRAINT [FK_BicycleFriendshipPayments_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[BicycleFriendshipPayments] WITH CHECK CHECK CONSTRAINT [FK_BicycleFriendshipPayments_Users];

ALTER TABLE [dbo].[BicycleFriendshipPayments] WITH CHECK CHECK CONSTRAINT [FK_BicycleFriendshipPayments_Clubs];

ALTER TABLE [dbo].[BicycleFriendshipPayments] WITH CHECK CHECK CONSTRAINT [FK_BicycleFriendshipPayments_Teams];

ALTER TABLE [dbo].[BicycleFriendshipPayments] WITH CHECK CHECK CONSTRAINT [FK_BicycleFriendshipPayments_Seasons];