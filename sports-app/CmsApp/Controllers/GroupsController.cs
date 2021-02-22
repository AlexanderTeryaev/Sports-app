using AppModel;
using CmsApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Omu.ValueInjecter;
using DataService.LeagueRank;
using Resources;
using System;
using DataService;
using CmsApp.Helpers;
using DataService.DTO;

namespace CmsApp.Controllers
{
    public class GroupsController : AdminController
    {
        // GET: Groups
        public ActionResult Index(int id, int seasonId, int? departmentId = null)
        {
            var resList = stagesRepo.GetAll(id, seasonId);
            CrossesStageHelper.SetNameForCrossesStages(resList);
            ViewBag.SeasonId = seasonId;
            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;
            var league = leagueRepo?.GetById(id);
            ViewBag.IsTennisLeague = leagueRepo.GetSectionAlias(id)?.Equals(GamesAlias.Tennis) == true
                && (league?.EilatTournament == null || league?.EilatTournament == false);
            return PartialView("_List", resList);
        }

        public ActionResult TennisIndex(int id, int seasonId, int? departmentId = null)
        {
            var resList = stagesRepo.GetAllTennisStages(id, seasonId);
            ViewBag.SeasonId = seasonId;
            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;

            return PartialView("_TennisList", resList);
        }

        public ActionResult Delete(int id, int seasonId)
        {
            var gr = groupRepo.GetById(id);
            int leagId = gr.Stage.LeagueId;
            gr.IsArchive = true;
            groupRepo.Save();
            return RedirectToAction("Index", new { id = leagId, seasonId });
        }

        public ActionResult DeleteTennis(int id, int seasonId)
        {
            var gr = groupRepo.GetTennisGroupById(id);
            int categoryId = gr.TennisStage.CategoryId;
            gr.IsArchive = true;
            groupRepo.Save();
            return RedirectToAction("TennisIndex", new { id = categoryId, seasonId });
        }

        public ActionResult Create(int id, int seasonId, int? departmentId = null)
        {
            var vm = new GroupsForm();
            vm.StageId = id;
            vm.SeasonId = seasonId;
            vm.NumberOfCycles = 1;
            var st = stagesRepo.GetById(id);
            vm.FirstStage = stagesRepo.CountStage(st.LeagueId) == 1;
            vm.LeagueId = st.LeagueId;
            UpdateGroupFormListsFromDB(vm, seasonId);
            vm.PointId = 2;
            ViewBag.SeasonId = seasonId;
            vm.SeasonId = seasonId;
            vm.IsIndividual = secRepo.GetByLeagueId(vm.LeagueId).IsIndividual;
            vm.Types.AddRange(new List<SelectListItem>
            {
                new SelectListItem{ Value = "0", Text = Messages.Teams, Selected = true },
                new SelectListItem{ Value = "1", Text = Messages.Athletes }
            });

            if (vm.IsIndividual)
                vm.Athtletes = new AthletesModel();
            if (vm.IsIndividual)
            {
                vm.Athtletes.WeightSelector = new List<SelectListItem>
                {
                    new SelectListItem{ Value = "Kg", Text = Messages.Kg, Selected = true},
                    new SelectListItem{ Value = "Lb", Text = Messages.Lb}
                };
            }
            vm.SectionAlias = secRepo.GetByLeagueId(vm.LeagueId).Alias;
            if (string.Equals(vm.SectionAlias, SectionAliases.MartialArts, StringComparison.CurrentCultureIgnoreCase))
            {
                var leagueSportType = unionsRepo.GetById((int)leagueRepo.GetById(vm.LeagueId).UnionId).Sport;
                vm.Athtletes.Ranks = SportsRepo.GetSportRanksBySportId(leagueSportType.Id).ToList();
            }

            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;
            return PartialView("_Edit", vm);
        }

