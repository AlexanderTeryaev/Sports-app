
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'ActivitiesUsers'))
BEGIN
    drop table ActivitiesUsers 
END


IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'ActivitiesPrices'))
BEGIN
    drop table ActivitiesPrices
end


IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'ActivitiesLeagues'))
BEGIN
    drop table ActivitiesLeagues
end



IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'Activities'))
BEGIN
    drop table Activities
end

IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'ActivityBranches'))
BEGIN
    drop table ActivityBranches
end


IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'LeaguesPrices'))
BEGIN
    drop table LeaguesPrices
end


delete uj 
    from UsersJobs uj 
        inner join Jobs j  on j.JobId = uj.JobId
        inner join JobsRoles jr on  jr.RoleId = j.RoleId
    where jr.RoleName in ('activitymanager', 'activityviewer')
GO

delete j 
    from Jobs j 
        inner join JobsRoles jr on  jr.RoleId = j.RoleId
    where jr.RoleName in ('activitymanager', 'activityviewer')
GO

delete FROM [dbo].[JobsRoles] where RoleName in ('activitymanager', 'activityviewer')
GO