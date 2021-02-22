using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using CmsApp.Models;
using DataService;

namespace CmsApp.Controllers
{
    public class SearchController : AdminController
    {
        SearchService searchServ = new SearchService();
        TeamsRepo teamsRepo = new TeamsRepo();
        UsersRepo userRepo = new UsersRepo();

        // GET: Search
        public ActionResult Index()
        {
            var vm = new SearchViewModel();
            vm.IsSearchLeagueVisible = true;
            vm.IsSearchTeamVisible = true;
            vm.IsSearchPlayerVisible = true;

            if (User.IsInRole(AppRole.Workers))
            {
                var usersRepo = new UsersRepo();
                int userId = base.AdminId;
                var roleName = usersRepo.GetTopLevelJob(userId);
                switch (roleName)
                {
                    case JobRole.LeagueManager:
                        vm.IsSearchLeagueVisible = false;
                        break;
                    case JobRole.TeamManager:
                        vm.IsSearchLeagueVisible = false;
                        vm.IsSearchTeamVisible = false;
                        break;
                    case JobRole.ClubManager:
                    case JobRole.ClubSecretary:
                        vm.IsSearchLeagueVisible = false;
                        break;
                }
            }
            return View(vm);
        }

        [HttpPost]
        public ActionResult FindTeam(string term, int? sectionId, int? departmentId, int? seasonId, int? clubId)
        {
            IEnumerable<ListItemDto> items;
            var roleName = usersRepo.GetTopLevelJob(AdminId);

            var isDepartmentClub = clubId.HasValue && clubsRepo.GetById(clubId.Value)?.ParentClub != null;

           switch (roleName)
            {
                case JobRole.UnionManager:
                    var unionId = jobsRepo.GetUnionIdByUnionManagerId(AdminId);
                    seasonId = seasonId ?? GetUnionCurrentSeasonFromSession();// seasonsRepository.GetCurrentByUnionId(unionId.Value);
                    items = unionsRepo.FindTeamsByNameAndSection(term, sectionId, 999, unionId, seasonId);
                    break;
                case JobRole.LeagueManager:
                    var leagueId = jobsRepo.GetLeagueIdByLeagueManagerId(AdminId);
                    items = leagueRepo.FindTeamsByNameAndSection(term, sectionId, 999, leagueId, seasonId);
                    break;
                case JobRole.ClubManager:
                case JobRole.ClubSecretary:
                    clubId = jobsRepo.GetClubIdByClubManagerId(AdminId);
                    items = teamsRepo.FindByNameAndSection(term, sectionId, 999, seasonId, clubId);
                    break;
                case JobRole.DepartmentManager:
                    items = teamsRepo.FindByDepartmentId(term, 999, departmentId, seasonId);
                    break;
                default:
                    items = departmentId == null
                        ? teamsRepo.FindByNameAndSection(term, sectionId, 999, seasonId,
                            User.IsInAnyRole(AppRole.Admins) ? null : clubId, isDepartmentClub)
                        : teamsRepo.FindByDepartmentId(term, 999, departmentId, seasonId);
                    break;
            }

            return Json(items.OrderBy(x => x.Name).ToList());
        }

        [HttpPost]
        public ActionResult FindLeague(string term)
        {
            IEnumerable<ListItemDto> items;
            var roleName = usersRepo.GetTopLevelJob(AdminId);

            if (roleName == JobRole.UnionManager)
            {
                var unionId = jobsRepo.GetUnionIdByUnionManagerId(AdminId);
                //var seasonId = seasonsRepository.GetCurrentByUnionId(unionId ?? 0);
                var seasonId = GetUnionCurrentSeasonFromSession();
                items = leagueRepo.FindByName(term, 999, unionId, seasonId);
            }
            else
            {
                items = leagueRepo.FindByName(term, 999);
            }

            return Json(items);
        }

        [HttpPost]
        public ActionResult FindPlayer(string term)
        {
            var roleName = usersRepo.GetTopLevelJob(AdminId);

            int? unionId = null, leagueId = null, clubId = null, teamId = null, seasonId = null;

            switch (roleName)
            {
                case JobRole.UnionManager:
                    unionId = jobsRepo.GetUnionIdByUnionManagerId(AdminId);
                    seasonId = GetUnionCurrentSeasonFromSession();
                    break;
                case JobRole.LeagueManager:
                    leagueId = jobsRepo.GetLeagueIdByLeagueManagerId(AdminId);
                    seasonId = GetUnionCurrentSeasonFromSession();
                    break;
                case JobRole.ClubManager:
                case JobRole.ClubSecretary:
                    clubId = jobsRepo.GetClubIdByClubManagerId(AdminId);
                    seasonId = getCurrentSeason()?.Id;
                    break;
                case JobRole.TeamManager:
                    teamId = jobsRepo.GetTeamIdByTeamManagerId(AdminId);
                    seasonId = getCurrentSeason()?.Id;
                    break;
            }

            var items = userRepo.SearchPlayersWithTeamAndSeason(AppRole.Players, term, 999, teamId, leagueId, unionId, clubId, seasonId);

            return Json(items);
        }

        [HttpPost]
        public ActionResult FindPlayerByNav(string term)
        {
            var roleName = usersRepo.GetTopLevelJob(AdminId);

            int? unionId = null, leagueId = null, clubId = null, teamId = null, seasonId = null;

            switch (roleName)
            {
                case JobRole.UnionManager:
                    unionId = jobsRepo.GetUnionIdByUnionManagerId(AdminId);
                    seasonId = GetUnionCurrentSeasonFromSession();
                    break;
                case JobRole.LeagueManager:
                    leagueId = jobsRepo.GetLeagueIdByLeagueManagerId(AdminId);
                    seasonId = GetUnionCurrentSeasonFromSession();
                    break;
                case JobRole.ClubManager:
                case JobRole.ClubSecretary:
                    clubId = jobsRepo.GetClubIdByClubManagerId(AdminId);
                    seasonId = getCurrentSeason()?.Id;

                    clubId = jobsRepo.GetClubIdByClubManagerIdForNavSearch(AdminId, seasonId);                 
                    break;
                case JobRole.TeamManager:
                    teamId = jobsRepo.GetTeamIdByTeamManagerId(AdminId);
                    seasonId = getCurrentSeason()?.Id;
                    break;
            }

            var items = userRepo.SearchPlayersWithTeamAndSeasonByNav(AppRole.Players, term, 999, teamId, leagueId, unionId, clubId, seasonId);
            var unionlessSeason = getUnionlessCurrentSeason();
            if (unionlessSeason != null && unionlessSeason.Id != seasonId)
            {
                var club2Id = jobsRepo.GetClubIdByClubManagerIdForNavSearch(AdminId, unionlessSeason.Id);
                items.AddRange(userRepo.SearchPlayersWithTeamAndSeasonByNav(AppRole.Players, term, 999, teamId, leagueId, unionId, club2Id, unionlessSeason.Id));
            }
            return Json(items);
        }
    }
}