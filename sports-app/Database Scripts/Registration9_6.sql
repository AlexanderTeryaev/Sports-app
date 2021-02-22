ALTER TABLE dbo.ActivityFormsDetails ADD
	CanBeRemoved bit NOT NULL CONSTRAINT DF_ActivityFormsDetails_CanBeRemoved DEFAULT 0,
	HasOptions bit NOT NULL CONSTRAINT DF_ActivityFormsDetails_HasOptions DEFAULT 0