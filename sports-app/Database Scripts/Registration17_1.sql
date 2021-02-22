CREATE TABLE [dbo].[ActivityStatusColumnsOrder] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [ActivityId] INT            NOT NULL,
    [UserId]     INT            NOT NULL,
    [Columns]    NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_ActivityStatusColumnsOrder] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[ActivityStatusColumnsOrder] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnsOrder_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]);

ALTER TABLE [dbo].[ActivityStatusColumnsOrder] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnsOrder_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[ActivityStatusColumnsOrder] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnsOrder_Activities];

ALTER TABLE [dbo].[ActivityStatusColumnsOrder] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnsOrder_Users];