        public ActionResult CreateTennisGroup(int leagueId, int categoryId, int id, int seasonId, int? departmentId = null)
        {
            var vm = new GroupsForm
            {
                StageId = id,
                SeasonId = seasonId,
                NumberOfCycles = 1,
                SectionAlias = secRepo.GetByLeagueId(leagueId).Alias,
                FirstStage = stagesRepo.CountTennisStage(id, seasonId) == 1,
                LeagueId = leagueId
            };
            UpdateTennisGroupFormListsFromDB(vm, categoryId);
            vm.PointId = 2;
            ViewBag.SeasonId = seasonId;
            vm.SeasonId = seasonId;
            vm.IsIndividual = secRepo.GetByLeagueId(vm.LeagueId).IsIndividual;
            vm.Types.AddRange(new List<SelectListItem>
            {
                //new SelectListItem{ Value = "0", Text = Messages.Teams, Selected = true },
                new SelectListItem { Value = "1", Text = Messages.Players },
                new SelectListItem { Value="2", Text = Messages.Pairs }
            });

            if (vm.IsIndividual)
                vm.Athtletes = new AthletesModel();

            if (string.Equals(vm.SectionAlias, SectionAliases.MartialArts, StringComparison.CurrentCultureIgnoreCase))
            {
                var leagueSportType = unionsRepo.GetById((int)leagueRepo.GetById(vm.LeagueId).UnionId).Sport;
                vm.Athtletes.Ranks = SportsRepo.GetSportRanksBySportId(leagueSportType.Id).ToList();
            }

            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;
            vm.CategoryId = categoryId;
            return PartialView("_TennisGroupEdit", vm);
        }

        public ActionResult Edit(int id, int? departmentId = null, int? seasonId = null)
        {
            var vm = new GroupsForm();

            var gr = groupRepo.GetById(id);
            vm.InjectFrom(gr);
            vm.IsIndividual = gr.IsIndividual;
            if (gr.NumberOfCycles.HasValue)
            {
                vm.NumberOfCycles = gr.NumberOfCycles.Value;
            }
            else
            {
                vm.NumberOfCycles = 1;
            }
            vm.Athtletes = new AthletesModel();
            vm.LeagueId = gr.Stage.LeagueId;
            UpdateGroupFormListsFromDB(vm, seasonId ?? 0);
            vm.GroupsTeams = groupRepo.GetTeamsGroups(vm.LeagueId);
            if (gr.IsIndividual)
            {
                vm.Types = new List<SelectListItem> { new SelectListItem { Value = "1", Text = Messages.Athletes } };
                vm.Athtletes.WeightSelector = new List<SelectListItem>
                {
                    new SelectListItem{ Value = "Kg", Text = Messages.Kg, Selected = true},
                    new SelectListItem{ Value = "Lb", Text = Messages.Lb}
                };
            }

            else
                vm.Types = new List<SelectListItem> { new SelectListItem { Value = "0", Text = Messages.Teams } };


            vm.IsIndividual = secRepo.GetByLeagueId(vm.LeagueId).IsIndividual;
            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;
            return PartialView("_Edit", vm);
        }

        public ActionResult EditTennis(int id, int? departmentId = null, int? seasonId = null)
        {
            var vm = new GroupsForm();

            var gr = groupRepo.GetTennisGroupById(id);
            vm.InjectFrom(gr);
            vm.IsIndividual = gr.IsIndividual;
            if (gr.NumberOfCycles.HasValue)
            {
                vm.NumberOfCycles = gr.NumberOfCycles.Value;
            }
            else
            {
                vm.NumberOfCycles = 1;
            }
            vm.Athtletes = new AthletesModel();
            vm.CategoryId = gr.TennisStage.CategoryId;
            UpdateTennisGroupFormListsFromDB(vm, vm.CategoryId.Value, seasonId ?? 0);

            vm.GroupsTeams = groupRepo.GetTennisTeamsGroups(vm.CategoryId.Value);

            if(gr.NumberOfPlayers != 0 && gr.NumberOfPlayers != gr.TennisGroupTeams.Count())
            {
                vm.GroupsTeams = gr.TennisGroupTeams.Select(gt => new GroupTeam
                {
                    GroupId = gr.GroupId,
                    TeamId = gt.TeamId ?? 0,
                    StageId = gr.StageId,
                    Title = gt.TeamsPlayer?.User?.FullName,
                    Pos = gt.Pos,
                    PlayerIdStr = gt?.PlayerId?.ToString() + (gt.PairPlayerId.HasValue
                        ? $"/{gt?.PairPlayerId?.ToString()}"
                        : string.Empty),
                }).OrderBy(gt => gt.Pos).ToList();
            }
            vm.Types = new List<SelectListItem> {
                new SelectListItem { Value = "1", Text = Messages.Players, Selected = !gr.IsPairs },
                new SelectListItem { Value = "2", Text = Messages.Pairs, Selected = gr.IsPairs },
            };

            vm.IsIndividual = true;
            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;
            return PartialView("_TennisGroupEdit", vm);
        }

