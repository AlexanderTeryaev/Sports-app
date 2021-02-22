ALTER TABLE dbo.Activities ADD
	SecondTeamDiscount decimal(18, 2) NULL,
	SecondTeamNoInsurance bit NOT NULL CONSTRAINT DF_Activities_SecondTeamNoInsurance DEFAULT 1