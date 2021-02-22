using DataService.LeagueRank;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DataService;
using Resources;
using ClosedXML.Excel;
using System.IO;
using AppModel;
using CmsApp.Helpers;
using CmsApp.Models;
using DataService.DTO;
using CmsApp.Services;
using DataService.Services;
using System.IO;
using System.Web;

namespace CmsApp.Controllers
{
    public class LeagueRankController : AdminController
    {
        #region Fields & constructor
        private readonly TeamsRepo _teamsRepo;
        private readonly UnionsRepo _unionsRepo;
        private LeagueRankExportHelper _exportHelper;
        public LeagueRankController()
        {
            _teamsRepo = new TeamsRepo();
            _unionsRepo = new UnionsRepo();
            _exportHelper = new LeagueRankExportHelper();
        }
        #endregion



        // GET: LeagueRank/Details/5
        public ActionResult Details(int id, int seasonId, int? unionId)
        {
            var league = leagueRepo.GetById(id);
            var club = clubsRepo.GetById(league?.ClubId ?? 0);
            var rleague = new RankLeague();
            var sectionAlias = "";
            if (unionId.HasValue)
            {
                var section = _unionsRepo.GetSectionByUnionId(unionId.Value);
                sectionAlias = section.Alias;
            }
            else if (club != null && club.SportSection == null)
            {
                var clubSection = club.Union?.Section?.Alias;
                sectionAlias = club.Section?.Alias ?? club.Union?.Section?.Alias;
            }
            else
            {
                sectionAlias = club.SportSection.Alias;
            }

            var rLeague = UpdateRankLeague(id, seasonId, sectionAlias == GamesAlias.Tennis);
            rLeague.Stages.Reverse();
            if (league.IsCompetitionLeague)
            {
                ViewBag.CompetitionDisciplinesTable = CompetitionDisciplinesTableRank(id, seasonId);
            }
            ViewBag.PlayoffTable = UpdatePlayoffRank(id, seasonId, sectionAlias == GamesAlias.Tennis);
            ViewBag.SectionAlias = sectionAlias;
            ViewBag.SeasonId = seasonId;
            switch (sectionAlias)
            {
                case GamesAlias.WaterPolo:
                case GamesAlias.Soccer:
                case GamesAlias.Rugby:
                case GamesAlias.Softball:
                    return PartialView("Waterpolo/_Details", rLeague);

                case GamesAlias.BasketBall:
                    return PartialView("Basketball/_Details", rLeague);

                case GamesAlias.NetBall:
                case GamesAlias.VolleyBall:
                    //TODO display extended table
                    return PartialView("Netball_VolleyBall/_Details", rLeague);

                default:
                    return PartialView("_Details", rLeague);
            }
        }


        public ActionResult RankedStanding(int leagueId, int seasonId)
        {
            var service = new LeagueRankService(leagueId);
            var rankedStanding = service.GetRankedStanding(seasonId);

            return PartialView("_RankedStanding", rankedStanding);
        }

        public ActionResult AthleticRankedStanding(int leagueId, int seasonId)
        {
            var rankings = CupClubRanks(leagueId, seasonId);
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            return PartialView("_AthleticRanksTable", rankings);
        }




        [HttpPost]
        public void UpdateRankedStandingCorrection(int leagueId, int teamId, int value)
        {
            var team = db.Teams.Find(teamId);
            var league = db.Leagues.Find(leagueId);

            if (team != null && league != null)
            {
                var correction =
                    db.RankedStandingCorrections.FirstOrDefault(x => x.LeagueId == leagueId && x.TeamId == teamId);

                if (correction == null)
                {
                    db.RankedStandingCorrections.Add(new RankedStandingCorrection
                    {
                        TeamId = teamId,
                        LeagueId = leagueId,
                        Value = value
                    });
                }
                else
                {
                    correction.Value = value;
                }

                db.SaveChanges();
            }
        }

