UPDATE [LogLig].[dbo].[Activities]
SET RegistrationPrice = 1, InsurancePrice = 1
WHERE [IsAutomatic] = 1 and 
	[Type] = 'personal' and
	[UnionId] is not null and
	[ClubId] is null