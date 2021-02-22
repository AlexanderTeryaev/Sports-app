USE [LogLig]
GO

DELETE FROM [dbo].[UsersDvices]
      WHERE ([dbo].[UsersDvices].SectionId IS NULL)
	  AND ([dbo].[UsersDvices].EndpointArn IS NULL)
GO


