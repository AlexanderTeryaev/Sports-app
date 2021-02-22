CREATE TABLE [dbo].[ActivityStatusColumnsVisibility] (
    [Id]          INT IDENTITY (1, 1) NOT NULL,
    [ActivityId]  INT NOT NULL,
    [UserId]      INT NOT NULL,
    [ColumnIndex] INT NOT NULL,
    [Visible]     BIT NOT NULL,
    CONSTRAINT [PK_ActivityStatusColumnsVisibility] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[ActivityStatusColumnsVisibility] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnsVisibility_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[ActivityStatusColumnsVisibility] WITH NOCHECK
    ADD CONSTRAINT [FK_ActivityStatusColumnsVisibility_Activities] FOREIGN KEY ([ActivityId]) REFERENCES [dbo].[Activities] ([ActivityId]);

ALTER TABLE [dbo].[UsersJobs] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersJobs_Clubs] FOREIGN KEY ([ClubId]) REFERENCES [dbo].[Clubs] ([ClubId]);

ALTER TABLE [dbo].[ActivityStatusColumnsVisibility] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnsVisibility_Users];

ALTER TABLE [dbo].[ActivityStatusColumnsVisibility] WITH CHECK CHECK CONSTRAINT [FK_ActivityStatusColumnsVisibility_Activities];

ALTER TABLE [dbo].[UsersJobs] WITH CHECK CHECK CONSTRAINT [FK_UsersJobs_Clubs];