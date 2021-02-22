using DataService;
using System;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using AppModel;
using System.Collections.Generic;
using DataService.DTO;

namespace CmsApp.Helpers
{
    public enum PageType
    {
        Default = 0,
        Union, 
        League,
        Club,
        Team,
        UnionClubs //union/clubs for individual sections
    }

    public class PageHelper
    {
        private ClubsRepo clubsRepo;
        private JobsRepo jobsRepo;
        private UsersRepo usersRepo;
        private TeamsRepo teamRepo;
        private LeagueRepo leagueRepo;
        private UnionsRepo unionsRepo;

        public PageHelper()
        {
            clubsRepo = new ClubsRepo();
            jobsRepo = new JobsRepo();
            usersRepo = new UsersRepo();
            teamRepo = new TeamsRepo();
            leagueRepo = new LeagueRepo();
            unionsRepo = new UnionsRepo();
        }

        public string GenerateUrl(string roleName, string sectionAlias, int? unionId, int? clubId, int? leagueId,
            int? disciplineId, int? teamId, int? seasonId, Regional region, int userId)
        {
            var urlHelper = new UrlHelper(HttpContext.Current?.Request.RequestContext);

            var isTennis = string.Equals(sectionAlias, SectionAliases.Tennis,
                StringComparison.CurrentCultureIgnoreCase);

            switch (roleName)
            {
                case JobRole.RegionalManager:
                    return urlHelper.Action("Edit", "Regional", new { id = region?.RegionalId ?? 0, seasonId = seasonId ?? 0, unionId = region?.UnionId ?? 0 });

                case JobRole.UnionManager:
                case JobRole.Unionviewer:
                case JobRole.RefereeAssignment:
                    var url0 = $"/Unions/Edit/{unionId}";
                    return $"{url0}?roleType={roleName}";

                case JobRole.LeagueManager:
                case JobRole.CallRoomManager:
                    var league = leagueRepo.GetById(leagueId ?? 0);

                    var url1 = $"/Leagues/Edit/{leagueId}?seasonId={league?.SeasonId ?? seasonId}{(isTennis && league?.EilatTournament == true ? "&isTennisCompetition=1" : string.Empty)}";
                    return $"{url1}&roleType={roleName}";

                case JobRole.TeamManager:
                case JobRole.TeamViewer:
                    var teamManagers = teamRepo.GetByTeamManagerId(userId);                    
                    var teamManager = teamManagers.FirstOrDefault(c => c.TeamId == teamId && c.SeasonId == seasonId);

                    if (teamManager?.LeagueId != null)
                    {
                        var url2 = $"/Teams/Edit/{teamManager.TeamId}?currentLeagueId={teamManager.LeagueId}&seasonId={teamManager.SeasonId}&unionId={teamManager.UnionId}";
                        return $"{url2}&roleType={roleName}";
                    }

                    if (teamManager?.ClubId != null)
                    {
                        var teamManagerClub = clubsRepo.GetById(teamManager.ClubId ?? 0);
                        if (teamManagerClub?.Section != null)
                        {
                            var url3 =  $"/Teams/Edit/{teamManager.TeamId}?clubId={teamManagerClub.ClubId}&seasonId={seasonId}&sectionId={teamManagerClub.SectionId}";
                            return $"{url3}&roleType={roleName}";
                        }
                        if (teamManagerClub?.Union != null)
                        {
                            var url4 = $"/Teams/Edit/{teamManager.TeamId}?clubId={teamManagerClub.ClubId}&seasonId={teamManagerClub.SeasonId}&unionId={teamManagerClub.UnionId}";
                            return $"{url4}&roleType={roleName}";
                        }
                    }
                    return string.Empty;

                case JobRole.CommitteeOfReferees:
                    var userUnions = unionsRepo.GetByManagerId(userId);
                    if (userUnions.Count == 1)
                    {
                        var seasons = userUnions.First().Seasons.Where(x => x.IsActive).ToList();
                        var season = 1;
                        if (seasons.Count > 0)
                        {
                            season = seasons.Last().Id;
                        }
                        /*
                        var unblockaded =
                            GetAllUnblockadedPlayers(userUnions.FirstOrDefault().UnionId, season); //TODO: Perfomance
                        Session["UnblockadedPlayers"] = unblockaded;

                        SetUnionCurrentSeason(season);

                        return RedirectToAction("EditReferees", "Unions", new { id = userUnions.First().UnionId, seasonId = season });
                        */
                        var url55 = $"/Unions/EditReferees/{userUnions.First().UnionId}?seasonId={season}";
                        return $"{url55}&roleType={roleName}";
                    }
                    return string.Empty;

                case JobRole.DisciplineManager:
                    var url5 = $"/Disciplines/Edit/{disciplineId}&seasonId={seasonId}";
                    return $"{url5}&roleType={roleName}";

                case JobRole.ClubManager:
                case JobRole.ClubSecretary:
                    if (clubId.HasValue)
                    {
                        var club = clubsRepo.GetById(clubId.Value);

                        //TODO: commented changes below might be not needed at all, as the season id should be directly from UserJobs
                        //var cseasonId = club?.SeasonId;
                        //if (!club.SeasonId.HasValue)
                        //{
                        //    cseasonId = club.Seasons.FirstOrDefault()?.Id;
                        //}

                        if (club != null)
                        {
                            var url6 = club?.UnionId != null
                                ? $"/Clubs/Edit/{club.ClubId}?seasonId={seasonId}&unionId={club.UnionId}&showAlerts=true"
                                : $"/Clubs/Edit/{club.ClubId}?seasonId={seasonId}&sectionId={club.SectionId}&showAlerts=true";
                            return $"{url6}&roleType={roleName}";
                        }
                    }
                    return string.Empty;

                case JobRole.DepartmentManager:
                    var department = clubsRepo.GetById(clubId.Value);
                    if (department != null)
                    {
                        var url7 = $"/Clubs/Edit/{department.ClubId}?seasonId={seasonId}&sportId={department?.SportSectionId}&isDepartment=True";
                        return $"{url7}&roleType={roleName}";
                    }
                    return string.Empty;

                case JobRole.Referee:
                case JobRole.Activityviewer:
                case JobRole.Activitymanager:
                case JobRole.ActivityRegistrationActive:
                    var activitiesAllowed = jobsRepo.IsActivityManager(userId) ||
                                            jobsRepo.IsActivityViewer(userId) ||
                                            jobsRepo.IsActivityRegistrationActive(userId);

                    var user = usersRepo.GetById(userId);
                    var leagueTemp = leagueRepo.GetById(leagueId ?? 0);
                    if (leagueTemp != null && sectionAlias == SectionAliases.Athletics && roleName == JobRole.Referee)                    
                            return $"/Leagues/Edit/{leagueId}?seasonId={leagueTemp?.SeasonId ?? seasonId}{(isTennis && leagueTemp?.EilatTournament == true ? "&isTennisCompetition=1" : string.Empty)}";

                    if (activitiesAllowed)
                    {
                        //var userJobs = jobsRepo.GetUsersJobCollection(x => x.UserId == userId &&
                        //                                                  (x.Job.JobsRole.RoleName == JobRole.Activitymanager ||
                        //                                                   x.Job.JobsRole.RoleName == JobRole.Activityviewer ||
                        //                                                   x.Job.JobsRole.RoleName == JobRole.ActivityRegistrationActive ||
                        //                                                   x.Job.JobsRole.RoleName == JobRole.Referee &&
                        //                                                   x.Season.IsActive)
                        //);

                        //var lastSeason = userJobs.OrderBy(x => x.SeasonId).LastOrDefault();
                        //if (lastSeason != null)
                        return $"/Activity/ActivityList?seasonId={seasonId}&roleType={roleName}";
                    }

                    if (activitiesAllowed)
                    {
                        var url9 = $"/Activity/ActivityList";
                        return $"{url9}?roleType={roleName}";
                    }
                    else
                    {
                        if (roleName == JobRole.Referee)
                        {
                            var tennisLeagues = jobsRepo.GetAllTennisLeagues(userId);
                            if (tennisLeagues.Count > 0)
                            {
                                if (tennisLeagues.Count == 1)
                                {
                                    var leagueRef = tennisLeagues.First();
                                    var url99 = $"/Leagues/Edit/{leagueRef.LeagueId}?SeasonId={leagueRef.SeasonId}";
                                    return $"{url99}?roleType={roleName}";
                                }
                                var url9 = $"/WorkerHome/RefereeLeagues";
                                return $"{url9}?roleType={roleName}";
                            }
                        }
                        return string.Empty;
                    }

                default:
                    return string.Empty;
            }
        }
    }
}