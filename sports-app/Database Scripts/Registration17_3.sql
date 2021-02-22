CREATE TABLE [dbo].[ActivityStatusColumnsSorting] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [ActivityId] INT            NOT NULL,
    [UserId]     INT            NOT NULL,
    [Sorting]    NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_ActivityStatusColumnsSorting] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[ActivityStatusColumnsSorting] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnsSorting_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]);

ALTER TABLE [dbo].[ActivityStatusColumnsSorting] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnsSorting_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[ActivityStatusColumnsSorting] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnsSorting_Activities];

ALTER TABLE [dbo].[ActivityStatusColumnsSorting] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnsSorting_Users];