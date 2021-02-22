CREATE TABLE [dbo].[PlayerFiles] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [PlayerId]    INT            NOT NULL,
    [SeasonId]    INT            NULL,
    [FileType]    INT            NOT NULL,
    [FileName]    NVARCHAR (MAX) NOT NULL,
    [DateCreated] DATETIME       NOT NULL,
    CONSTRAINT [PK_PlayerFiles] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[RetirementRequests] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [UserId]           INT            NOT NULL,
    [TeamId]           INT            NOT NULL,
    [RequestDate]      DATETIME       NOT NULL,
    [Reason]           NVARCHAR (MAX) NOT NULL,
    [DocumentFileName] NVARCHAR (MAX) NOT NULL,
    [Approved]         BIT            NOT NULL,
    [ApprovedBy]       INT            NULL,
    [DateApproved]     DATETIME       NULL,
    [ApproveText]      NVARCHAR (MAX) NULL,
    [RefundAmount]     INT            NOT NULL,
    CONSTRAINT [PK_RetirementRequests] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[RetirementRequests]
    ADD CONSTRAINT [DF_RetirementRequests_Approved] DEFAULT ((0)) FOR [Approved];

ALTER TABLE [dbo].[RetirementRequests]
    ADD CONSTRAINT [DF_RetirementRequests_RefundAmount] DEFAULT ((0)) FOR [RefundAmount];

ALTER TABLE [dbo].[PlayerFiles] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerFiles_Users] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[PlayerFiles] WITH NOCHECK
    ADD CONSTRAINT [FK_PlayerFiles_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[RetirementRequests] WITH NOCHECK
    ADD CONSTRAINT [FK_RetirementRequests_Teams] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([TeamId]);

ALTER TABLE [dbo].[RetirementRequests] WITH NOCHECK
    ADD CONSTRAINT [FK_RetirementRequests_RequestUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[RetirementRequests] WITH NOCHECK
    ADD CONSTRAINT [FK_RetirementRequests_ApproveUser] FOREIGN KEY ([ApprovedBy]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[PlayerFiles] WITH CHECK CHECK CONSTRAINT [FK_PlayerFiles_Users];

ALTER TABLE [dbo].[PlayerFiles] WITH CHECK CHECK CONSTRAINT [FK_PlayerFiles_Seasons];

ALTER TABLE [dbo].[RetirementRequests] WITH CHECK CHECK CONSTRAINT [FK_RetirementRequests_Teams];

ALTER TABLE [dbo].[RetirementRequests] WITH CHECK CHECK CONSTRAINT [FK_RetirementRequests_RequestUser];

ALTER TABLE [dbo].[RetirementRequests] WITH CHECK CHECK CONSTRAINT [FK_RetirementRequests_ApproveUser];