
GO
ALTER TABLE [dbo].[Auditoriums]
    ADD [SeasonId] INT NULL;


GO
PRINT N'Altering [dbo].[UsersJobs]...';


GO
ALTER TABLE [dbo].[UsersJobs]
    ADD [SeasonId] INT NULL;


GO
PRINT N'Creating [dbo].[FK_Auditoriums_Seasons]...';


GO
ALTER TABLE [dbo].[Auditoriums] WITH NOCHECK
    ADD CONSTRAINT [FK_Auditoriums_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


GO
PRINT N'Creating [dbo].[FK_UsersJobs_Seasons]...';


GO
ALTER TABLE [dbo].[UsersJobs] WITH NOCHECK
    ADD CONSTRAINT [FK_UsersJobs_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]);


GO
PRINT N'Checking existing data against newly created constraints';

GO
ALTER TABLE [dbo].[Auditoriums] WITH CHECK CHECK CONSTRAINT [FK_Auditoriums_Seasons];

ALTER TABLE [dbo].[UsersJobs] WITH CHECK CHECK CONSTRAINT [FK_UsersJobs_Seasons];


GO
PRINT N'Update complete.';


GO
