IF EXISTS (SELECT 1 FROM sys.objects o
          INNER JOIN sys.columns c ON o.object_id = c.object_id
          WHERE o.name = 'UsersJobs' AND c.name = 'NoTravel')
ALTER TABLE [LogLig].[dbo].[UsersJobs] DROP COLUMN NoTravel;

ALTER TABLE [LogLig].[dbo].[UsersJobs] ADD NoTravel Bit Null Default(0)