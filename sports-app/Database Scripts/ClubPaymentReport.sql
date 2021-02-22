ALTER TABLE dbo.Clubs ADD IsClubFeesPaid bit NOT NULL Default(0);
ALTER TABLE dbo.Clubs ADD IsClubManagerCanSeePayReport bit NOT NULL Default(0);