        private void UpdateGroupFormListsFromDB(GroupsForm vm, int seasonId = 0)
        {
            var stageId = vm.StageId;
            var league = leagueRepo.GetByStage(stageId);
            var section = secRepo.GetByLeagueId(vm.LeagueId);

            var isIndividualSection = section?.IsIndividual == true;
            var isTennisEilatTournament = section?.Alias.Equals(GamesAlias.Tennis) == true &&
                (league?.EilatTournament == null || league?.EilatTournament == false);

            List<Team> leagueTeams = isTennisEilatTournament
                ? teamRepo.GetTennisLeagueTeams(vm.LeagueId, seasonId)
                : teamRepo.GetTeamsByLeague(vm.LeagueId);

            var groupeTeams = isTennisEilatTournament
                ? groupRepo.GetTennisLeagueGroupTeams(vm.LeagueId)
                    .Where(gt => gt.StageId == stageId)
                    .OrderBy(gt => gt.Pos)
                : groupRepo.GetTeamsGroups(vm.LeagueId)
                    .Where(gt => gt.StageId == stageId)
                    .OrderBy(gt => gt.Pos);

            var notIncludedTeams = leagueTeams.Where(t => !groupeTeams.Where(gt => gt.StageId == stageId)
                .Select(gt => gt.TeamId)
                .ToList()
                .Contains(t.TeamId))
                .ToList();
            var selectedTeams = new List<Team>();
            if (vm.GroupId != 0)
                selectedTeams = groupRepo.GetGroupTeamsByGroupId(vm.GroupId).OrderBy(gt => gt.Pos).Select(gt => gt.Team).ToList();

            var selectedAthletes = new List<TeamsPlayer>();
            if (vm.GroupId != 0)
                selectedAthletes = playersRepo.GetAthletesOfTheGroup(vm.GroupId);

            vm.TeamsList = new SelectList(notIncludedTeams
                .Select(x => new
                {
                    x.TeamId,
                    Title = x.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId)?.TeamName ?? x.Title
                }).ToList(),
                "TeamId", "Title");
            vm.SelectedTeamsList = new SelectList(selectedTeams
                    .Select(x => new
                    {
                        x?.TeamId,
                        Title = x?.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId)?.TeamName ?? x?.Title
                    }).ToList(),
                "TeamId", "Title");
            var athletesLists = new List<SelectListItem>();
            var selectedAthletesLists = new List<SelectListItem>();

            if (isIndividualSection == true)
            {
                var nonIncludedAthletes = GetAllNonIncludedAthletes(leagueTeams, vm.SeasonId, vm.StageId);
                foreach (var athlete in nonIncludedAthletes)
                {
                    athletesLists.Add(new SelectListItem
                    {
                        Value = athlete.Id.ToString(),
                        Text = $"{athlete.FullName} ({db.TeamsPlayers.Find(athlete.Id).Team.Title})"
                    });
                }
                foreach (var athlet in selectedAthletes)
                {
                    selectedAthletesLists.Add(new SelectListItem
                    {
                        Value = athlet.Id.ToString(),
                        Text = $"{athlet.User.FullName} ({db.TeamsPlayers.Find(athlet.Id).Team.Title})"
                    });
                }

                vm.AthletesList = athletesLists.OrderBy(c => c.Text).AsEnumerable();
                if (vm.IsIndividual)
                    vm.SelectedTeamsList = selectedAthletesLists.OrderBy(c => c.Text).AsEnumerable();
            }

            vm.GamesTypes = new SelectList(groupRepo.GetGamesTypes(), "TypeId", "Name", vm.TypeId);

