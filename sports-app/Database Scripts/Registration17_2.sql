CREATE TABLE [dbo].[ActivityStatusColumnNames] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [ActivityId]  INT            NOT NULL,
    [UserId]      INT            NOT NULL,
    [ColumnIndex] INT            NOT NULL,
    [ColumnName]  NVARCHAR (MAX) NOT NULL,
    [Language]    NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_ActivityStatusColumnNames] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[ActivityStatusColumnNames] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnNames_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]);

ALTER TABLE [dbo].[ActivityStatusColumnNames] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnNames_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[ActivityStatusColumnNames] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnNames_Activities];

ALTER TABLE [dbo].[ActivityStatusColumnNames] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnNames_Users];