CREATE TABLE [dbo].[RefereeSalaryReports] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [UserId]    INT            NOT NULL,
    [SeasonId]  INT            NOT NULL,
    [StartDate] DATETIME       NOT NULL,
    [EndDate]   DATETIME       NOT NULL,
    [FileName]  NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_RefereeSalaryReports] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[RefereeSalaryReports] WITH NOCHECK
    ADD CONSTRAINT [FK_RefereeSalaryReports_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]);

ALTER TABLE [dbo].[RefereeSalaryReports] WITH NOCHECK
    ADD CONSTRAINT [FK_RefereeSalaryReports_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);

ALTER TABLE [dbo].[RefereeSalaryReports] WITH CHECK CHECK CONSTRAINT [FK_RefereeSalaryReports_Users];

ALTER TABLE [dbo].[RefereeSalaryReports] WITH CHECK CHECK CONSTRAINT [FK_RefereeSalaryReports_Seasons];