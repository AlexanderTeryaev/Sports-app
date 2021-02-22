CREATE TABLE dbo.ActivityFormsFiles
	(
	Id int NOT NULL IDENTITY (1, 1),
	ActivityId int NOT NULL,
	PropertyName nvarchar(MAX) NOT NULL,
	FileName nvarchar(MAX) NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.ActivityFormsFiles ADD CONSTRAINT
	PK_ActivityFormsFiles PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


ALTER TABLE dbo.ActivityFormsFiles ADD CONSTRAINT
	FK_ActivityFormsFiles_Activities FOREIGN KEY
	(
	ActivityId
	) REFERENCES dbo.Activities
	(
	ActivityId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 