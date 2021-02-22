ALTER TABLE dbo.ActivityFormsSubmittedData ADD
	RegistrationPaid decimal(18, 2) NOT NULL CONSTRAINT DF_ActivityFormsSubmittedData_RegistrationPaid DEFAULT 0