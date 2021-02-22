CREATE TABLE [dbo].[ResetPasswordRequests] (
    [Id]          INT              IDENTITY (1, 1) NOT NULL,
    [ResetGuid]   UNIQUEIDENTIFIER NOT NULL,
    [UserId]      INT              NOT NULL,
    [DateCreated] DATETIME         NOT NULL,
    [IsCompleted] BIT              NOT NULL,
    CONSTRAINT [PK_ResetPasswordRequests] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[ResetPasswordRequests] WITH NOCHECK
    ADD CONSTRAINT [FK_ResetPasswordRequests_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[ResetPasswordRequests] WITH CHECK CHECK CONSTRAINT [FK_ResetPasswordRequests_Users];