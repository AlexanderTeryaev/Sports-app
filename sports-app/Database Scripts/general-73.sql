-- 1. Add IsHidden categories flag
ALTER TABLE [dbo].[CompetitionAges]
Add 
	IsHidden bit null Default(0)

ALTER TABLE [dbo].[Users]
Add 
	IsReligious bit null Default(0)