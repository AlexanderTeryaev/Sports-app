ALTER TABLE dbo.UsersJobs 
ADD 
	Active bit NOT NULL DEFAULT(1), 
	FormatPermissions varchar(255) NULL