        public ActionResult CategoryRankDetails(int categoryId, int seasonId, int? unionId, bool isCategoryStanding = false)
        {
            var category = teamRepo.GetById(categoryId);
            var listLevels = category.CompetitionLevel?.LevelPointsSettings?.OrderBy(x => x.Rank).Where(t => t.SeasonId == seasonId).ToList();
            var leagueId = category.LeagueTeams.FirstOrDefault(t => t.SeasonId == seasonId)?.LeagueId ?? 0;

            RankCategory rCategory = new RankCategory();
            if (listLevels != null)
            {
                ViewBag.PlayerRankGroupList = gamesRepo.GetTennisPlayoffRanksGroup(categoryId, leagueId, seasonId, false);
            }
            ViewBag.PlayoffTable = UpdateTennisPlayoffRank(categoryId, seasonId);
            ViewBag.CategoryId = categoryId;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            return PartialView("_TennisDetails", rCategory);
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult SetTennisCategoryRankCorrection(int PlayerId, int SeasonId, int LeagueId, int Correction, int CategoryId, string GroupName)
        {
            var decodedGroupName = HttpUtility.HtmlDecode(GroupName);
            gamesRepo.SetTennisCategoryRankCorrection(PlayerId, LeagueId, CategoryId, SeasonId, Correction, decodedGroupName);
            return Json(new { Success = true });
        }



        public ActionResult UpdateTennisPlayoffRanksGroup(int categoryId, int seasonId, int? unionId, bool isCategoryStanding = false)
        {
            var category = teamRepo.GetById(categoryId);
            var listLevels = category.CompetitionLevel?.LevelPointsSettings?.OrderBy(x => x.Rank).Where(t => t.SeasonId == seasonId).ToList();
            var leagueId = category.LeagueTeams.FirstOrDefault(t => t.SeasonId == seasonId)?.LeagueId ?? 0;
            RankCategory rCategory = new RankCategory();
            if (listLevels != null)
            {
                ViewBag.PlayerRankGroupList = gamesRepo.UpdateTennisPlayoffRanksGroup(categoryId, leagueId, seasonId, listLevels);
                ViewBag.PlayerRankGroupList = gamesRepo.GetTennisPlayoffRanksGroup(categoryId, leagueId, seasonId, false);
            }
            ViewBag.PlayoffTable = UpdateTennisPlayoffRank(categoryId, seasonId);
            ViewBag.CategoryId = categoryId;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            return PartialView("_TennisDetails", rCategory);
        }

        public ActionResult SetManualTennisRankCalculation(int categoryId, int seasonId, int? unionId)
        {
            var category = teamRepo.GetById(categoryId);
            var listLevels = category.CompetitionLevel?.LevelPointsSettings?.OrderBy(x => x.Rank).Where(t => t.SeasonId == seasonId).ToList();
            var leagueId = category.LeagueTeams.FirstOrDefault(t => t.SeasonId == seasonId)?.LeagueId ?? 0;
            RankCategory rCategory = new RankCategory();
            if (listLevels != null)
            {
               gamesRepo.SetManualTennisRankCalculation(categoryId, leagueId, seasonId, listLevels);
                ViewBag.PlayerRankGroupList = gamesRepo.GetTennisPlayoffRanksGroup(categoryId, leagueId, seasonId, false);
            }
            ViewBag.PlayoffTable = UpdateTennisPlayoffRank(categoryId, seasonId);
            ViewBag.CategoryId = categoryId;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            return PartialView("_TennisDetails", rCategory);
        }


        public ActionResult SetManualTennisRankSwap(int categoryId, int seasonId, string groupName, int? swap1, int? swap2)
        {
            var category = teamRepo.GetById(categoryId);
            var listLevels = category.CompetitionLevel?.LevelPointsSettings?.OrderBy(x => x.Rank).Where(t => t.SeasonId == seasonId).ToList();
            var leagueId = category.LeagueTeams.FirstOrDefault(t => t.SeasonId == seasonId)?.LeagueId ?? 0;
            RankCategory rCategory = new RankCategory();
            if (listLevels != null && swap1.HasValue && swap2.HasValue)
            {
                if (string.IsNullOrWhiteSpace(groupName))
                {
                    groupName = "";
                }
                gamesRepo.ManualTennisRankSwap(categoryId, leagueId, seasonId, groupName, swap1.Value, swap2.Value);
                ViewBag.PlayerRankGroupList = gamesRepo.GetTennisPlayoffRanksGroup(categoryId, leagueId, seasonId, false);
            }
            ViewBag.PlayoffTable = UpdateTennisPlayoffRank(categoryId, seasonId);
            ViewBag.CategoryId = categoryId;
            ViewBag.SeasonId = seasonId;
            ViewBag.LeagueId = leagueId;
            return PartialView("_TennisDetails", rCategory);
        }




        public ContentResult GetTennisCompetitionRank(int id, int categoryId, int competitionId, int seasonId)
        {
            var data = gamesRepo.GetTennisPlayoffRanksGroup(categoryId, competitionId, seasonId, true);
            var index = 0;
            foreach (var group in data)
            {
                foreach (var player in group.PlayersRanks)
                {
                    if (!string.IsNullOrWhiteSpace(player.PlayerName))
                    {
                        index += 1;
                        player.IndexedRank = index;
                     }
                }
            }

            var playerRanks = new Dictionary<string,TennisPlayoffRank>();
            var tpIds = new List<int>();
            foreach (var group in data)
            {
                var playerIds = group.PlayersRanks.Select(p => p.PlayerId ?? 0).ToList();
                var playerPairedIds = group.PlayersRanks.Select(p => p.PairPlayerId ?? 0).ToList();
                tpIds.AddRange(playerIds);
                tpIds.AddRange(playerPairedIds);
            }
            var tps = db.TeamsPlayers.Where(tp => tpIds.Contains(tp.Id)).ToList();

            foreach (var group in data)
            {
                var selectedTps = tps.Where(tp => tp.UserId == id).Select(tp => tp.Id).ToList();
                var rank = group.PlayersRanks.FirstOrDefault(r => (r.PlayerId.HasValue && selectedTps.Contains(r.PlayerId ?? 0)) || (r.PairPlayerId.HasValue && selectedTps.Contains(r.PairPlayerId ?? 0)));
                if(rank != null)
                {
                    playerRanks.Add(group.GroupName, rank);
                }
            }
            var htmlstrData = "";
            foreach (var group in playerRanks)
            {
                htmlstrData += $"<div>{group.Key}</div>";
                htmlstrData += $"<div><label>{@Messages.Rank}:</label>{group.Value.IndexedRank}</div>";
                htmlstrData += $"<div><label>{@Messages.Points}:</label>{group.Value.Points}</div>";
                htmlstrData += $"<div></div>";
            }
            return new ContentResult { Content = htmlstrData, ContentType = "application/html" };
        }





        public ActionResult TennisUnionRankDetails(int? unionId, int? ageId, int? clubId, int seasonId, bool isUpdate = false)
        {
            ViewBag.IsUpdated = isUpdate;
            var season = seasonsRepository.GetById(seasonId);
            if (ageId.HasValue)
            {
                var rankList = teamRepo.GetTennisUnionRanks(unionId, ageId, clubId, seasonId, isUpdate);
                UnionTennisRankForm result = new UnionTennisRankForm();
                result.RankList = rankList;
                result.UnionId = unionId;
                result.ClubId = clubId;
                result.SeasonId = seasonId;
                result.CompetitionAgeId = ageId;
                result.MinimumParticipationRequired = season.MinimumParticipationRequired ?? 0;
                result.ListAges = new SelectList(teamRepo.GetCompetitionAges(unionId.Value, seasonId), "id", "age_name", ageId);
                return PartialView("_TennisUnionRankTable", result);
            }
            else
            {
                UnionTennisRankForm result = new UnionTennisRankForm();
                result.RankList = new List<UnionTennisRankDto>();
                result.UnionId = unionId;
                result.ClubId = clubId;
                result.SeasonId = seasonId;
                result.CompetitionAgeId = ageId;
                result.MinimumParticipationRequired = season.MinimumParticipationRequired ?? 0;
                result.ListAges = new SelectList(teamRepo.GetCompetitionAges(unionId.Value, seasonId), "id", "age_name", ageId);

                return PartialView("_TennisUnionRankTable", result);
            }

        }

        [HttpPost]
        public JsonResult ExportTennisUnionRank(int? unionId, int? ageId, int? clubId, int seasonId)
        {
            if (ageId.HasValue)
            {
                var fromBirth = db.CompetitionAges.Where(x => x.id == ageId).FirstOrDefault().from_birth;
                var toBirth = db.CompetitionAges.Where(x => x.id == ageId).FirstOrDefault().to_birth;

                List<League> competitionList = new List<League>();
                List<UnionTennisRankDto> rankList = new List<UnionTennisRankDto>();

                if (clubId.HasValue)
                {
                    competitionList = leagueRepo.GetByClub(clubId.Value, seasonId)
                        .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                        .ToList();
                }
                else if (unionId.HasValue)
                {
                    if (User.IsInAnyRole(AppRole.Workers))
                    {
                        switch (usersRepo.GetTopLevelJob(base.AdminId))
                        {
                            case JobRole.UnionManager:
                            case JobRole.Unionviewer:
                                competitionList = leagueRepo.GetByUnion(unionId.Value, seasonId)
                                    .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                                    .ToList();
                                break;
                            case JobRole.LeagueManager:
                                competitionList = leagueRepo.GetByManagerId(base.AdminId, seasonId)
                                    .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                                    .ToList();
                                break;
                            case JobRole.TeamManager:
                                break;
                        }
                    }
                    else
                    {
                        competitionList = leagueRepo.GetByUnion(unionId.Value, seasonId)
                            .Where(x => x.EilatTournament != null && ((bool)x.EilatTournament) == true)
                            .ToList();
                    }
                }

                if (competitionList != null && competitionList.Any())
                {
                    var players = playersRepo.GetTeamPlayersByUnionIdShort(unionId.Value, seasonId).Where(x => x.User.BirthDay >= fromBirth && x.User.BirthDay <= toBirth).GroupBy(x => x.UserId);
                    foreach (var player in players)
                    {
                        var item = new UnionTennisRankDto();
                        item.UserId = player.FirstOrDefault().UserId;
                        item.Birthday = player.FirstOrDefault().User.BirthDay;
                        item.FullName = player.FirstOrDefault().User.FullName;
                        item.TotalPoints = 0;
                        item.AveragePoints = 0;
                        item.PointsToAverage = 0;
                        var club = playersRepo.GetClubOfPlayer(item.UserId);
                        item.TrainingTeam = club == null ? string.Empty : club.Name;
                        item.CompetitionList = new List<UnionTennisCompetitionDto>();
                        var competitionListOfPlayer = competitionList.Where(x => !x.IsArchive && x.LeagueTeams.Any(y => y.Teams.TeamsPlayers.Any(z => z.UserId == item.UserId)));
                        foreach (var competition in competitionListOfPlayer)
                        {
                            UnionTennisCompetitionDto competitionItem = new UnionTennisCompetitionDto();
                            competitionItem.CompetitionId = competition.LeagueId;
                            competitionItem.CompetitionPoints = 0;
                            competitionItem.StartDate = competition.LeagueStartDate;
                            competitionItem.MinParticipationReq = competition.MinParticipationReq.HasValue ? competition.MinParticipationReq.Value : 0;
                            item.CompetitionList.Add(competitionItem);
                        }

                        rankList.Add(item);
                    }

                    foreach (var player in players)
                    {
                        foreach (var playerInCategory in player)
                        {
                            var club = clubsRepo.GetByTeamAndSeason(playerInCategory.TeamId, seasonId).FirstOrDefault();
                            var sectionAlias = "";
                            if (unionId.HasValue)
                            {
                                var section = _unionsRepo.GetSectionByUnionId(unionId.Value);
                                sectionAlias = section.Alias;
                            }
                            else if (club != null && club.SportSection == null)
                            {
                                var clubSection = club.Union?.Section?.Alias;
                                sectionAlias = club.Section?.Alias ?? club.Union?.Section?.Alias;
                            }
                            else
                            {
                                sectionAlias = club.SportSection.Alias;
                            }

                            var rCategory = UpdateRankCategory(playerInCategory.TeamId, seasonId);
                            var playersKnockout = new List<int?>();
                            foreach (var stageGroup in rCategory.Stages)
                            {
                                var isEnded = leagueRepo.CheckAllTennisGamesIsEnded(stageGroup.StageId, playerInCategory.TeamId,
                                    stageGroup.Groups.All(g => g.GameType == GameType.Playoff || g.GameType == GameType.Knockout));

                                var hasPlayers = stageGroup.Groups.Any(x => x.Players.Any());
                                if (hasPlayers && isEnded)
                                {
                                    foreach (var group in stageGroup.Groups)
                                    {
                                        foreach (var playerInGroup in group.Players)
                                        {
                                            if (playerInGroup.Id == playerInCategory.Id)
                                            {
                                                if (group.GameType == GameType.Knockout || group.GameType == GameType.Playoff)
                                                {
                                                    if (playersKnockout.Contains(playerInGroup.Id))
                                                    {
                                                        break;
                                                    }
                                                    playersKnockout.Add(playerInGroup.Id);
                                                }
                                                var item = rankList.Where(x => x.UserId == playerInCategory.UserId).FirstOrDefault();

                                                var category = db.Teams.Where(x => x.TeamId == playerInCategory.TeamId).FirstOrDefault();
                                                if (category.LevelId != null)
                                                {
                                                    int position = Int32.Parse(playerInGroup.Position);
                                                    var pointSetting = db.LevelPointsSettings.Where(x => x.LevelId == category.LevelId && x.Rank == position).FirstOrDefault();
                                                    item.TotalPoints = item.TotalPoints + pointSetting.Points;

                                                    var league = teamRepo.GetLeagueByTeamId(category.TeamId);
                                                    var competitionItem = item.CompetitionList.Where(x => x.CompetitionId == league.LeagueId).FirstOrDefault();
                                                    if (competitionItem != null)
                                                    {
                                                        competitionItem.CompetitionPoints += pointSetting.Points;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //TODO: Why we need here playoff table?
                            //ViewBag.PlayoffTable = UpdateTennisPlayoffRank(playerInCategory.TeamId, seasonId);
                        }
                    }
                }

                foreach (var rankItem in rankList)
                {
                    if (rankItem.CompetitionList.Count > 0)
                    {
                        rankItem.CompetitionList = rankItem.CompetitionList.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.CompetitionId).ToList();
                        int minParticipationReq = rankItem.CompetitionList.FirstOrDefault().MinParticipationReq;

                        rankItem.CompetitionList = rankItem.CompetitionList.OrderByDescending(x => x.CompetitionPoints).ToList();
                        int pointsToAverage = 0;
                        if (minParticipationReq > 0)
                        {
                            if (rankItem.CompetitionList.Count < minParticipationReq)
                            {
                                minParticipationReq = rankItem.CompetitionList.Count;
                            }
                            for (int i = 0; i < minParticipationReq; i++)
                            {
                                pointsToAverage += rankItem.CompetitionList[i].CompetitionPoints;
                            }

                            rankItem.PointsToAverage = pointsToAverage;
                            rankItem.AveragePoints = pointsToAverage / minParticipationReq;
                        }
                    }
                }

                rankList = rankList.OrderByDescending(x => x.TotalPoints).ToList();
                UnionTennisRankForm result = new UnionTennisRankForm();
                result.RankList = rankList;
                result.UnionId = unionId;
                result.ClubId = clubId;
                result.SeasonId = seasonId;
                result.CompetitionAgeId = ageId;
                result.ListAges = new SelectList(teamRepo.GetCompetitionAges(unionId.Value, seasonId), "id", "age_name", ageId);

                var ageName = db.CompetitionAges.Where(x => x.id == ageId).FirstOrDefault().age_name;

                using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
                {
                    var union = unionsRepo.GetById(unionId.Value);
                    var ws = workbook.AddWorksheet($"{union?.Name}_{Messages.Rank}");
                    _exportHelper.CreateExcelForTennisRank(ws, result, getCulture() == CultEnum.He_IL);

                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", $"attachment;filename= {union?.Name?.Replace(' ', '_')}_{Messages.Standings}_{ageName}.xlsx");

                    using (MemoryStream myMemoryStream = new MemoryStream())
                    {
                        workbook.SaveAs(myMemoryStream);
                        myMemoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                }
            }
            else
            {
                UnionTennisRankForm result = new UnionTennisRankForm();
                result.RankList = new List<UnionTennisRankDto>();
                result.UnionId = unionId;
                result.ClubId = clubId;
                result.SeasonId = seasonId;
                result.CompetitionAgeId = ageId;
                result.ListAges = new SelectList(teamRepo.GetCompetitionAges(unionId.Value, seasonId), "id", "age_name", ageId);

                using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
                {
                    var union = unionsRepo.GetById(unionId.Value);
                    var ws = workbook.AddWorksheet($"{union?.Name?.Replace(' ', '_')}_{Messages.Rank}");
                    _exportHelper.CreateExcelForTennisRank(ws, result, getCulture() == CultEnum.He_IL);

                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", $"attachment;filename= {union?.Name?.Replace(' ', '_')}_{Messages.Rank}_{AppFunc.GetUniqName()}.xlsx");

                    using (MemoryStream myMemoryStream = new MemoryStream())
                    {
                        workbook.SaveAs(myMemoryStream);
                        myMemoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                }
            }

            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult RemovePenalty(int id, int leagueId, int seasonId)
        {
            teamRepo.RemovePenalty(id);

            return RedirectToAction("Edit", "Leagues", new { id = leagueId, seasonId });
        }

        public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.LastIndexOf(Find);
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }

        //for further excel columns timespans parsings
        private void BuildFileList()
        {
            string[] array = new string[] {
                                             "00:11:16:00",
                                          };

            string path = @"c:\temp\MyTest.txt";

            // Create a file to write to.
            using (StreamWriter sw = System.IO.File.CreateText(path))
            {
                for (int i = 0; i < array.Length; i++)
                {
                    TimeSpan res;
                    var str = ReplaceLastOccurrence(array[i], ":", ".");   //array[i];

                    bool success = TimeSpan.TryParse(str, out res);
                    if (success)
                    {
                        sw.WriteLine(res.TotalMilliseconds.ToString() + ",");
                    }
                    else
                    {
                        var t = 0;
                    }

                }
            }


            /*
            using (StreamReader sr = System.IO.File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    Console.WriteLine(s);
                }
            }
            */
        }



        public ActionResult AthleticLeagueCompetitionRanks(int competitionId, int seasonId, bool isGoldenSpike = false)
        {
            var competition = leagueRepo.GetById(competitionId);
            ViewBag.IsFieldCompetition = false;
            List<IGrouping<Tuple<int?, int>, CompetitionClubsCorrection>> grouped = new List<IGrouping<Tuple<int?, int>, CompetitionClubsCorrection>>();
            if (competition.IsFieldCompetition)
            {
                ViewBag.IsFieldCompetition = true;
                ViewBag.FieldRaceTables = leagueRepo.GetFieldRaceTables(competition); ;
            }

            var data = db.CompetitionClubsCorrections.Where(c => c.LeagueId == competitionId && c.SeasonId == seasonId).ToList();
            grouped = data.OrderBy(x => x.TypeId).ThenByDescending(x => x.GenderId).ThenByDescending(x => x.FinalScore).GroupBy(c => new Tuple<int?, int>(c.TypeId, c.GenderId)).ToList();

            ViewBag.LeagueId = competitionId;
            ViewBag.SeasonId = seasonId;
            ViewBag.isGoldenSpike = isGoldenSpike;
            return PartialView("_AthleticLeagueCompetitionRanks", grouped);
        }

        public ActionResult AthleticCombinedRanks(int competitionId, int seasonId)
        {
            var competition = leagueRepo.GetById(competitionId);
            var competitionDisciplines = disciplinesRepo.GetCompetitionDisciplines(competitionId).Select(c => new CompetitionDisciplineDto
            {
                SectionAlias = c.League.Union.Section.Alias,
                DisciplineId = c.DisciplineId
            }).ToList();
            var isGoldenSpike = 0;
            if (competitionDisciplines.FirstOrDefault()?.SectionAlias == GamesAlias.Athletics)
            {
                foreach (var competitionDiscipline in competitionDisciplines)
                {
                    if (competitionDiscipline.DisciplineId.HasValue)
                    {
                        var discipline = disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);
                        if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && discipline.DisciplineType == "GoldenSpikesU14")
                        {
                            isGoldenSpike = 1;
                            break;
                        }
                        if (!string.IsNullOrWhiteSpace(discipline.DisciplineType) && discipline.DisciplineType == "GoldenSpikesU16")
                        {
                            isGoldenSpike = 2;
                            break;
                        }
                    }
                }
            }

            var combinedPlayerRanks = leagueRepo.GetAthleticCombinedRanking(competition, isGoldenSpike);
            var competitionsDisciplines = competition.CompetitionDisciplines.Where(cd => cd.IsMultiBattle && !cd.IsDeleted);
            var multiBattleCompetitionsDisciplines = competitionsDisciplines.OrderBy(a => db.Disciplines.FirstOrDefault(t => t.DisciplineId == a.DisciplineId), new LeagueRepo.CombinedDisciplineComparer(isGoldenSpike == 0 ? competitionsDisciplines.Count() : -1));
            var disciplinesNameListForMen = new List<string>();
            var disciplinesNameListForWomen = new List<string>();
            foreach (var multiBattleCompetitionsDiscipline in multiBattleCompetitionsDisciplines)
            {
                var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == multiBattleCompetitionsDiscipline.DisciplineId);
                var name = discipline.Name;
                if (multiBattleCompetitionsDiscipline.CompetitionAge.gender == 1)
                {
                    disciplinesNameListForMen.Add(name);
                }
                else
                {
                    disciplinesNameListForWomen.Add(name);
                }
            }
            ViewBag.MultiBattleDisciplinesNameListForMen = disciplinesNameListForMen;
            ViewBag.MultiBattleDisciplinesNameListForWomen = disciplinesNameListForWomen;

            ViewBag.LeagueId = competitionId;
            ViewBag.SeasonId = seasonId;

            return PartialView("_AthleticCombinedRanks", combinedPlayerRanks);
        }




        public ActionResult UpdateAthleticLeagueCompetitionRank(int competitionId, int seasonId)
        {
            leagueRepo.UpdateAthleticLeagueCompetitionRanks(competitionId, seasonId);
            return RedirectToAction("AthleticLeagueCompetitionRanks", new { competitionId, seasonId });
        }

        public ActionResult UpdateAthleticCompetitionPoints(int competitionId, int seasonId, bool isGoldenSpikes)
        {
            leagueRepo.UpdateAthleticLeagueCompetitionRanks(competitionId, seasonId, isGoldenSpikes);
            return Json(new { });
        }


        [HttpPost]
        public ActionResult AddPenalty(AddTeamPenaltyModel model)
        {
            teamRepo.AddPenalty(new TeamPenalty
            {
                StageId = model.StageId,
                TeamId = model.TeamId,
                Points = model.Points,
                Date = DateTime.Now
            });

            return RedirectToAction("Edit", "Leagues", new { id = model.LeagueId, model.SeasonId });
        }

        private IEnumerable<PlayoffRank> UpdatePlayoffRank(int id, int seasonId, bool isTennisLeague = false)
        {
            var svc = new LeagueRankService(id);
            return svc.CreatePlayoffEmptyTable(seasonId, out bool hasPlayoff, isTennisLeague).OrderBy(c => c.Rank);
        }

        private IEnumerable<TennisPlayoffRank> UpdateTennisPlayoffRank(int categoryId, int seasonId)
        {
            var svc = new CategoryRankService(categoryId);
            return svc.CreatePlayoffEmptyTable(seasonId, out bool hasPlayoff).OrderBy(c => c.Rank);
        }

        private RankLeague UpdateRankLeague(int id, int seasonId, bool isTennisLeague = false)
        {
            LeagueRankService svc = new LeagueRankService(id);
            RankLeague rLeague = svc.CreateLeagueRankTable(seasonId, isTennisLeague);

            if (rLeague == null)
            {
                rLeague = new RankLeague();
            }
            else if (rLeague.Stages.Count == 0)
            {
                rLeague = svc.CreateEmptyRankTable(seasonId);
                rLeague.IsEmptyRankTable = true;

                if (rLeague.Stages.Count == 0)
                {
                    if (User.IsInAnyRole(AppRole.Workers))
                    {
                        switch (usersRepo.GetTopLevelJob(base.AdminId))
                        {
                            case JobRole.UnionManager:
                                rLeague.Teams = _teamsRepo.GetTeams(seasonId, id).ToList();
                                break;
                            case JobRole.LeagueManager:
                                rLeague.Teams = _teamsRepo.GetTeams(seasonId, id).ToList();
                                break;
                            case JobRole.TeamManager:
                                rLeague.Teams = _teamsRepo.GetByManagerId(base.AdminId, seasonId);
                                break;
                        }
                    }
                    else
                    {
                        rLeague.Teams = _teamsRepo.GetTeams(seasonId, id).ToList();
                    }
                }
            }

            rLeague.CanEditPenalties = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager);

            return rLeague;
        }


        private List<List<CompetitionDisciplineDto>> CompetitionDisciplinesTableRank(int id, int seasonId)
        {
            var league = leagueRepo.GetById(id);
            var possitionSettings = db.PositionSettings.Where(p => p.LeagueId == id && p.SeasonId == seasonId).ToList();
            List<CompetitionDisciplineDto> rankedMDisciplines = new List<CompetitionDisciplineDto>();
            List<CompetitionDisciplineDto> rankedFDisciplines = new List<CompetitionDisciplineDto>();

            foreach (var competitionDiscipline in league.CompetitionDisciplines.Where(cd => cd.IsForScore))
            {
                List<decimal> positionValues = new List<decimal>();
                var discipline = disciplinesRepo.GetById(competitionDiscipline.DisciplineId.Value);
                var validForScores = competitionDiscipline.CompetitionDisciplineRegistrations.Where(r => !r.IsArchive && r.CompetitionResult.FirstOrDefault()?.Heat == "1" && r.CompetitionResult.FirstOrDefault()?.AlternativeResult == 0);
                List<CompDiscRegRankDTO> orderedRegistrationsWithResults;
                var genderId = competitionDiscipline.CompetitionAge.gender;

                if (competitionDiscipline.IsResultsManualyRanked)
                {
                    orderedRegistrationsWithResults = validForScores.OrderBy(x => x.CompetitionResult.FirstOrDefault().Rank ?? int.MaxValue).Select(r => new CompDiscRegRankDTO
                    {
                        RegistrationId = r.Id,
                        ClubId = r.ClubId.Value,
                        ClubName = r.Club.Name,
                        isAlternative = r.CompetitionResult.FirstOrDefault()?.AlternativeResult > 0
                    }).ToList();
                }
                else if (discipline.Format != null && ((discipline.Format.Value >= 6 && discipline.Format.Value <= 8) || discipline.Format.Value == 10 || discipline.Format.Value == 11))
                {
                    orderedRegistrationsWithResults = validForScores.OrderByDescending(x => x.CompetitionResult.FirstOrDefault().SortValue ?? long.MinValue).Select(r => new CompDiscRegRankDTO
                    {
                        RegistrationId = r.Id,
                        ClubId = r.ClubId.Value,
                        ClubName = r.Club.Name,
                        SortedValue = r.CompetitionResult.FirstOrDefault()?.SortValue,
                        isAlternative = r.CompetitionResult.FirstOrDefault()?.AlternativeResult > 0
                    }).ToList();
                }
                else
                {
                    orderedRegistrationsWithResults = validForScores.OrderBy(x => x.CompetitionResult.FirstOrDefault().SortValue ?? long.MaxValue).Select(r => new CompDiscRegRankDTO
                    {
                        RegistrationId = r.Id,
                        ClubId = r.ClubId.Value,
                        ClubName = r.Club.Name,
                        SortedValue = r.CompetitionResult.FirstOrDefault()?.SortValue,
                        isAlternative = r.CompetitionResult.FirstOrDefault()?.AlternativeResult > 0
                    }).ToList();
                }

                for (int i = 0; i < orderedRegistrationsWithResults.Count; i++)
                {
                    int upToIndex = i;
                    positionValues.Add(possitionSettings.FirstOrDefault(p => p.Position == i + 1)?.Points ?? 0);

                    if (!competitionDiscipline.IsResultsManualyRanked)
                    {
                        for (int j = i; j >= 0; j--)
                        {
                            if (orderedRegistrationsWithResults.ElementAt(j).SortedValue == orderedRegistrationsWithResults.ElementAt(i).SortedValue)
                            {
                                upToIndex = j;
                            }
                            else
                            {
                                break;
                            }
                        }
                        decimal totalvalue = 0;
                        for (int j = upToIndex; j <= i; j++)
                        {
                            var posVal = possitionSettings.FirstOrDefault(p => p.Position == j + 1)?.Points ?? 0;
                            totalvalue = totalvalue + posVal;
                        }
                        decimal media = 0;
                        if (totalvalue != 0)
                        {
                            media = totalvalue / (i - upToIndex + 1);
                            for (int j = upToIndex; j <= i; j++)
                            {
                                positionValues[j] = media;
                            }
                        }
                    }
                }

                for (int i = 0; i < orderedRegistrationsWithResults.Count; i++)
                {
                    var posVal = positionValues[i];
                    if (orderedRegistrationsWithResults.ElementAt(i).SortedValue.HasValue && !orderedRegistrationsWithResults.ElementAt(i).isAlternative)
                        orderedRegistrationsWithResults.ElementAt(i).Score = posVal;
                }

                if (orderedRegistrationsWithResults.Count() > 0)
                {
                    var competitionDisciplineDto = new CompetitionDisciplineDto
                    {
                        Id = competitionDiscipline.Id,
                        DisciplineName = discipline.Name,
                        ClubsPointed = orderedRegistrationsWithResults.OrderByDescending(r => r.Score ?? decimal.MinValue).ToList()
                    };
                    if (competitionDiscipline.CompetitionAge.gender == 1)
                    {
                        rankedMDisciplines.Add(competitionDisciplineDto);
                    }
                    if (competitionDiscipline.CompetitionAge.gender == 0)
                    {
                        rankedFDisciplines.Add(competitionDisciplineDto);
                    }
                }
            }

            List<List<CompetitionDisciplineDto>> res = new List<List<CompetitionDisciplineDto>>
            {
                rankedMDisciplines,
                rankedFDisciplines
            };

            return res;
        }


        private List<List<CompetitionClubRankedStanding>> CupClubRanks(int id, int seasonId)
        {
            return disciplinesRepo.CupClubRanks(id, seasonId);
        }

        private RankCategory UpdateRankCategory(int categoryId, int seasonId, bool isCategoryStading = false)
        {
            CategoryRankService svc = new CategoryRankService(categoryId);
            RankCategory rCategory = svc.CreateCategoryRankTable(seasonId, isCategoryStading);

            if (rCategory == null)
            {
                rCategory = new RankCategory();
            }
            else if (rCategory.Stages.Count == 0)
            {
                rCategory = svc.CreateEmptyRankTable(seasonId);
                rCategory.IsEmptyRankTable = true;

                if (rCategory.Stages.Count == 0)
                {
                    rCategory.Players = db.TeamsPlayers.Where(x => x.TeamId == categoryId && x.SeasonId == seasonId).ToList();
                }
            }


            return rCategory;
        }

        [HttpPost]
        public void ExportLeagueRanksToExcel(int id, int seasonId, int? unionId)
        {
            var league = leagueRepo.GetById(id);
            var sectionAlias = league?.Club?.Section?.Alias ?? league?.Club?.Union?.Section?.Alias
                ?? league?.Union?.Section?.Alias ?? string.Empty;
            var rLeague = UpdateRankLeague(id, seasonId);
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet($"{league?.Name}_{Messages.Standings}");

                switch (sectionAlias)
                {
                    case GamesAlias.WaterPolo:
                    case GamesAlias.Soccer:
                    case GamesAlias.Rugby:
                    case GamesAlias.Softball:
                        _exportHelper.CreateExcelForWaterpoloAndSoccer(ws, rLeague, getCulture() == CultEnum.He_IL);
                        break;
                    case GamesAlias.BasketBall:
                        _exportHelper.CreateExcelForBasketball(ws, rLeague, getCulture() == CultEnum.He_IL);
                        break;

                    case GamesAlias.NetBall:
                    case GamesAlias.VolleyBall:
                        _exportHelper.CreateExcelForNetballAndVolleyball(ws, rLeague, getCulture() == CultEnum.He_IL);
                        break;

                    default:
                        _exportHelper.CreateDefaultExcel(ws, rLeague, getCulture() == CultEnum.He_IL);
                        break;
                }

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment;filename= {league?.Name?.Replace(' ', '_')}_{Messages.Standings}_{AppFunc.GetUniqName()}.xlsx");

                using (MemoryStream myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        public void ExportTennisLeagueRanksToExcel(int id, int seasonId)
        {
            var league = leagueRepo.GetById(id);
            var sectionAlias = league?.Club?.Section?.Alias ?? league?.Club?.Union?.Section?.Alias
                ?? league?.Union?.Section?.Alias ?? string.Empty;
            var rLeague = UpdateRankLeague(id, seasonId, true);
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet($"{league?.Name}_{Messages.Standings}");

                _exportHelper.CreateExcelForTennisLeague(ws, rLeague, getCulture() == CultEnum.He_IL);

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment;filename= {league?.Name?.Replace(' ', '_')}_{Messages.Standings}_{AppFunc.GetUniqName()}.xlsx");

                using (MemoryStream myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        [HttpPost]
        public void ChangePositionSettingsStatus(int leagueId, bool status)
        {
            bool isSuccess = leagueRepo.ChangePositionSettingsStatus(leagueId, status);
            if (isSuccess) leagueRepo.Save();
        }

        public ActionResult LoadPositionSettings(int leagueId, int seasonId)
        {
            var positions = leagueRepo.GetAllPositionSettings(leagueId, seasonId);

            ViewBag.LeagueId = leagueId;
            ViewBag.SeasonId = seasonId;

            return PartialView("_PositionSettings", positions);
        }

        [HttpPost]
        public ActionResult CreatePositionSetting(PositionStatusForm form)
        {
            if (form.Points.HasValue && form.Position.HasValue)
            {
                var dto = new PositionSettingFormDto
                {
                    LeagueId = form.LeagueId,
                    SeasonId = form.SeasonId,
                    Points = form.Points.Value,
                    Position = form.Position.Value
                };
                leagueRepo.CreatePositionSetting(dto);
            }

            return RedirectToAction(nameof(LoadPositionSettings), new { leagueId = form.LeagueId, seasonId = form.SeasonId });
        }

        public ActionResult DeletePositionSetting(int id, int leagueId, int seasonId)
        {
            leagueRepo.DeletePositionSetting(id);
            return RedirectToAction(nameof(LoadPositionSettings), new { leagueId, seasonId });
        }

        [HttpPost]
        public ActionResult UpdatePositionSettings(PositionStatusForm form)
        {
            if (form.Position.HasValue && form.Points.HasValue)
            {
                var dto = new PositionSettingsDto
                {
                    Id = form.Id,
                    Points = form.Points.Value,
                    Position = form.Position.Value
                };
                leagueRepo.UpdatePositionSettings(dto);
            }
            return RedirectToAction(nameof(LoadPositionSettings), new { leagueId = form.LeagueId, seasonId = form.SeasonId });
        }

        [HttpGet]
        public ActionResult TennisLeagueDetails(int id, int seasonId)
        {
            var rLeague = UpdateRankLeague(id, seasonId, true);
            ViewBag.PlayoffTable = UpdatePlayoffRank(id, seasonId);
            return PartialView("Tennis/_LeagueDetails", rLeague);
        }

        public ActionResult Ranks(int id, int? seasonId = null)
        {
            var games = new int[] { };
            var resList = gamesRepo.GetSchedulesForExternalLink(id, seasonId, games);
            var lRepo = new LeagueRepo();
            var league = lRepo.GetById(id);

            ViewBag.SeasonId = seasonId;
            if (league != null)
            {
                ViewBag.LeagueId = league.LeagueId;
                ViewBag.ResTitle = league.Name.Trim();

                var alias = league.Union?.Section?.Alias ?? league.Club?.Section?.Alias;
                ViewBag.IsTennisLeagueGame = (league.EilatTournament == null || league.EilatTournament == false)
                    && alias.Equals(GamesAlias.Tennis);
                if (ViewBag.IsTennisLeagueGame == true)
                {
                    _teamsRepo.ChangeTeamNamesForTheTennis(resList);
                }
                switch (alias)
                {
                    case GamesAlias.WaterPolo:
                    case GamesAlias.Soccer:
                    case GamesAlias.Rugby:
                    case GamesAlias.BasketBall:
                    case GamesAlias.Softball:
                        return PartialView("_WaterBasketRanksTable", resList);
                    default:
                        return PartialView("_RanksTable", resList);
                }
            }

            return View(resList);
        }
    }
}