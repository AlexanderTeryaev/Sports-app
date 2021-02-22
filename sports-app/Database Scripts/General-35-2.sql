CREATE TABLE [dbo].[OfficialGameReportDetails] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [UserJobId]      INT            NOT NULL,
    [GameCycleId]    INT            NOT NULL,
    [TravelDistance] INT            NULL,
    [Comment]        NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_OfficialGameReportDetails] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[OfficialGameReportDetails] WITH NOCHECK
    ADD CONSTRAINT [FK_OfficialGameReportDetails_UsersJobs] FOREIGN KEY ([UserJobId]) REFERENCES [dbo].[UsersJobs] ([Id]);

ALTER TABLE [dbo].[OfficialGameReportDetails] WITH NOCHECK
    ADD CONSTRAINT [FK_OfficialGameReportDetails_GamesCycles] FOREIGN KEY ([GameCycleId]) REFERENCES [dbo].[GamesCycles] ([CycleId]);

ALTER TABLE [dbo].[OfficialGameReportDetails] WITH CHECK CHECK CONSTRAINT [FK_OfficialGameReportDetails_UsersJobs];

ALTER TABLE [dbo].[OfficialGameReportDetails] WITH CHECK CHECK CONSTRAINT [FK_OfficialGameReportDetails_GamesCycles];