            var group = groupRepo.GetById(vm.GroupId);
            if (group != null)
                vm.PointId = group.PointEditType != null ? (int)group.PointEditType + 1 : 2;
        }

        private void UpdateTennisGroupFormListsFromDB(GroupsForm vm, int categoryId, int seasonId = 0)
        {
            var stageId = vm.StageId;
            var isIndividualSection = true;

            var selectedAthletes = Enumerable.Empty<TennisPlayerWithPair>();
            if (vm.GroupId != 0)
                selectedAthletes = playersRepo.GetTennisPlayersOfTheGroup(vm.GroupId);


            var athletesLists = new List<SelectListItem>();
            var selectedAthletesLists = new List<SelectListItem>();

            if (isIndividualSection == true)
            {
                var nonIncludedAthletes = GetAllNonIncludedTennisPlayers(vm.LeagueId, categoryId, vm.SeasonId, vm.StageId);
                foreach (var athlete in nonIncludedAthletes)
                {
                    athletesLists.Add(new SelectListItem
                    {
                        Value = athlete.Id.ToString(),
                        Text = $"{athlete.FullName}"
                    });
                }
                foreach (var athlet in selectedAthletes)
                {
                    selectedAthletesLists.Add(new SelectListItem
                    {
                        Value = athlet.Value,
                        Text = athlet.Name
                    });
                }

                vm.AthletesList = athletesLists.AsEnumerable();
                
                if (vm.IsIndividual)
                    vm.SelectedTeamsList = selectedAthletesLists.AsEnumerable(); //.OrderBy(c => c.Text)

            }

            vm.GamesTypes = new SelectList(groupRepo.GetGamesTypes(), "TypeId", "Name", vm.TypeId);

            var group = groupRepo.GetById(vm.GroupId);
            if (group != null)
                vm.PointId = group.PointEditType != null ? (int)group.PointEditType + 1 : 2;
        }

        [HttpPost]
        public JsonResult FilterAthletes(AthletesModel settings)
        {
            AthletesFilter filter = new AthletesFilter(settings);

            var leagueTeams = teamRepo.GetTeamsByLeague(settings.LeagueId);
            var athletes = playersRepo.GetTeamPlayersDTOByIds(
                GetAllNonIncludedAthletes(leagueTeams, settings.SeasonId, settings.StageId).Select(c => c.Id).ToArray())
                .ToList();

            var includedAthletes = settings.SelectedAthletesIds == null
                ? playersRepo.GetTeamPlayersDTOByIds(settings.SelectedPlayoffAthletesIds).ToList()
                : playersRepo.GetTeamPlayersDTOByIds(settings.SelectedAthletesIds).ToList();

            List<TeamPlayerItem> filtredAthletes = athletes;
            if (settings.AgeStart.HasValue || settings.AgeEnd.HasValue ||
                settings.WeightFrom.HasValue || settings.WeightTo.HasValue ||
                settings.SelectedRanks != null || settings.SelectedRanks?.Count != 0)
            {
                filtredAthletes = filter.FilterAthletes(athletes);
            }

            var filtredAthletesList = filter.GetAthletesFinalData(filtredAthletes, includedAthletes);

            return Json(new { FiltredAthletes = filtredAthletesList, IncludedAthletes = includedAthletes });
        }

        private void UpdateGroupFormListsFromSelection(GroupsForm vm, bool isIndividualGroup)
        {
            var notIncludedAthletes = new List<TeamsPlayer>();
            var notIncludedTeams = new List<Team>();

            var selectedAthletes = new List<TeamsPlayer>();
            var selectedTeams = new List<Team>();

            if (vm.TeamsArr != null)
            {
                if (isIndividualGroup)
                    notIncludedAthletes = playersRepo.GetPlayersByIds(vm.AthletesArr);
                notIncludedTeams = teamRepo.GetTeamsByIds(vm.TeamsArr);
            }

            if (vm.SelectedTeamsArr != null)
            {
                if (isIndividualGroup)
                    notIncludedAthletes = playersRepo.GetPlayersByIds(vm.SelectedTeamsArr);
                notIncludedTeams = teamRepo.GetTeamsByIds(vm.SelectedTeamsArr);
            }

            if (!isIndividualGroup)
            {
                vm.TeamsList = new SelectList(notIncludedTeams, "TeamId", "Title");
                vm.SelectedTeamsList = new SelectList(selectedTeams, "TeamId", "Title");
            }

            if (isIndividualGroup)
            {
                //Add here fullname/teams
                vm.TeamsList = new SelectList(notIncludedAthletes, "Id", "User.FullName");
            }
            vm.GamesTypes = new SelectList(groupRepo.GetGamesTypes(), "TypeId", "Name", vm.TypeId);
        }

        [HttpPost]
        public ActionResult Edit(GroupsForm vm, int? departmentId = null)
        {
            bool isIndividual = vm.Type == "0" || vm.Type == null ? false : true;
            var numberOfTeamsNow = gamesRepo.GetTeamsNumberInGroup(vm.GroupId);
            if (!vm.AllowIncomplete && vm.NumberOfTeams.HasValue &&
                vm.SelectedTeamsArr != null && vm.NumberOfTeams.Value != vm.SelectedTeamsArr.Count(_ => _ != null))
            {
                UpdateGroupFormListsFromDB(vm);
                ModelState.AddModelError("NumberOfTeams", Messages.NumberOfTeamsError);

                return PartialView("_Edit", vm);
            }

            if (vm.TeamsArr != null || vm.SelectedTeamsArr != null || vm.AthletesArr != null)
                UpdateGroupFormListsFromSelection(vm, isIndividual);
            else
            {
                UpdateGroupFormListsFromDB(vm);
            }
            if (vm.SelectedTeamsArr == null || vm.SelectedTeamsArr.Length == 0)
            {
                ModelState.AddModelError("SelectedTeamsArr", "נא להוסיף קבוצות לפני שמירה");
                return PartialView("_Edit", vm);
            }

            var item = new Group();
            if (vm.GroupId != 0)
                item = groupRepo.GetById(vm.GroupId);
            else
            {
                if (isIndividual == true)
                    vm.IsIndividual = isIndividual;
                groupRepo.Create(item);
                vm.JustCreated = true;
            }

            //TODO: Not sure why do we need this. If no-one will find a reason till summer 2018, remmove this check
            if (vm.PointId < 1 || vm.PointId > 4)
                vm.PointId = 0;

            item.PointEditType = vm.PointId - 1;
            TryUpdateModel(item);
            item.IsAdvanced = false;
            item.IsIndividual = isIndividual;


            LeagueRankService svc = new LeagueRankService(vm.LeagueId);
            var teams = new List<RankTeam>();
            foreach (var team in vm.SelectedTeamsArr)
            {
                var res = svc.AddTeamIfNotExist(team, teams, isIndividual);
            }
            vm.Points = new int[teams.Count];
            vm.Names = new string[teams.Count];
            vm.IdTeams = new int?[teams.Count];
            for (var i = 0; i < teams.Count; i++)
            {
                vm.Points[i] = 0;
                if (isIndividual)
                {
                    //Add here logic for athletes (need for points)
                }
                else
                {
                    var team = teamRepo.GetGroupTeam((int)item.GroupId, (int)teams[i].Id);
                    if (team != null)
                        vm.Points[i] = team.Points != null ? (int)team.Points : 0;
                }
                vm.Names[i] = teams[i].Title;
                vm.IdTeams[i] = teams[i].Id;
            }

            groupRepo.UpdateTeams(item, vm.SelectedTeamsArr, isIndividual);
            vm.InjectFrom(item);
            groupRepo.Save();
            vm.GroupId = item.GroupId;
            if (vm.PointId == 3)
            {
                TempData["GroupId"] = vm.GroupId;
                return PartialView("_EditPoints", vm);
            }

            //Update schedule 
            if (vm.NumberOfTeams == numberOfTeamsNow)
            {
                gamesRepo.UpdateGamesInGameSchedule(vm.GroupId, vm.StageId, vm.SelectedTeamsArr);
            }

            ViewBag.SeasonId = vm.SeasonId;
            TempData["LeagueId"] = vm.LeagueId;
            vm.IsIndividual = secRepo.GetByLeagueId(vm.LeagueId).IsIndividual;
            vm.Athtletes = new AthletesModel();

            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;
            return PartialView("_Edit", vm);
        }

        [HttpPost]
        public ActionResult EditTennisGroup(GroupsForm vm, int? departmentId = null)
        {
            bool isIndividual = vm.Type == "0" || vm.Type == null ? false : true;
            bool isPairs = vm.Type == "2";
            var numberOfTeamsNow = gamesRepo.GetTennisTeamsNumberInGroup(vm.GroupId);
            /*
            if (!vm.AllowIncomplete && vm.NumberOfTeams.HasValue &&
                vm.SelectedTennisTeamsArr != null && vm.NumberOfTeams.Value != vm.SelectedTennisTeamsArr.Count(_ => _ != null))
            {
                UpdateTennisGroupFormListsFromDB(vm, vm.CategoryId.Value);
                ModelState.AddModelError("NumberOfTeams", Messages.NumberOfTeamsError);

                return PartialView("_TennisGroupEdit", vm);
            }
            */
            if (vm.TeamsArr != null || vm.SelectedTennisTeamsArr != null || vm.AthletesArr != null)
                UpdateGroupFormListsFromSelection(vm, isIndividual);
            else
            {
                UpdateTennisGroupFormListsFromDB(vm, vm.CategoryId.Value);
            }
            if (vm.SelectedTennisTeamsArr == null || vm.SelectedTennisTeamsArr.Length == 0)
            {
                ModelState.AddModelError("SelectedTennisTeamsArr", "נא להוסיף קבוצות לפני שמירה");
                return PartialView("_TennisGroupEdit", vm);
            }

            var item = new TennisGroup();
            if (vm.GroupId != 0)
                item = groupRepo.GetTennisGroupById(vm.GroupId);
            else
            {
                groupRepo.CreateTennisGroup(item);
                vm.JustCreated = true;
            }

            //TODO: Not sure why do we need this. If no-one will find a reason till summer 2018, remmove this check
            if (vm.PointId < 1 || vm.PointId > 4)
                vm.PointId = 0;

            item.PointEditType = vm.PointId - 1;
            TryUpdateModel(item);
            item.IsAdvanced = false;
            item.IsIndividual = isIndividual;
            item.IsPairs = vm.Type == "2";
            item.NumberOfPlayers = vm.NumberOfTeams;

            LeagueRankService svc = new LeagueRankService(vm.LeagueId);
            var teams = new List<RankTeam>();
            //TODO: Check this part of code for tennis competition with pairs
            //foreach (var team in vm.SelectedTeamsArr)
            //{
            //    var res = svc.AddTeamIfNotExist(team, teams, isIndividual);
            //}

            vm.Points = new int[teams.Count];
            vm.Names = new string[teams.Count];
            vm.IdTeams = new int?[teams.Count];
            //for (var i = 0; i < teams.Count; i++)
            //{
            //    vm.Points[i] = 0;
            //    if (isIndividual)
            //    {
            //        //Add here logic for athletes (need for points)
            //    }
            //    else
            //    {
            //        var team = teamRepo.GetGroupTeam((int)item.GroupId, (int)teams[i].Id);
            //        if (team != null)
            //            vm.Points[i] = team.Points != null ? (int)team.Points : 0;
            //    }
            //    vm.Names[i] = teams[i].Title;
            //    vm.IdTeams[i] = teams[i].Id;
            //}

            groupRepo.UpdateTennisTeams(item, vm.SelectedTennisTeamsArr, isIndividual, isPairs);
            vm.InjectFrom(item);
            groupRepo.Save();
            vm.GroupId = item.GroupId;
            if (vm.PointId == 3)
            {
                TempData["GroupId"] = vm.GroupId;
                return PartialView("_EditPoints", vm);
            }

            //Update schedule 
            if (vm.NumberOfTeams == numberOfTeamsNow)
            {
                gamesRepo.UpdateTennisGamesInGameSchedule(vm.GroupId, vm.StageId, vm.SelectedTennisTeamsArr, isPairs);
            }

            ViewBag.SeasonId = vm.SeasonId;
            TempData["LeagueId"] = vm.LeagueId;
            vm.IsIndividual = true;
            vm.Athtletes = new AthletesModel();

            ViewBag.IsDepartmentLeague = departmentId.HasValue ? true : false;
            ViewBag.DepartmentId = departmentId;
            return PartialView("_TennisGroupEdit", vm);
        }

        [HttpPost]
        public ActionResult EditPoints(GroupsForm vm)
        {
            for (var i = 0; i < vm.IdTeams.Count(); i++)
            {
                if (vm.IdTeams[i] == null || TempData["GroupId"] == null)
                    continue;
                var team = teamRepo.GetGroupTeam((int)TempData["GroupId"], (int)vm.IdTeams[i]);
                if (team != null)
                    team.Points = vm.Points[i];
            }
            teamRepo.Save();
            TempData["LeagueId"] = vm.LeagueId;
            return PartialView("_Edit", vm);
        }

        private List<TeamPlayerItem> GetAllNonIncludedAthletes(List<Team> teams, int? seasonId, int stageId)
        {
            var playerList = new List<TeamPlayerItem>();
            var leagueId = stagesRepo.GetById(stageId).League.LeagueId;
            var league = db.Leagues.Find(leagueId);
            var season = db.Seasons.Find(seasonId);

            foreach (var team in teams)
                playerList.AddRange(playersRepo.GetTeamPlayersShort(team.TeamId, 0, league, season));


            var groupeAthletes = playersRepo.GetGroupAthletes((int)seasonId, stageId);

            if (groupeAthletes != null)
            {
                foreach (var groupAthlete in groupeAthletes)
                    playerList.RemoveAll(a => a.Id == groupAthlete?.Id);
            }
            return playerList;
        }

        private List<TeamPlayerItem> GetAllNonIncludedTennisPlayers(int leagueId, int categoryId, int? seasonId, int stageId)
        {
            var playerList = new List<TeamPlayerItem>();
            var groupeAthletes = playersRepo.GetTennisPlayers(leagueId, categoryId, seasonId.Value);

            foreach (var t in groupeAthletes)
            {
                var res = new TeamPlayerItem
                {
                    Id = t.Id,
                    Weight = t.User.Weight,
                    WeightUnits = t.User.WeightUnits,
                    UserId = t.UserId,
                    ShirtNum = t.ShirtNum,
                    PosId = t.PosId,
                    FullName = t.User.FullName,
                    IdentNum = t.User.IdentNum,
                    PassportNum = t.User.PassportNum,
                    IsActive = t.IsActive,
                    SeasonId = seasonId.Value,
                    Birthday = t.User.BirthDay,
                    City = t.User.City,
                    Email = t.User.Email,
                    Insurance = t.User.Insurance,
                    InsuranceFile = t.User.PlayerFiles.Where(x => x.SeasonId == seasonId && x.FileType == (int)PlayerFileType.Insurance)
                            .Select(x => x.FileName)
                            .FirstOrDefault(),
                    MedicalCertificate = t.User.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == seasonId)?.Approved == true,
                    MedicalCertificateFile = t.User.PlayerFiles.Where(x => x.SeasonId == seasonId && x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive)
                            .Select(x => x.FileName)
                            .FirstOrDefault(),
                    ShirtSize = t.User.ShirtSize,
                    Telephone = t.User.Telephone,
                    IsLocked = t.IsLocked,
                    PlayerImage = t.User.PlayerFiles.Where(x => x.SeasonId == seasonId && x.FileType == (int)PlayerFileType.PlayerImage)
                                          .Select(x => x.FileName)
                                          .FirstOrDefault() ?? t.User.Image,
                    NoInsurancePayment = t.User.NoInsurancePayment,

                    IsTrainerPlayer = t.IsTrainerPlayer,
                    IsEscortPlayer = t.IsEscortPlayer,
                    IsApprovedByManager = t.IsApprovedByManager,
                    IsBlockaded = t.User?.BlockadeId != null,
                    StartPlaying = t.StartPlaying,
                    Comment = t.Comment,
                    UnionComment = t.UnionComment,
                    TeamPlayerPaid = t.Paid,
                    IsExceptionalMoved = t.IsExceptionalMoved,
                    IsUnderPenalty = t.User.PenaltyForExclusions.Where(c => !c.IsCanceled).Any(c => !c.IsEnded)
                };
                playerList.Add(res);
            }

            return playerList;
        }
    }
}