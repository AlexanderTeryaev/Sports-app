USE [LogLig]
GO

CREATE TABLE [dbo].[TravelInformation] (
    [Id]       INT IDENTITY (1, 1) NOT NULL,
    [UserJobId]   INT NOT NULL,
    [FromHour] datetime null,
    [ToHour] datetime null,
    [IsUnionTravel] Bit Not Null Default(0),
    [NoTravel] Bit Null Default(0)
    CONSTRAINT [PK_TravelInformation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[TravelInformation] WITH CHECK ADD CONSTRAINT [FK_TravelInformation_UserJobs] FOREIGN KEY([UserJobId])
REFERENCES [dbo].[UsersJobs] ([Id])
GO

ALTER TABLE [dbo].[TravelInformation] CHECK CONSTRAINT [FK_TravelInformation_UserJobs]
GO

INSERT INTO [dbo].[TravelInformation] ([UserJobId], [FromHour], [ToHour], [IsUnionTravel], [NoTravel])
SELECT uj.[Id], DATETIMEFROMPARTS ( year(l.LeagueStartDate), month(l.LeagueStartDate), day(l.LeagueStartDate), 0, 0, 0, 0 ) + cast(uj.FromHour as time), DATETIMEFROMPARTS ( year(l.LeagueStartDate), month(l.LeagueStartDate), day(l.LeagueStartDate), 0, 0, 0, 0 ) + cast(uj.ToHour as time), uj.[IsUnionTravel], uj.[NoTravel] FROM [UsersJobs] uj left join leagues l on uj.LeagueId = l.LeagueId


ALTER TABLE [LogLig].[dbo].[UsersJobs] DROP CONSTRAINT DF__UsersJobs__IsUni__37F02A96
ALTER TABLE [LogLig].[dbo].[UsersJobs] DROP CONSTRAINT DF__UsersJobs__NoTra__487B981A

ALTER TABLE [LogLig].[dbo].[UsersJobs] DROP COLUMN FromHour;
ALTER TABLE [LogLig].[dbo].[UsersJobs] DROP COLUMN ToHour;
ALTER TABLE [LogLig].[dbo].[UsersJobs] DROP COLUMN IsUnionTravel;
ALTER TABLE [LogLig].[dbo].[UsersJobs] DROP COLUMN NoTravel;
GO
