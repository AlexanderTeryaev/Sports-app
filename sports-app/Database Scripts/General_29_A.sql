insert into [dbo].[JobsRoles] ([RoleId],[Title],[RoleName],[Priority]) values (16,'Referee assignment', 'referee_assignment', 10)
GO

alter table UnionOfficialSettings add RateCPerGame decimal(18, 2) null
GO

alter table UnionOfficialSettings add RateCForTravel decimal(18, 2) null
GO



-- Added 08.11.2018

alter table LeagueOfficialsSettings add RateCPerGame decimal(18, 2) null
GO

alter table LeagueOfficialsSettings add RateCForTravel decimal(18, 2) null
GO