using AppModel;
using DataService.DTO;
using DataService.Services;
using DataService.Utils;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DataService.LeagueRank;
using CmsApp.Models;

namespace DataService
{
    public class GamesRepo : BaseRepo
    {
        private UsersRepo usersRepo;
        private BracketsRepo bracketsRepo;
        private TeamsRepo tRepo;
        private IQueryable<GamesCycle> _gameCycles;
        private SeasonsRepo seasonsRepo;
        private GoogleMapsApiService googleMapsApiService;
        private object league;
        public static List<string> disciplineTypesThatUsesWinds = new List<string> { "100m", "100mh", "110mh", "200m", "60m", "60mh", "80m", "80mh", "long_jump", "triple_jump" };

        public class GameFilterConditions
        {
            public int seasonId { get; set; }
            public IEnumerable<LeagueShort> leagues { get; set; }
            public IEnumerable<AuditoriumShort> auditoriums { get; set; }
            //  Always starts from midnight
            private DateTime? _dateFrom;

            public bool onlyNotIsArchive = false;

            public DateTime? dateFrom
            {
                get { return _dateFrom; }
                set
                {
                    if (!value.HasValue)
                    {
                        _dateFrom = null;
                    }
                    else
                    {   //  Cut off time
                        _dateFrom = new DateTime(value.Value.Year, value.Value.Month, value.Value.Day);
                    }
                }
            }
            //  Always ends one millisecond before the next midnight
            private DateTime? _dateTo;
            public int? leagueId;

            public DateTime? dateTo
            {
                get { return _dateTo; }
                set
                {
                    if (!value.HasValue)
                    {
                        _dateTo = null;
                    }
                    else
                    {   //  Set time to the last millisecond in the day (include whole day)
                        _dateTo = new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, 23, 59, 59, 999);
                    }
                }
            }
        }

        public Game GetGameSettings(int leagueId)
        {
            var nonArchiveStagesId = db.Stages.Where(s => s.LeagueId == leagueId && !s.IsArchive).Select(t => t.StageId).ToList();
            return db.Games.FirstOrDefault(g => g.StageId.HasValue && nonArchiveStagesId.Contains(g.StageId.Value))
                ?? db.Games.FirstOrDefault(g => g.LeagueId == leagueId);
        }

        public TennisGame GetTennisGameSettings(int categoryId)
        {
            var nonArchiveStagesId = db.TennisStages.Where(s => s.CategoryId == categoryId && !s.IsArchive).Select(t => t.StageId).ToList();
            return db.TennisGames.FirstOrDefault(g => g.StageId.HasValue && nonArchiveStagesId.Contains(g.StageId.Value))
                ?? db.TennisGames.FirstOrDefault(g => g.CategoryId == categoryId);
        }

        public Game GetGameSettings(int stageId, int leagueId)
        {
            var groupedGroups = db.Groups.Where(g => g.Stage.LeagueId == leagueId && !g.IsArchive).GroupBy(r => r.Name);
            var neededStageId = groupedGroups.FirstOrDefault(g => g.Any(r => r.StageId == stageId))?.FirstOrDefault()?.StageId;
            return db.Games.FirstOrDefault(g => g.StageId == neededStageId && g.LeagueId == leagueId);
        }

        public GoogleMapsApiService GetGoogleMapsApiService()
        {
            return googleMapsApiService;
        }

        private decimal getJobFee(decimal? hourFee, DateTime? LeagueStartDate, bool isSaturdayTariff, DateTime? fromTime, DateTime? toTime)
        {
            if (!hourFee.HasValue)
            {
                return 0;
            }
            decimal multiplier = 1;
            if (isSaturdayTariff && fromTime.HasValue && toTime.HasValue && LeagueStartDate.HasValue)
            {
                DayOfWeek fromDay = fromTime.Value.DayOfWeek;
                int fromHour = fromTime.Value.Hour;
                DayOfWeek toDay = toTime.Value.DayOfWeek;
                int toHour = toTime.Value.Hour;

                if (toDay <= fromDay && (toDay != fromDay || toHour <= fromHour))
                {
                    toTime.Value.AddDays(7);
                }

                DayOfWeek gameDay = LeagueStartDate.Value.DayOfWeek;
                var gameHour = LeagueStartDate.Value.Hour;
                DateTime testDate = new DateTime(1970, 2, (int)gameDay + 1);
                testDate = testDate.AddHours(gameHour);
                if (fromTime < testDate && toTime > testDate)
                {
                    multiplier = 1.5M;
                }
            }
            return hourFee.Value * multiplier;
        }

        public WorkerReportDTO GetWorkerReportInfo(User user, WorkerReportDTO report, DateTime ReportStartDate, DateTime ReportEndDate, string distanceSettings, bool isSaturdayTariff)
        {
            if (user == null) return report;
            List<string> neededJobs = new List<string> { JobRole.Referee, JobRole.Spectator, JobRole.Desk };
            var roleType = user.UsersJobs.FirstOrDefault(uu => uu.Union != null && uu.Union.Section.Alias == GamesAlias.Athletics).RateType;
            var IsOfficialSettingsChecked = user.UsersJobs.FirstOrDefault(uu => uu.Union != null)?.Union?.IsOfficialSettingsChecked ?? false;
            List<UnionOfficialSetting> unionOfficialSettings = null;
            if (IsOfficialSettingsChecked)
            {
                unionOfficialSettings = user.UsersJobs.FirstOrDefault(uu => uu.Union != null).Union.UnionOfficialSettings.ToList();
                report.WithholdingTax = user.UsersJobs.FirstOrDefault(uu => uu.Union != null).WithhodlingTax;
            }
            List<GameShortDto> gamesAssigned = null;
            gamesAssigned = user.UsersJobs.Where(u => u.League != null && u.TravelInformations.Any(x => x.FromHour.HasValue) && u.TravelInformations.Any(x => x.ToHour.HasValue) && u.League.LeagueStartDate >= ReportStartDate && u.League.LeagueStartDate <= ReportEndDate && neededJobs.Contains(u.Job.JobsRole.RoleName)).Select(u => new GameShortDto
            {
                StartDate = u.League.LeagueStartDate,
                AuditoriumName = u.League.PlaceOfCompetition,
                Role = u.Job.JobsRole.RoleName,
                OfficialCity = u.User.City,
                OfficialAddress = u.User.Address,
                League = u.League,
                Id = u.Id,
                Union = u.League.Union,
                Comment = u.OfficialGameReportDetails.Count() > 0 ? u.OfficialGameReportDetails.FirstOrDefault().Comment : "",
                IsUnionReport = IsOfficialSettingsChecked,
                TravelDistance = GetTravelDistance(u),
                WorkedHours = GetWorkedHours(u),
                DaysWorked = u.TravelInformations.Where(x => x.FromHour.HasValue).Select(x => x.FromHour).ToList(),
                LeagueFee = GetLeagueFee(isSaturdayTariff, u, roleType, unionOfficialSettings),
            }).ToList();
            report.GamesAssigned = gamesAssigned.ToList();
            report.GamesCount = report.GamesAssigned.Count();
            if (report.GamesAssigned.FirstOrDefault()?.League?.Union?.Section?.IsIndividual == true)
            {
                var allDaysWorked = new List<DateTime?>();
                foreach (var game in report.GamesAssigned)
                {
                    allDaysWorked.AddRange(game.DaysWorked);
                }
                report.DaysCount = allDaysWorked.Distinct().Count();
            }
            else
            {
                report.DaysCount = report.GamesAssigned.Where(c => c.StartDate.HasValue)
                    .Select(c => c.StartDate.Value.ToShortDateString()).Distinct().Count();
            }
            report.LeaguesGrouped = report.GamesAssigned?.GroupBy(c => c.League);
            report.TotalFeeCount = report.GamesAssigned.Sum(r => r.LeagueFee ?? 0);
            return report;
        }

        private decimal GetLeagueFee(bool isSaturdayTariff, UsersJob userJob, string roleType, List<UnionOfficialSetting> unionOfficialSettings)
        {
            decimal workedHours = 0;
            foreach (var travelInformation in userJob.TravelInformations)
            {
                workedHours += Decimal.Round((decimal)((travelInformation.ToHour - travelInformation.FromHour).Value.TotalMinutes / 60));
            }
            if (unionOfficialSettings != null)
            {
                return Decimal.Round((decimal)workedHours * getJobFee(GetLeaguePayment(unionOfficialSettings.FirstOrDefault(ls => ls.JobsRole?.RoleName == userJob.Job.JobsRole.RoleName), roleType, "Game"), userJob.League.LeagueStartDate, isSaturdayTariff, userJob.League.Union.SaturdaysTariffFromTime, userJob.League.Union.SaturdaysTariffToTime), 2);
            }
            else
            {
                return Decimal.Round((decimal)workedHours * getJobFee(GetLeaguePayment(userJob.League.LeagueOfficialsSettings?.FirstOrDefault(ls => ls.JobsRole?.RoleName == userJob.Job.JobsRole.RoleName), roleType, "Game"), userJob.League.LeagueStartDate, isSaturdayTariff, userJob.League.Union.SaturdaysTariffFromTime, userJob.League.Union.SaturdaysTariffToTime), 2);
            }
        }

        private static double GetWorkedHours(UsersJob userJob)
        {
            double workedHours = 0;
            foreach (var travelInformation in userJob.TravelInformations)
            {
                workedHours += Math.Round((travelInformation.ToHour - travelInformation.FromHour).Value.TotalMinutes / 60.0f, 2); ;
            }
            return workedHours;
        }

        private double GetTravelDistance(UsersJob userJob)
        {
            double travelDistance = 0;
            foreach (var travelInformation in userJob.TravelInformations)
            {
                travelDistance += (travelInformation.NoTravel.HasValue && travelInformation.NoTravel.Value) ? 0 : userJob.OfficialGameReportDetails.Count() > 0 && userJob.OfficialGameReportDetails.FirstOrDefault().TravelDistance.HasValue ? userJob.OfficialGameReportDetails.FirstOrDefault().TravelDistance.Value : 2 * GetGoogleMapsApiService().GetDistance(userJob.User.Address, travelInformation.IsUnionTravel ? userJob.League.Union.Address : userJob.League.PlaceOfCompetition);
            }
            return travelDistance;
        }

        public List<string> SetAllReferees(int stageId, int[] cycleIds, int[] refereeIds)
        {
            List<string> allRefNames = new List<string>();
            foreach (var cycleId in cycleIds)
            {
                var cycle = db.GamesCycles.FirstOrDefault(g => g.StageId == stageId && g.CycleId == cycleId);
                if (cycle != null)
                {
                    var splitedReferees = cycle.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() ?? new List<int>();
                    var newReferees = splitedReferees.Union(refereeIds).Where(i => i != 0).ToArray();
                    var refereeIdsStr = string.Join(",", newReferees);
                    cycle.RefereeIds = refereeIdsStr;
                    var referees = db.Users.Where(p => newReferees.Contains(p.UserId)).ToList();
                    string btnTitle = "";
                    List<string> refNames = new List<string>();
                    foreach (var referee in referees)
                    {
                        refNames.Add(referee.FullName);
                    }
                    btnTitle = string.Join(",", refNames);
                    allRefNames.Add(btnTitle);
                }

            }
            Save();
            return allRefNames;
        }


        public List<string> UpdateRoundReferees(int stageId, int[] cycleIds, int refereeId, bool isChecked)
        {
            List<string> allRefNames = new List<string>();
            foreach (var cycleId in cycleIds)
            {
                var cycle = db.GamesCycles.FirstOrDefault(g => g.StageId == stageId && g.CycleId == cycleId);
                if (cycle != null)
                {
                    var splitedReferees = cycle.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).Where(i => i != 0).ToList() ?? new List<int>();

                    if (refereeId > 0)
                    {
                        if (splitedReferees.Contains(refereeId) && !isChecked)
                        {
                            splitedReferees = splitedReferees.Where(r => r != refereeId).ToList();
                        }
                        else if (!splitedReferees.Contains(refereeId) && isChecked)
                        {
                            splitedReferees.Add(refereeId);
                        }
                    }

                    var newReferees = splitedReferees.Where(i => i != 0).ToArray();

                    var refereeIdsStr = string.Join(",", newReferees);
                    cycle.RefereeIds = refereeIdsStr;

                    var referees = db.Users.Where(p => newReferees.Contains(p.UserId)).ToList();
                    string btnTitle = "";
                    List<string> refNames = new List<string>();
                    foreach (var referee in referees)
                    {
                        refNames.Add(referee.FullName);
                    }
                    btnTitle = string.Join(",", refNames);
                    allRefNames.Add(btnTitle);
                }

            }
            Save();
            return allRefNames;
        }


        public List<string> UpdateRoundAllReferees(int stageId, int[] cycleIds, int[] refereeIds, bool isChecked)
        {
            List<string> allRefNames = new List<string>();
            foreach (var cycleId in cycleIds)
            {
                var cycle = db.GamesCycles.FirstOrDefault(g => g.StageId == stageId && g.CycleId == cycleId);
                if (cycle != null)
                {
                    var newReferees = new int[0];
                    if (isChecked)
                    {
                        var splitedReferees = cycle.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).Where(i => i != 0).ToList() ?? new List<int>();
                        foreach (var refereeId in refereeIds)
                        {
                            if (refereeId > 0)
                            {
                                if (splitedReferees.Contains(refereeId) && !isChecked)
                                {
                                    splitedReferees = splitedReferees.Where(r => r != refereeId).ToList();
                                }
                                else if (!splitedReferees.Contains(refereeId) && isChecked)
                                {
                                    splitedReferees.Add(refereeId);
                                }
                            }
                        }
                        newReferees = splitedReferees.Where(i => i != 0).ToArray();
                    }
                    var refereeIdsStr = string.Join(",", newReferees);
                    cycle.RefereeIds = refereeIdsStr;

                    var referees = db.Users.Where(p => newReferees.Contains(p.UserId)).ToList();
                    string btnTitle = "";
                    List<string> refNames = new List<string>();
                    foreach (var referee in referees)
                    {
                        refNames.Add(referee.FullName);
                    }
                    btnTitle = string.Join(",", refNames);
                    allRefNames.Add(btnTitle);
                }

            }
            Save();
            return allRefNames;
        }

        public List<PlayersStatisticsDTO> GetStatisticsOfWGame(GamesCycle game, int teamId)
        {
            var oppoTeamInfo = db.WaterpoloStatistics.Where(x => x.TeamId == game.GuestTeamId && x.GameId == game.CycleId);
            var oppoGoal = oppoTeamInfo.Sum(x => x.GOAL) ?? 0;
            var oppoMiss = oppoTeamInfo.Sum(x => x.Miss) ?? 0;

            var teamPlayersIds = db.TeamsPlayers.Where(t => t.TeamId == teamId).Select(t => t.Id).Distinct();
            var gamePlayersStatistics = game.WaterpoloStatistics.Where(c => teamPlayersIds.Contains(c.PlayerId));
            var games = new List<PlayersStatisticsDTO>();

            if (gamePlayersStatistics != null && gamePlayersStatistics.Any())
            {
                foreach (var playerId in teamPlayersIds)
                {
                    var statistics = gamePlayersStatistics.Where(c => c.PlayerId == playerId).ToList();
                    var plStat = new PlayersStatisticsDTO();
                    if (statistics.Any())
                    {
                        plStat.StatId = statistics.FirstOrDefault().Id;
                        plStat.PlayersId = playerId;
                        plStat.PlayersName = statistics.FirstOrDefault()?.TeamsPlayer?.User?.FullName ?? "";
                        plStat.MinsInFormat = statistics.Sum(s => s.MinutesPlayed).ToMinutesString();
                        plStat.Goal = statistics.Sum(s => s.GOAL) ?? 0;
                        plStat.PGoal = statistics.Sum(s => s.PGOAL) ?? 0;
                        plStat.Miss = statistics.Sum(s => s.Miss) ?? 0;
                        plStat.PMiss = statistics.Sum(s => s.PMISS) ?? 0;
                        plStat.AST = statistics.Sum(s => s.AST) ?? 0;
                        plStat.STL = statistics.Sum(s => s.STL) ?? 0;
                        plStat.TO = statistics.Sum(s => s.TO) ?? 0;
                        plStat.BLK = statistics.Sum(s => s.BLK) ?? 0;
                        plStat.Offs = statistics.Sum(s => s.OFFS) ?? 0;
                        plStat.Foul = statistics.Sum(s => s.FOUL) ?? 0;
                        plStat.Exc = statistics.Sum(s => s.EXC) ?? 0;
                        plStat.BFoul = statistics.Sum(s => s.BFOUL) ?? 0;
                        plStat.SSave = statistics.Sum(s => s.SSAVE) ?? 0;
                        plStat.YC = statistics.Sum(s => s.YC) ?? 0;
                        plStat.RD = statistics.Sum(s => s.RD) ?? 0;
                        plStat.EFF = statistics.Sum(s => s.EFF) ?? 0D;
                        plStat.PlusMinus = statistics.Sum(s => s.DIFF) ?? 0D;
                        plStat.FGP = (plStat.Goal + plStat.Miss) == 0 ? 0D : Math.Round((double)plStat.Goal * 100 / (double)(plStat.Goal + plStat.Miss), 1);
                        plStat.GSPP = oppoGoal == 0 ? 0D : Math.Round((double)plStat.SSave * 100 / (double)oppoGoal, 1);
                        plStat.SAR = plStat.SSave == 0 ? 0D : Math.Round((double)(oppoGoal + plStat.SSave) / (double)plStat.SSave, 1);
                        plStat.SCR = (plStat.SSave == 0 || oppoGoal == 0) ? 0D : Math.Round((double)(oppoGoal + oppoMiss) / (double)oppoGoal, 1);
                        games.Add(plStat);
                    }
                }
            }

            return games;
        }

        public List<PlayersStatisticsDTO> GetStatisticsOfGame(GamesCycle game, int teamId)
        {
            var teamPlayersIds = db.TeamsPlayers.Where(t => t.TeamId == teamId).Select(t => t.Id).Distinct();
            var gamePlayersStatistics = game.GameStatistics.Where(c => teamPlayersIds.Contains(c.PlayerId));
            var games = new List<PlayersStatisticsDTO>();
            if (gamePlayersStatistics != null && gamePlayersStatistics.Any())
            {
                foreach (var playerId in teamPlayersIds)
                {
                    var statistics = gamePlayersStatistics.Where(c => c.PlayerId == playerId).ToList();
                    var plStat = new PlayersStatisticsDTO();
                    if (statistics.Any())
                    {
                        plStat.StatId = statistics.FirstOrDefault().Id;
                        plStat.PlayersId = playerId;
                        plStat.PlayersName = statistics.FirstOrDefault()?.TeamsPlayer?.User?.FullName ?? "";
                        plStat.MinsInFormat = statistics.Sum(s => s.MinutesPlayed).ToMinutesString();
                        plStat.FG = statistics.Sum(s => s.FG) ?? 0;
                        plStat.FGA = statistics.Sum(s => s.FGA) ?? 0;
                        plStat.FGA = statistics.Sum(s => s.FGA) ?? 0;
                        plStat.ThreePT = statistics.Sum(s => s.ThreePT) ?? 0;
                        plStat.ThreePA = statistics.Sum(s => s.ThreePA) ?? 0;
                        plStat.TwoPT = statistics.Sum(s => s.TwoPT) ?? 0;
                        plStat.TwoPA = statistics.Sum(s => s.TwoPA) ?? 0;
                        plStat.FT = statistics.Sum(s => s.FT) ?? 0;
                        plStat.FTA = statistics.Sum(s => s.FTA) ?? 0;
                        plStat.DREB = statistics.Sum(s => s.DREB) ?? 0;
                        plStat.OREB = statistics.Sum(s => s.OREB) ?? 0;
                        plStat.REB = statistics.Sum(s => s.REB) ?? 0;
                        plStat.REB = statistics.Sum(s => s.REB) ?? 0;
                        plStat.AST = statistics.Sum(s => s.AST) ?? 0;
                        plStat.STL = statistics.Sum(s => s.STL) ?? 0;
                        plStat.TO = statistics.Sum(s => s.TO) ?? 0;
                        plStat.BLK = statistics.Sum(s => s.BLK) ?? 0;
                        plStat.PF = statistics.Sum(s => s.PF) ?? 0;
                        plStat.PTS = statistics.Sum(s => s.PTS) ?? 0;
                        plStat.EFF = statistics.Sum(s => s.EFF) ?? 0D;
                        plStat.PlusMinus = statistics.Sum(s => s.DIFF) ?? 0D;
                        games.Add(plStat);
                    }
                }
            }
            return games;
        }

        public CompetitionSchedulesDto GetCompetitionSchedulesForExternalLink(int categoryId, int seasonId)
        {
            IQueryable<TennisGameCycle> query;
            var cond = PredicateBuilder.New<TennisGameCycle>(gc => gc.TennisStage.CategoryId == categoryId);
            cond = cond.And(gc => gc.TennisStage.SeasonId == seasonId);
            cond = cond.And(gc => gc.TennisGroup.TypeId != GameTypeId.Division || gc.IsPublished);
            var groupCond = PredicateBuilder.New<TennisGroup>(g => !g.IsArchive && g.TennisStage.CategoryId == categoryId && g.TennisGameCycles.Any(gc => gc.IsPublished));
            cond = cond.And(gc => db.TennisGroups.Where(groupCond).Select(g => g.Name).Contains(gc.TennisGroup.Name));
            query = db.TennisGameCycles.Where(cond);

            var cycleList = query.ToList();
            bool? isIndividual = cycleList.FirstOrDefault()?.TennisGroup?.IsIndividual;

            var cycles = ParseCompetitionGameCycles(cycleList, seasonId);

            //  Form brackets (they could contain unpublished games in Playoff and Knockout groups, but not in Division groups)

            var bracketsList = cycles.GroupBy(gc => (gc.GameType == GameTypeId.Division ? GameType.Division : gc.GroupName)).Select(gr => new CompetitionGroupBracketsDto
            {
                GroupName = gr.Key,
                GameTypeId = gr.First().GameType,
                Stages = gr.OrderBy(x => x.CycleId).GroupBy(t => t.GameType == GameTypeId.Knockout34 || t.GameType == GameTypeId.Knockout34Consolences1Round || t.GameType == GameTypeId.Knockout34ConsolencesQuarterRound ? t.StageId.ToString() : t.StageName).Select(t => new CompetitionStageCyclesDto
                {
                    StageName = t.Key,
                    Items = gr.First().GameType == GameTypeId.Division ?
                        t.OrderBy(x => x.CycleNum).ToList() :
                        //  Knockout and Playoff games are temporary sorted by BracketIndex
                        //  because game ranks are calculated straight in the view. To bo fixed. 
                        t.OrderBy(x => x.BracketIndex).ToList()
                }).ToList()
            }).ToList();


            return new CompetitionSchedulesDto
            {
                GameCycles = cycles.Where(gc => gc.IsPublished),
                BracketData = bracketsList,
                //Events = events,
            };
        }

        private List<TennisGameCycleCompetitionDto> ParseCompetitionGameCycles(List<TennisGameCycle> cycleList, int seasonId)
        {
            var games = new List<TennisGameCycleCompetitionDto>();

            foreach (var gc in cycleList)
            {
                var dto = new TennisGameCycleCompetitionDto
                {
                    AuditoriumId = gc.Auditorium?.AuditoriumId ?? 0,
                    Auditorium = gc.Auditorium?.Name,
                    AuditoriumAddress = gc.Auditorium?.Address,
                    CycleId = gc.CycleId,
                    GameType = gc.TennisGroup.TypeId,
                    CategoryId = gc.TennisStage.CategoryId,
                    CategoryName = gc.TennisStage.Team?.Title,
                    CategoryLogo = gc.TennisStage.Team?.Logo,
                    GameStatus = gc.GameStatus,
                    FirstPlayerId = gc.FirstPlayerId,
                    FirstPlayerImage = gc.FirstPlayer?.User?.PlayerFiles?.FirstOrDefault(x => x.FileType == (int)PlayerFileType.PlayerImage && x.SeasonId == seasonId)?.FileName ?? gc.TeamsPlayer?.User?.Image,
                    IsFirstPlayerKnown = gc.FirstPlayerId.HasValue,
                    FirstPlayerScore = gc.IsPublished ? gc.TennisGameSets.Count(t => t.FirstPlayerScore > t.SecondPlayerScore) : -1,
                    SecondPlayerId = gc.SecondPlayerId,
                    SecondPlayerImage = gc.SecondPlayer?.User?.PlayerFiles?.FirstOrDefault(x => x.FileType == (int)PlayerFileType.PlayerImage && x.SeasonId == seasonId)?.FileName ?? gc.TeamsPlayer1?.User?.Image,
                    IsSecondPlayerKnown = gc.SecondPlayerId.HasValue,
                    SecondPlayerScore = gc.IsPublished ? gc.TennisGameSets.Count(t => t.SecondPlayerScore > t.FirstPlayerScore) : -1,
                    StageId = gc.StageId,
                    StageName = "שלב " + gc.TennisStage.Number,
                    GroupId = gc.GroupId.Value,
                    GroupName = gc.TennisGroup.Name,
                    StartDate = gc.StartDate,
                    MaxPlayoffPos = gc.MaxPlayoffPos,
                    MinPlayoffPos = gc.MinPlayoffPos,
                    IsPublished = gc.IsPublished,
                    WinnerId = gc.TechnicalWinnerId ?? gc.TennisPlayoffBracket?.WinnerId,
                    PdfGameReport = gc.PdfGameReport,
                    RoundNum = gc.RoundNum,
                    CycleNum = gc.CycleNum,
                    IsNotSetYet = gc.IsNotSetYet,
                    IsDivision = gc.TennisGroup.TypeId == GameTypeId.Division,
                    TimeInitial = gc.TimeInitial
                };
                if (gc.BracketIndex.HasValue)
                {
                    dto.BracketIndex = gc.BracketIndex.Value;
                }

                string arrivalPlayersPreviousStatus = gc.TennisPlayoffBracket?.Type == (int)PlayoffBracketType.Condolence3rdPlaceBracket || ((gc.TennisGroup.TypeId == GameTypeId.Knockout34Consolences1Round || gc.TennisGroup.TypeId == GameTypeId.Knockout34ConsolencesQuarterRound || gc.TennisGroup.TypeId == GameTypeId.Playoff) && gc.TennisPlayoffBracket?.Type == (int)PlayoffBracketType.Loseer) ? $"Loser" : $"Winner";
                dto.FirstPlayer = gc.FirstPlayerPairId.HasValue
                    ? $"{gc.FirstPlayer?.User?.FullName} / {gc.FirstPairPlayer?.User?.FullName}"
                    : gc.FirstPlayer?.User?.FullName ?? arrivalPlayersPreviousStatus;

                dto.SecondPlayer = gc.SecondPlayerPairId.HasValue
                    ? $"{gc.SecondPlayer?.User?.FullName} / {gc.SecondPairPlayer?.User?.FullName}"
                    : gc.SecondPlayer?.User?.FullName ?? arrivalPlayersPreviousStatus;

                if (gc.FirstPlayerPos == null && gc.TennisPlayoffBracket != null && gc.TennisPlayoffBracket.ParentBracket1Id == null && !gc.FirstPlayerId.HasValue)
                {
                    dto.FirstPlayer = "--";
                }
                if (gc.SecondPlayerId == null && gc.TennisPlayoffBracket != null && gc.TennisPlayoffBracket.ParentBracket2Id == null && !gc.SecondPlayerId.HasValue)
                {
                    dto.SecondPlayer = "--";
                }

                if (gc.BracketId.HasValue && gc.TennisPlayoffBracket != null)
                {
                    dto.Bracket = new BracketDto
                    {
                        Id = gc.BracketId.Value,
                        Type = gc.TennisPlayoffBracket.Type
                    };
                }

                if (gc.TennisGroup.IsAdvanced)
                {
                    dto.IsAdvanced = true;
                    int numOfBrackets = gc.TennisGroup.TennisPlayoffBrackets.Count(b => b.Type != (int)PlayoffBracketType.Loseer);
                    switch (numOfBrackets)
                    {
                        case 1:
                            dto.StageName = "גמר";
                            break;
                        case 2:
                            dto.StageName = "חצי גמר";
                            break;
                        case 4:
                            dto.StageName = "רבע גמר";
                            break;
                        case 8:
                            dto.StageName = "שמינית גמר";
                            break;
                        default:
                            dto.StageName = (numOfBrackets * 2) + " אחרונות";
                            break;
                    }

                    dto.IndexInBracket = gc.TennisPlayoffBracket.TennisGameCycles.ToList().IndexOf(gc);

                }
                games.Add(dto);
            }

            return games;
        }

        public void AddTennisLeagueGameSetResult(int gameId, int homeScore, int guestScore, bool isPairScores, bool isTieBreak)
        {
            var game = db.TennisLeagueGames.Find(gameId);
            db.TennisLeagueGameScores.Add(new TennisLeagueGameScore
            {
                GameId = gameId,
                HomeScore = homeScore,
                GuestScore = guestScore,
                IsPairScores = game?.HomePairPlayerId.HasValue == true || game?.GuestPairPlayerId.HasValue == true,
                IsTieBreak = isTieBreak
            });
        }

        public int? CreateOrUpdateTennisLeagueGame(int cycleId, int gameNumber, int? homePlayerId, int? guestPlayerId, int? technicalWinnerId,
            int? homePairPlayerId, int? guestPairPlayerId)
        {
            var game = db.TennisLeagueGames.FirstOrDefault(g => g.CycleId == cycleId && g.GameNumber == gameNumber);
            if (game == null && (homePlayerId.HasValue || guestPlayerId.HasValue))
            {
                db.TennisLeagueGames.Add(new TennisLeagueGame
                {
                    CycleId = cycleId,
                    GameNumber = gameNumber,
                    HomePlayerId = homePlayerId,
                    GuestPlayerId = guestPlayerId,
                    TechnicalWinnerId = technicalWinnerId,
                    HomePairPlayerId = homePairPlayerId,
                    GuestPairPlayerId = guestPairPlayerId
                });
            }
            else if (game != null && (homePlayerId.HasValue || guestPlayerId.HasValue))
            {
                if (game.HomePairPlayerId.HasValue && game.GuestPairPlayerId.HasValue)
                    db.TennisLeagueGameScores.RemoveRange(game.TennisLeagueGameScores);
                else
                    db.TennisLeagueGameScores.RemoveRange(game.TennisLeagueGameScores);

                game.HomePlayerId = homePlayerId;
                game.GuestPlayerId = guestPlayerId;
                game.TechnicalWinnerId = technicalWinnerId;
                game.HomePairPlayerId = homePairPlayerId;
                game.GuestPairPlayerId = guestPairPlayerId;
            }
            else if (game != null && !homePlayerId.HasValue && !guestPlayerId.HasValue)
            {
                db.TennisLeagueGameScores.RemoveRange(game.TennisLeagueGameScores);
                db.TennisLeagueGames.Remove(game);
            }

            db.SaveChanges();
            int? gameId = null;
            if (game != null) gameId = game.Id;
            else if (homePlayerId.HasValue || guestPlayerId.HasValue)
                gameId = db.TennisLeagueGames.OrderByDescending(g => g.Id).FirstOrDefault().Id;
            return gameId;
        }

        public void CalculateTennisGameScores(Game settings, List<TennisLeagueGame> games, out int homeScore, out int guestScore, out int homeSetsWon, out int guestSetsWon, out int homeGaming, out int guestGaming)
        {
            homeScore = 0;
            guestScore = 0;
            homeSetsWon = 0;
            guestSetsWon = 0;
            homeGaming = 0;
            guestGaming = 0;

            foreach (var game in games)
            {
                if (!game.TechnicalWinnerId.HasValue)
                {
                    var homeCurrentScore = 0;
                    var guestCurrentScore = 0;

                    foreach (var scores in game.TennisLeagueGameScores.ToList())
                    {
                        if (scores.HomeScore > scores.GuestScore)
                        {
                            homeCurrentScore++;
                            homeSetsWon++;
                        }
                        else if (scores.HomeScore < scores.GuestScore)
                        {
                            guestCurrentScore++;
                            guestSetsWon++;
                        }

                        // if its IsTieBreak dont add scores to the scores table.
                        if (!scores.IsTieBreak)
                        {
                            homeGaming += scores.HomeScore;
                            guestGaming += scores.GuestScore;
                        }
                    }

                    if (homeCurrentScore > guestCurrentScore)
                        homeScore += settings.PointsWin;

                    if (homeCurrentScore < guestCurrentScore)
                        guestScore += settings.PointsWin;

                    if (homeCurrentScore == guestCurrentScore)
                    {
                        homeScore += settings.PointsDraw;
                        guestScore += settings.PointsDraw;
                    }
                }
                else
                {
                    var gamesCompleted = game.TennisLeagueGameScores.Where(t => !(t.HomeScore == 0 && t.GuestScore == 0))?.ToList();

                    if (game.TechnicalWinnerId == game.HomePlayerId || game.TechnicalWinnerId == game.HomePairPlayerId)
                    {
                        homeScore += settings.PointsTechWin;
                        guestScore += settings.PointsTechLoss;
                        homeSetsWon += gamesCompleted.Count;
                        homeGaming += gamesCompleted.Count * settings.TechWinHomePoints ?? 6;
                        guestGaming += gamesCompleted.Count * settings.TechWinGuestPoints ?? 0;
                    }

                    else if (game.TechnicalWinnerId == game.GuestPlayerId || game.TechnicalWinnerId == game.GuestPairPlayerId)
                    {
                        guestScore += settings.PointsTechWin;
                        homeScore += settings.PointsTechLoss;
                        guestSetsWon += gamesCompleted.Count;
                        guestGaming += gamesCompleted.Count * settings.TechWinHomePoints ?? 6;
                        homeGaming = gamesCompleted.Count * settings.TechWinGuestPoints ?? 0;
                    }
                }
            }
        }

        public void RemoveTennisSets(TennisLeagueGame game)
        {
            db.TennisLeagueGameScores.RemoveRange(game.TennisLeagueGameScores);
        }

        public void SetAuditoriums(int stageId, int cycleNum, int? auditoriumId)
        {
            var cycles = db.GamesCycles.Where(g => g.StageId == stageId && g.CycleNum == cycleNum)?.ToList();
            if (cycles.Count > 0)
            {
                foreach (var cycle in cycles)
                {
                    cycle.AuditoriumId = auditoriumId;
                }
            }
        }

        public void SetTennisAuditoriums(int stageId, int cycleNum, int? auditoriumId)
        {
            var cycles = db.TennisGameCycles.Where(g => g.StageId == stageId && g.CycleNum == cycleNum)?.ToList();
            if (cycles.Count > 0)
            {
                foreach (var cycle in cycles)
                {
                    cycle.FieldId = auditoriumId;
                }
            }
        }

        public void SetTennisTimeInitials(int stageId, int cycleNum, string timeInitial)
        {
            var cycles = db.TennisGameCycles.Where(g => g.StageId == stageId && g.CycleNum == cycleNum)?.ToList();
            if (cycles.Count > 0)
            {
                foreach (var cycle in cycles)
                {
                    cycle.TimeInitial = timeInitial;
                }
            }
        }

        private void InitPrivates()
        {
            usersRepo = new UsersRepo(db);
            bracketsRepo = new BracketsRepo(db, this);
            seasonsRepo = new SeasonsRepo();
            tRepo = new TeamsRepo(db);
            _gameCycles = db.GamesCycles.Include(t => t.HomeTeam)
                            .Include(t => t.GuestTeam)
                            .Include(t => t.Auditorium)
                            .Include(t => t.Stage)
                            .Include(t => t.Group)
                            .Include(t => t.Group.Stage.League.Games)
                            .Include(t => t.HomeTeam.TeamsDetails)
                            .Include(t => t.GuestTeam.TeamsDetails);
            googleMapsApiService = new GoogleMapsApiService();
        }

        public GamesRepo() : base()
        {
            InitPrivates();
        }
        public GamesRepo(DataEntities db)
            : base(db)
        {
            InitPrivates();
        }


        public Game GetById(int id)
        {
            return db.Games.Find(id);
        }

        public bool IsBasketBallOrWaterPoloGameCycle(int cycleId)
        {
            var game = GetGameCycleById(cycleId);
            var alias = game?.Stage?.League?.Union?.Section?.Alias;
            if (alias == null)
            {
                alias = game.Stage.League.Club.Section.Alias;
            }
            return alias == GamesAlias.BasketBall || alias == GamesAlias.WaterPolo || alias == GamesAlias.Soccer || alias == GamesAlias.Rugby || alias == GamesAlias.Softball;
        }

        public Game GetByLeagueStage(int leagueId, int stageId)
        {
            return db.Games.FirstOrDefault(t => t.LeagueId == leagueId && t.StageId == stageId);
        }

        public TennisGame GetTennisByCategoryStage(int categoryId, int stageId)
        {
            return db.TennisGames.FirstOrDefault(t => t.CategoryId == categoryId && t.StageId == stageId);
        }

        public Game GetByLeague(int leagueId)
        {
            return db.Games.FirstOrDefault(t => t.LeagueId == leagueId);
        }

        public void Create(Game item)
        {
            db.Games.Add(item);
        }

        public void CreateTennis(TennisGame item)
        {
            db.TennisGames.Add(item);
        }

        public IEnumerable<BaseGameDto> GetRefereeGames(User currentUser)
        {
            var waterpoloGames = db.GamesCycles.Where(r => r.GameStatus != GameStatus.Ended && r.IsPublished
                && r.Group.Stage.League.Union.Section.Alias == GamesAlias.WaterPolo
                || r.Group.Stage.League.Club.Union.Section.Alias == GamesAlias.WaterPolo
                || r.Group.Stage.League.Club.Section.Alias == GamesAlias.WaterPolo).OrderBy(r => r.StartDate);
            foreach (var game in waterpoloGames)
            {
                if (!string.IsNullOrEmpty(game.RefereeIds))
                {
                    var refereeIds = game.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.Select(int.Parse);
                    var spectatorIds = game.SpectatorIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.Select(int.Parse);
                    if (refereeIds?.Contains(currentUser.UserId) == true ||
                        spectatorIds?.Contains(currentUser.UserId) == true)
                    {
                        var league = game.Group.Stage.League;
                        var homeTeamDetailsName = game.HomeTeam?.TeamsDetails?.OrderByDescending(d => d.Id)?.FirstOrDefault()?.TeamName;
                        var guestTeamDetailsName = game.GuestTeam?.TeamsDetails?.OrderByDescending(d => d.Id)?.FirstOrDefault()?.TeamName;
                        yield return new BaseGameDto
                        {
                            GameId = game.CycleId,
                            CycleNumber = game.CycleNum,
                            GameCycleStatus = game.GameStatus,
                            StartDate = game.StartDate,
                            Auditorium = game.Auditorium?.Name,
                            HomeTeamId = game.HomeTeamId,
                            HomeTeamScore = game.HomeTeamScore,
                            HomeTeamTitle = homeTeamDetailsName ?? game.HomeTeam?.Title,
                            HomeTeamLogo = game.HomeTeam?.Logo,
                            GuestTeamId = game.GuestTeamId,
                            GuestTeamScore = game.GuestTeamScore,
                            GuestTeamTitle = guestTeamDetailsName ?? game.GuestTeam?.Title,
                            GuestTeamLogo = game.GuestTeam?.Logo,
                            LeagueId = league.LeagueId,
                            LeagueName = league.Name,
                            MaxHandicap = league.MaximumHandicapScoreValue,
                            IsHandicapEnabled = league.Union.IsHadicapEnabled
                        };
                    }
                }
            }
        }

        public Group GetGroupById(int id)
        {
            return db.Groups.Find(id);
        }

        public IEnumerable<GamesCycle> GetBySeasonLeagueAndStage(int seasonId, int leagueId, int? stageId)
        {
            return _gameCycles
                .Where(gc => gc.Stage.LeagueId == leagueId
                             && gc.Stage.League.SeasonId == seasonId
                             && (!stageId.HasValue || gc.StageId == stageId));
        }

        public IEnumerable<TennisGameCycle> GetTennisBySeasonCategoryAndStage(int seasonId, int categoryId, int? stageId)
        {
            return db.TennisGameCycles
                .Where(gc => gc.TennisStage.CategoryId == categoryId
                             && gc.TennisStage.SeasonId == seasonId
                             && (!stageId.HasValue || gc.StageId == stageId));
        }

        public IEnumerable<GamesCycle> GetGroupsCycles(int leagueId, int? seasonId = null)
        {
            var games = _gameCycles
                .Where(t => t.Stage.LeagueId == leagueId).AsQueryable();
            if (seasonId.HasValue)
            {
                var seasonGames = games.Where(x => x.Stage.League.SeasonId == seasonId.Value).ToList();

                foreach (var game in seasonGames)
                {
                    //if home team has details
                    var homeTeamDetails = game.HomeTeam?.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId.Value);
                    if (homeTeamDetails != null)
                    {
                        game.HomeTeam.Title = homeTeamDetails.TeamName;
                    }


                    //if guest team has detials
                    var guesTeamDetails = game.GuestTeam?.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId.Value);
                    if (guesTeamDetails != null)
                    {
                        game.GuestTeam.Title = guesTeamDetails.TeamName;
                    }
                }

                return seasonGames.OrderBy(g => g.StartDate)
                    .ThenBy(g => g.Group.Name)
                    .ThenBy(g => g.CycleNum)
                    .ToList();
            }

            games = games.OrderBy(g => g.StartDate)
                .ThenBy(g => g.Group.Name)
                .ThenBy(g => g.CycleNum);

            return games.ToList();
        }

        public IEnumerable<TennisGameCycle> GetTennisGroupsCycles(int categoryId, int? seasonId = null)
        {
            var games = db.TennisGameCycles
                .Where(t => t.TennisStage.CategoryId == categoryId).AsQueryable();
            if (seasonId.HasValue)
            {
                var seasonGames = games.Where(x => x.TennisStage.SeasonId == seasonId.Value).ToList();

                return seasonGames.OrderBy(g => g.StartDate)
                    .ThenBy(g => g.TennisGroup.Name)
                    .ThenBy(g => g.CycleNum)
                    .ToList();
            }

            games = games.OrderBy(g => g.StartDate)
                .ThenBy(g => g.TennisGroup.Name)
                .ThenBy(g => g.CycleNum);

            return games.ToList();
        }

        public void SetRefereesToNull(int cycleId)
        {
            db.GamesCycles.Find(cycleId).RefereeIds = null;
        }

        public void SetDescsToNull(int cycleId)
        {
            db.GamesCycles.Find(cycleId).DeskIds = null;
        }

        public void SaveSpectatorsByCycleId(string spectatorIds, int cycleId)
        {
            var gameCycle = db.GamesCycles.Find(cycleId);
            gameCycle.SpectatorIds = spectatorIds;
            db.SaveChanges();
        }

        public void EndGameForBasketballApp(int gameId)
        {
            var gc = db.GamesCycles.FirstOrDefault(g => g.CycleId == gameId);
            gc.GameStatus = GameStatus.Ended;
            if (gc.AppliedExclusionId.HasValue)
            {
                checkAllLeagueGamesExcludedDone(gc.PenaltyForExclusion);
            }
            UpdateGameScoreForBasketballApp(gc);
            bracketsRepo.GameEndedEvent(gc);
        }

        public string GetSectionAlias(int gameId)
        {
            var gc = db.GamesCycles.Find(gameId);
            return GetSectionAlias(gc);
        }

        public string GetSectionAlias(GamesCycle game)
        {
            var league = game?.Stage?.League;
            return league?.Club?.Section?.Alias ?? league?.Club?.Union?.Section?.Alias ?? league?.Union?.Section?.Alias
                ?? string.Empty;
        }

        private void UpdateGameScoreForBasketballApp(GamesCycle gc)
        {
            UpdateQuartersForBasketballApp(gc);
            Update(gc);
        }

        private void UpdateQuartersForBasketballApp(GamesCycle gc)
        {
            if (gc.Statistics.Any())
            {
                if (!gc.GameSets.Any())
                {
                    var gameQuartersDictionary = new Dictionary<string, IEnumerable<Statistic>>();
                    var gameQuarters = gc.Statistics.Select(s => s.TimeSegmentName).Distinct();
                    if (gameQuarters != null && gameQuarters.Any())
                    {
                        foreach (var quarter in gameQuarters)
                        {
                            gameQuartersDictionary.Add(quarter, gc.Statistics.Where(s => string.Equals(s.TimeSegmentName.Replace(" ", string.Empty),
                                quarter.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase)));
                        }
                        if (gameQuartersDictionary.Any())
                        {
                            var quarterNum = 1;
                            foreach (var gameQuarter in gameQuartersDictionary)
                            {
                                var homeScore = CalculateScoreForBasketballApp(gameQuarter.Value?.Where(s => s.TeamId == gc.HomeTeamId));
                                var guestScore = CalculateScoreForBasketballApp(gameQuarter.Value?.Where(s => s.TeamId == gc.GuestTeamId));
                                db.GameSets.Add(new GameSet
                                {
                                    GameCycleId = gc.CycleId,
                                    HomeTeamScore = homeScore,
                                    GuestTeamScore = guestScore,
                                    SetNumber = quarterNum,
                                    IsGoldenSet = false
                                });
                                quarterNum++;
                            }
                        }
                    }
                }
            }
        }

        public void UpdateGameScoreForPenaltySections(GamesCycle gc)
        {
            var homeScore = 0;
            var guestScore = 0;

            foreach (var set in gc.GameSets)
            {
                if (!set.IsPenalties && set.HomeTeamScore > set.GuestTeamScore)
                    homeScore += 1;
                if (!set.IsPenalties && set.HomeTeamScore < set.GuestTeamScore)
                    guestScore += 1;
                if (set.IsPenalties && set.HomeTeamScore > set.GuestTeamScore)
                    homeScore += 1;
                if (set.IsPenalties && set.HomeTeamScore < set.GuestTeamScore)
                    guestScore += 1;
            }

            gc.HomeTeamScore = homeScore;
            gc.GuestTeamScore = guestScore;
        }

        public int? GetUnionIdByGameCycle(int id)
        {
            var gameCycle = db.GamesCycles.Find(id);
            var league = gameCycle.Stage.League;
            return league?.UnionId ?? league?.Club?.UnionId;
        }

        private int CalculateScoreForBasketballApp(IEnumerable<Statistic> statistic)
        {
            int freeThrowPoints = statistic.Count(c => string.Equals(c.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.MadeFreeThrow, StringComparison.OrdinalIgnoreCase));
            int twoPoints = statistic.Count(c => string.Equals(c.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Made2Points, StringComparison.OrdinalIgnoreCase)) * 2;
            int threePoints = statistic.Count(c => string.Equals(c.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Made3Points, StringComparison.OrdinalIgnoreCase)) * 3;

            return freeThrowPoints + twoPoints + threePoints;
        }

        public IEnumerable<GamesCycle> GetTeamCycles(int teamId)
        {
            return _gameCycles
                .Where(t => t.HomeTeamId == teamId || t.GuestTeamId == teamId)
                .OrderBy(t => t.Stage.LeagueId)
                .ThenBy(g => g.StartDate)
                .ThenBy(g => g.Group.Name)
                .ThenBy(g => g.CycleNum)
                .ToList();
        }

        internal IEnumerable<GamesCycle> GetTeamCycles(int teamId, int leagueId)
        {
            IEnumerable<GamesCycle> allGames = GetTeamCycles(teamId);
            List<GamesCycle> orderdGames = new List<GamesCycle>();
            orderdGames.AddRange(allGames.Where(g => g.Stage.LeagueId == leagueId));
            orderdGames.AddRange(allGames.Where(g => g.Stage.LeagueId != leagueId));
            return orderdGames;
        }

        public List<GamesCycleDto> GetCyclesByLeague(int leagueId, bool needScore = false)
        {
            var gameCycles = GetGamesQuery(t => t.Stage.LeagueId == leagueId)
                .ToList();
            return ParseGameCycles(gameCycles, null, null, null, needScore);
        }

        public IEnumerable<GamesCycleDto> GetCyclesByAuditorium(int auditoriumId, int? seasonId)
        {
            var query = GetGamesQuery(g => g.AuditoriumId == auditoriumId && g.IsPublished)
                .ToList();
            return ParseGameCycles(query, seasonId);
        }

        public IEnumerable<GamesCycle> GetCyclesByFilterConditions(GameFilterConditions cond, bool userIsEditor, bool ignoreCondIfAllChecked = true, bool onlyPublished = false, bool onlyUnpublished = false)
        {
            var someLeaguesChecked = cond.leagues.Any(l => l.Check);
            var allLeaguesChecked = cond.leagues.All(l => l.Check) && ignoreCondIfAllChecked;
            var someAuditoriumsChecked = cond.auditoriums.Any(a => a.Check);
            var allAuditoriumsChecked = cond.auditoriums.All(a => a.Check) && ignoreCondIfAllChecked;
            var leaguesId = cond.leagues.Where(l => l.Check && l.Id > 0).Select(l => l.Id).ToList();
            var auditoriumsId = cond.auditoriums.Where(a => a.Check && a.Id > 0).Select(a => a.Id).ToList();

            IEnumerable<GamesCycle> result = new List<GamesCycle>();
            if (someLeaguesChecked || someAuditoriumsChecked)
            {
                result = GetGamesQuery(gc => (userIsEditor || gc.IsPublished)
                                             && (gc.Stage.League.SeasonId == cond.seasonId) && (!cond.onlyNotIsArchive || !gc.Stage.League.IsArchive)
                                             && (allLeaguesChecked || !someLeaguesChecked || leaguesId.Contains(gc.Stage.LeagueId))
                                             && (allAuditoriumsChecked || !someAuditoriumsChecked || auditoriumsId.Contains(gc.AuditoriumId ?? 0))
                                             && (!cond.dateFrom.HasValue || gc.StartDate >= cond.dateFrom)
                                             && (!cond.dateTo.HasValue || gc.StartDate <= cond.dateTo)
                    );
            }
            if (onlyPublished)
            {
                return result.Where(r => r.IsPublished);
            }
            if (onlyUnpublished)
            {
                return result.Where(r => !r.IsPublished);
            }
            return result;
        }

        public IEnumerable<TennisGameCycle> GetTennisCyclesByFilterConditions(GameFilterConditions cond, bool userIsEditor, bool ignoreCondIfAllChecked = true, bool onlyPublished = false, bool onlyUnpublished = false)
        {
            var someAuditoriumsChecked = cond.auditoriums.Any(a => a.Check);
            var allAuditoriumsChecked = cond.auditoriums.All(a => a.Check) && ignoreCondIfAllChecked;
            var auditoriumsId = cond.auditoriums.Where(a => a.Check && a.Id > 0).Select(a => a.Id).ToList();

            IEnumerable<TennisGameCycle> result = new List<TennisGameCycle>();
            result = GetTennisGamesQuery(gc => (userIsEditor || gc.IsPublished)
                                            && (gc.TennisStage.SeasonId == cond.seasonId)
                                            && (allAuditoriumsChecked || !someAuditoriumsChecked || auditoriumsId.Contains(gc.FieldId ?? 0))
                                            && (!cond.dateFrom.HasValue || gc.StartDate >= cond.dateFrom)
                                            && (!cond.dateTo.HasValue || gc.StartDate <= cond.dateTo));
            if (result == null || !result.Any())
            {
                result = GetTennisGamesQuery(gc => (userIsEditor || gc.IsPublished)
                                                && (gc.TennisStage.SeasonId == cond.seasonId)
                                                && (allAuditoriumsChecked || !someAuditoriumsChecked || auditoriumsId.Contains(gc.FieldId ?? 0))
                                            );
            }
            if (onlyPublished)
            {
                return result.Where(r => r.IsPublished);
            }
            if (onlyUnpublished)
            {
                return result.Where(r => !r.IsPublished);
            }
            return result;
        }

        public void DetermineTheTennisLeagueGameWinner(GamesCycle game, int homeScore, int guestScore, int homeSetsWon,
            int guestSetsWon, int homeGaming, int guestGaming, bool isDivision, out int? winnerId, out int? loserId)
        {
            winnerId = null;
            loserId = null;
            if (isDivision)
            {
                if (game.HomeTeamScore > game.GuestTeamScore) { winnerId = game.HomeTeamId; loserId = game.GuestTeamId; }
                else if (game.HomeTeamScore < game.GuestTeamScore) { winnerId = game.GuestTeamId; loserId = game.HomeTeamId; }
            }
            else
            {
                var bracket = game.PlayoffBracket;
                if (homeScore > guestScore)
                {
                    winnerId = game.HomeTeamId;
                    loserId = game.GuestTeamId;
                }
                else if (homeScore < guestScore)
                {
                    winnerId = game.GuestTeamId;
                    loserId = game.HomeTeamId;
                }
                else
                {
                    if (homeSetsWon > guestSetsWon)
                    {
                        winnerId = game.HomeTeamId;
                        loserId = game.GuestTeamId;
                    }
                    else if (homeSetsWon < guestSetsWon)
                    {
                        winnerId = game.GuestTeamId;
                        loserId = game.HomeTeamId;
                    }
                    else
                    {
                        if (homeGaming > guestGaming)
                        {
                            winnerId = game.HomeTeamId;
                            loserId = game.GuestTeamId;
                        }
                        else if (homeGaming < guestGaming)
                        {
                            winnerId = game.GuestTeamId;
                            loserId = game.HomeTeamId;
                        }
                    }
                }
            }
        }

        public PlayersStatisticsDTO UpdatePlayersWGameStat(PlayersStatisticsDTO stat)
        {
            var statDb = db.WaterpoloStatistics.FirstOrDefault(s => s.Id == stat.StatId);
            if (statDb != null)
            {
                statDb.MinutesPlayed = stat.MinsInFormat.ToMilisecondsFromString();
                statDb.GOAL = stat.Goal;
                statDb.PGOAL = stat.PGoal;
                statDb.Miss = stat.Miss;
                statDb.PMISS = stat.PMiss;
                statDb.AST = stat.AST;
                statDb.TO = stat.TO;
                statDb.STL = stat.STL;
                statDb.BLK = stat.BLK;
                statDb.OFFS = stat.Offs;
                statDb.FOUL = stat.Foul;
                statDb.EXC = stat.Exc;
                statDb.BFOUL = stat.BFoul;
                statDb.SSAVE = stat.SSave;
                statDb.YC = stat.YC;
                statDb.RD = stat.RD;
                statDb.EFF = stat.Goal + stat.PGoal + stat.AST + stat.BLK + stat.STL - (stat.Miss + stat.PMiss + stat.TO);
            }
            db.SaveChanges();

            return GetWPlayerStatisticDto(statDb);
        }

        public PlayersStatisticsDTO UpdatePlayersGameStat(PlayersStatisticsDTO stat)
        {
            var statDb = db.GameStatistics.FirstOrDefault(s => s.Id == stat.StatId);
            if (statDb != null)
            {
                statDb.MinutesPlayed = stat.MinsInFormat.ToMilisecondsFromString();
                statDb.FT = stat.FT;
                statDb.FTA = stat.FTA;
                statDb.TwoPT = stat.TwoPT;
                statDb.TwoPA = stat.TwoPA;
                statDb.ThreePT = stat.ThreePT;
                statDb.ThreePA = stat.ThreePA;
                statDb.FG = stat.TwoPT + stat.ThreePT;
                statDb.FGA = stat.TwoPA + stat.ThreePA;
                statDb.OREB = stat.OREB;
                statDb.DREB = stat.DREB;
                statDb.REB = stat.OREB + stat.DREB;
                statDb.AST = stat.AST;
                statDb.TO = stat.TO;
                statDb.STL = stat.STL;
                statDb.BLK = stat.BLK;
                statDb.PF = stat.PF;
                statDb.PTS = stat.FT + statDb.TwoPT * 2 + stat.ThreePT * 3;
                statDb.FGM = stat.TwoPT + stat.ThreePT;
                statDb.FTM = stat.TwoPA + stat.ThreePA;
                statDb.EFF = (statDb.PTS + statDb.REB + statDb.AST + statDb.STL + statDb.BLK - ((statDb.TwoPA - statDb.TwoPT) + (statDb.FTA - statDb.FT) + statDb.TO));
            }
            db.SaveChanges();

            return GetPlayerStatisticDto(statDb);
        }

        public List<GameReferee> GetSelectedRefereesForGame(int id)
        {
            var game = db.GamesCycles.FirstOrDefault(g => g.CycleId == id);
            var gameRefereesIds = game.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.Select(int.Parse)
                ?? Enumerable.Empty<int>();
            return db.Users.AsNoTracking().Where(u => gameRefereesIds.Contains(u.UserId))
                ?.Select(x => new GameReferee
                {
                    Id = x.UserId,
                    Name = x.FullName
                })?.ToList();
        }

        public void SaveReferees(int id, IEnumerable<int> ids)
        {
            try
            {
                var refereesIds = string.Join(",", ids);
                var game = db.GamesCycles.Find(id);


                if (game != null)
                {
                    if (!string.IsNullOrEmpty(refereesIds))
                    {
                        game.RefereeIds = refereesIds;
                    }
                    else if (string.IsNullOrEmpty(refereesIds) && !string.IsNullOrEmpty(game.RefereeIds))
                    {
                        game.RefereeIds = null;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private PlayersStatisticsDTO GetWPlayerStatisticDto(WaterpoloStatistic statDb)
        {
            return new PlayersStatisticsDTO
            {
                StatId = statDb.Id,
                PlayersId = statDb.PlayerId,
                PlayersName = statDb.TeamsPlayer.User.FullName,
                Min = statDb.MinutesPlayed,
                Goal = statDb.GOAL ?? 0,
                PGoal = statDb.PGOAL ?? 0,
                Miss = statDb.Miss ?? 0,
                PMiss = statDb.PMISS ?? 0,
                AST = statDb.AST ?? 0,
                STL = statDb.STL ?? 0,
                TO = statDb.TO ?? 0,
                BLK = statDb.BLK ?? 0,
                Offs = statDb.OFFS ?? 0,
                Foul = statDb.FOUL ?? 0,
                Exc = statDb.EXC ?? 0,
                BFoul = statDb.BFOUL ?? 0,
                SSave = statDb.SSAVE ?? 0,
                YC = statDb.YC ?? 0,
                RD = statDb.RD ?? 0,
                EFF = statDb.EFF ?? 0D,
                PlusMinus = statDb.DIFF ?? 0D
            };
        }

        private PlayersStatisticsDTO GetPlayerStatisticDto(GameStatistic statDb)
        {
            return new PlayersStatisticsDTO
            {
                StatId = statDb.Id,
                PlayersId = statDb.PlayerId,
                PlayersName = statDb.TeamsPlayer.User.FullName,
                Min = statDb.MinutesPlayed,
                FG = statDb.FG ?? 0,
                FGA = statDb.FGA ?? 0,
                ThreePT = statDb.ThreePT ?? 0,
                ThreePA = statDb.ThreePA ?? 0,
                TwoPT = statDb.TwoPT ?? 0,
                TwoPA = statDb.TwoPA ?? 0,
                FT = statDb.FT ?? 0,
                FTA = statDb.FTA ?? 0,
                DREB = statDb.DREB ?? 0,
                OREB = statDb.OREB ?? 0,
                REB = statDb.REB ?? 0,
                AST = statDb.AST ?? 0,
                STL = statDb.STL ?? 0,
                TO = statDb.TO ?? 0,
                BLK = statDb.BLK ?? 0,
                PF = statDb.PF ?? 0,
                PTS = statDb.PTS ?? 0,
                EFF = statDb.EFF ?? 0D,
                PlusMinus = statDb.DIFF ?? 0D
            };
        }

        public SchedulesDto GetSchedulesForExternalLink(int leagueId, int? seasonId = null, params int[] gameIds)
        {
            IQueryable<GamesCycle> query;
            //  Restrict games dataset by league
            var cond = PredicateBuilder.New<GamesCycle>(gc => gc.Stage.LeagueId == leagueId);
            //  If season is defined, then restrict by season as well
            if (seasonId.HasValue)
            {
                cond = cond.And(gc => gc.Stage.League.SeasonId == seasonId);
            }
            //  If certain set of games cycles is defined, then add this restriction
            if (gameIds?.Length > 0)
            {
                cond.And(gc => !gameIds.Contains(gc.CycleId));
            }
            //  For games belonging to Division groups, filter unpulished ones out.
            cond = cond.And(gc => gc.Group.GamesType.TypeId != GameTypeId.Division || gc.IsPublished);
            //  Limit groups to ones containing at least one published game each
            var groupCond = PredicateBuilder.New<Group>(g => !g.IsArchive && g.Stage.LeagueId == leagueId &&
                                                             g.GamesCycles.Any(gc => gc.IsPublished));
            //  Select only games belonging to groups chosen on previous step
            //  !!!Important!!! Groups are defined by names (one group might be represented by several
            //  records in Groups table having different GroupId, but they have the same Name)
            cond = cond.And(gc => db.Groups.Where(groupCond).Select(g => g.Name).Contains(gc.Group.Name));
            query = db.GamesCycles.Where(cond);

            //  Get game cycles data from DB
            var cycleList = query.ToList();
            bool? isIndividual = cycleList.FirstOrDefault()?.Group?.IsIndividual;

            foreach (var cycle in cycleList)
            {
                if (cycle.PlayoffBracket == null) continue;
                if (cycle.PlayoffBracket.Team1Id == null ||
                    (isIndividual.HasValue && isIndividual == true && cycle.PlayoffBracket.Athlete1Id == null))
                {
                    cycle.HomeTeamId = cycle.PlayoffBracket.Team1GroupPosition;
                }
                if (cycle.PlayoffBracket.Team2Id == null ||
                    (isIndividual.HasValue && isIndividual == true && cycle.PlayoffBracket.Athlete2Id == null))
                {
                    cycle.GuestTeamId = cycle.PlayoffBracket.Team2GroupPosition;
                }
            }
            //  Get events data from DB
            var events = db.Events.Where(e => e.LeagueId == leagueId && e.IsPublished)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    LeagueId = e.LeagueId,
                    EventTime = e.EventTime,
                    Title = e.Title,
                    Place = e.Place
                })
                .ToList();
            //  Form game cycles data transfer object
            var cycles = ParseGameCycles(cycleList, seasonId, events, isIndividual).ToList();

            //  Form brackets (they could contain unpublished games in Playoff and Knockout groups, but not in Division groups)
            var bracketsList = cycles
                .GroupBy(gc => gc.GameType == GameTypeId.Division ? GameType.Division : gc.GroupName)
                .Select(gr => new GroupBracketsDto
                {
                    GroupName = gr.Key,
                    GameTypeId = gr.First().GameType,
                    Stages = gr
                        .GroupBy(t => t.GameType == GameTypeId.Knockout34 || t.GameType == GameTypeId.Knockout34Consolences1Round || t.GameType == GameTypeId.Knockout34ConsolencesQuarterRound ? t.StageId.ToString() : t.StageName)
                        .Select(t => new StageCyclesDto
                        {
                            StageName = t.Key,
                            StageCustomName = t.FirstOrDefault()?.StageCustomName,
                            Items = gr.First().GameType == GameTypeId.Division
                                ? t.OrderBy(x => x.StartDate).ToList()
                                //  Knockout and Playoff games are temporary sorted by BracketIndex
                                //  because game ranks are calculated straight in the view. To be fixed. 
                                : t.OrderBy(x => x.BracketIndex).ToList()
                        }).ToList()
                }).ToList();

            var league = db.Leagues.FirstOrDefault(l => l.LeagueId == leagueId);

            return new SchedulesDto
            {
                //  Filter out all unpublished games from game list
                GameCycles = cycles.Where(gc => gc.IsPublished),
                BracketData = bracketsList,
                Events = events,
                gameAlias = league?.Union?.Section?.Alias ?? league?.Club?.Section?.Alias
            };
        }

        public void SaveNewPdfGameReport(GamesCycle game, string newFileName, string savePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(game.PdfGameReport) && !string.Equals(game.PdfGameReport, newFileName))
                {
                    if (File.Exists(savePath + game.PdfGameReport))
                    {
                        File.Delete(savePath + game.PdfGameReport);
                    }
                }
                game.PdfGameReport = newFileName;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<GamesCycleDto> GetCyclesByTeam(int teamId)
        {
            var gameCycles =
                GetGamesQuery(t => (t.GuestTeamId == teamId || t.HomeTeamId == teamId) && t.IsPublished)
                    .ToList();
            return ParseGameCycles(gameCycles);
        }

        private List<GamesCycleDto> ParseGameCycles(List<GamesCycle> gameCycles, int? seasonId = null,
            List<EventDto> events = null, bool? isIndividual = null, bool needScore = false)
        {
            var games = new List<GamesCycleDto>();
            var crossesStagesRanks = new Dictionary<int, int>(); // <stageId, rankCounter>

            foreach (var game in gameCycles)
            {
                var stageLeague = game?.Stage?.League;
                var sectionAlias = GetSectionAlias(game);
                var dto = new GamesCycleDto
                {
                    AuditoriumId = game.Auditorium?.AuditoriumId ?? 0,
                    Auditorium = game.Auditorium?.Name,
                    AuditoriumAddress = game.Auditorium?.Address,
                    CycleId = game.CycleId,
                    GameType = game.Group.TypeId,
                    LeagueId = game.Stage.LeagueId,
                    LeagueName = game.Stage.League.Name,
                    LeagueLogo = game.Stage.League.Logo,
                    SectionAlias = game.Stage.League.Union?.Section?.Alias ?? game.Stage.League.Club?.Section?.Alias,
                    GameStatus = game.GameStatus,
                    HomeTeamId = game.HomeTeamId,
                    HomeAthleteId = game.HomeAthleteId,
                    HomeLogo = game.HomeTeam?.Logo,
                    IsHomeTeamKnown = game.HomeTeam != null,
                    HomeTeamScore = game.IsPublished || needScore ? game.HomeTeamScore : -1,
                    GuestTeamId = game.GuestTeamId,
                    GuestAthleteId = game.GuestAthleteId,
                    GuesLogo = game.GuestTeam?.Logo,
                    IsGuestTeamKnown = game.GuestTeam != null,
                    GuestTeamScore = game.IsPublished || needScore ? game.GuestTeamScore : -1,
                    StageId = game.StageId,
                    StageName = $"שלב {game.Stage.Number}",
                    StageCustomName = game.Stage.Name,
                    IsCrossesStage = game.Stage.IsCrossesStage,
                    IsNotSetYet = game.IsNotSetYet,
                    GroupId = game.GroupId.Value,
                    GroupName = game.Group.Name,
                    StartDate = game.StartDate,
                    MaxPlayoffPos = game.MaxPlayoffPos,
                    MinPlayoffPos = game.MinPlayoffPos,
                    IsPublished = game.IsPublished,
                    EventRef = events?.Where(ev => ev.LeagueId == game.Stage.LeagueId
                                                   && ev.EventTime <= game.StartDate)
                        .OrderByDescending(ev => ev.EventTime).FirstOrDefault(),
                    WinnerId = game.TechnicalWinnnerId ?? game.PlayoffBracket?.WinnerId,
                    IsTennisLeagueGame = sectionAlias.Equals(GamesAlias.Tennis) &&
                                         (stageLeague?.EilatTournament == null || stageLeague?.EilatTournament == false),
                    PdfGameReport = game.PdfGameReport,
                    RoundNum = game.RoundNum,
                    CycleNum = game.CycleNum,
                    IsDivision = game.Group.TypeId == GameTypeId.Division,
                    HasHomeTennisTechWinner = game.TennisLeagueGames.Any(g => g.HomePlayerId == g.TechnicalWinnerId),
                    HasGuestTennisTechWinner = game.TennisLeagueGames.Any(g => g.GuestPlayerId == g.TechnicalWinnerId),
                    Remark = game.Remark
                };

                if (dto.IsCrossesStage)
                {
                    var currentRank = crossesStagesRanks.ContainsKey(dto.StageId)
                        ? crossesStagesRanks[dto.StageId]
                        : 1;

                    if (crossesStagesRanks.ContainsKey(dto.StageId))
                    {
                        currentRank += 2;

                        crossesStagesRanks[dto.StageId] = currentRank;
                    }
                    else
                    {
                        crossesStagesRanks.Add(dto.StageId, currentRank);
                    }

                    dto.CrossesRank = currentRank;
                }

                if (game.BracketIndex.HasValue)
                {
                    dto.BracketIndex = game.BracketIndex.Value;
                }

                if (game.GameSets.Any())
                {
                    dto.BasketBallWaterpoloHomeTeamScore = dto.IsPublished || needScore ? game.GameSets.Where(c => !c.IsPenalties).Sum(x => x.HomeTeamScore) : -1;
                    dto.BasketBallWaterpoloGuestTeamScore = dto.IsPublished || needScore ? game.GameSets.Where(c => !c.IsPenalties).Sum(x => x.GuestTeamScore) : -1;
                }

                var penalty = game.GameSets.FirstOrDefault(x => x.IsPenalties);
                if (penalty != null)
                {
                    dto.PenaltyHomeTeamScore = penalty.HomeTeamScore;
                    dto.PenaltyGuestTeamScore = penalty.GuestTeamScore;
                    dto.HomeTeamScore = dto.IsPublished || needScore ? game.GameSets.Sum(x => x.HomeTeamScore) - penalty.HomeTeamScore : -1;
                    dto.GuestTeamScore = dto.IsPublished || needScore ? game.GameSets.Sum(x => x.GuestTeamScore) - penalty.GuestTeamScore : -1;
                }

                if (seasonId.HasValue)
                {
                    if (isIndividual.HasValue && isIndividual == true)
                    {
                        var guestAthleteDetails = game.TeamsPlayer;
                        if (guestAthleteDetails != null)
                        {
                            game.TeamsPlayer.User.FirstName = guestAthleteDetails.User.FirstName;
                            game.TeamsPlayer.User.LastName = guestAthleteDetails.User.LastName;
                            game.TeamsPlayer.User.MiddleName = guestAthleteDetails.User.MiddleName;
                        }

                        var homeAthleteDetails = game.TeamsPlayer1;
                        if (homeAthleteDetails != null)
                        {
                            game.TeamsPlayer1.User.FirstName = homeAthleteDetails.User.FirstName;
                            game.TeamsPlayer1.User.LastName = homeAthleteDetails.User.LastName;
                            game.TeamsPlayer1.User.MiddleName = homeAthleteDetails.User.MiddleName;
                        }
                    }
                    else
                    {
                        var guestTeamDetails = game.GuestTeam?.TeamsDetails.FirstOrDefault(f => f.SeasonId == seasonId.Value);
                        if (guestTeamDetails != null)
                        {
                            game.GuestTeam.Title = guestTeamDetails.TeamName;
                        }

                        var homeTeamDetails = game.HomeTeam?.TeamsDetails.FirstOrDefault(f => f.SeasonId == seasonId.Value);
                        if (homeTeamDetails != null)
                        {
                            game.HomeTeam.Title = homeTeamDetails.TeamName;
                        }
                    }
                }
                if (isIndividual.HasValue && isIndividual == true)
                {
                    dto.HomeTeam = game.TeamsPlayer1 != null
                        ? $"{game.TeamsPlayer1.User.FullName}"
                        : $"Position# {game.HomeTeamPos}";
                    dto.GuestTeam = game.TeamsPlayer != null
                        ? $"{game.TeamsPlayer.User.FullName}"
                        : $"Position# {game.GuestTeamPos}";
                }
                else
                {
                    dto.HomeTeam = game.HomeTeam != null ? game.HomeTeam.Title : $"Position# {game.HomeTeamPos}";
                    dto.GuestTeam = game.GuestTeam != null ? game.GuestTeam.Title : $"Position# {game.GuestTeamPos}";
                }

                if (game.HomeTeamPos == null && game.PlayoffBracket != null && game.PlayoffBracket.ParentBracket1Id == null && !game.HomeTeamId.HasValue ||
                    isIndividual.HasValue && isIndividual == true && game.HomeTeamPos == null &&
                    game.PlayoffBracket != null && game.PlayoffBracket.ParentBracket1Id == null &&
                    !game.HomeAthleteId.HasValue)
                {
                    dto.HomeTeam = "--";
                }
                if (game.GuestTeamPos == null && game.PlayoffBracket != null && game.PlayoffBracket.ParentBracket2Id == null && !game.GuestTeamId.HasValue ||
                    isIndividual.HasValue && isIndividual == true &&
                    game.GuestTeamPos == null && game.PlayoffBracket != null && game.PlayoffBracket.ParentBracket2Id == null && !game.GuestAthleteId.HasValue)
                {
                    dto.GuestTeam = "--";
                }

                if (game.BracketId.HasValue && game.PlayoffBracket != null)
                {
                    dto.Bracket = new BracketDto
                    {
                        Id = game.BracketId.Value,
                        Type = game.PlayoffBracket.Type
                    };
                }

                if (game.Group.IsAdvanced)
                {
                    dto.IsAdvanced = true;
                    int numOfBrackets = game.Group.PlayoffBrackets.Count(b => b.Type != (int)PlayoffBracketType.Loseer && b.Type != (int)PlayoffBracketType.Condolence3rdPlaceBracket);
                    switch (numOfBrackets)
                    {
                        case 1:
                            dto.StageName = "גמר";
                            break;
                        case 2:
                            dto.StageName = "חצי גמר";
                            break;
                        case 4:
                            dto.StageName = "רבע גמר";
                            break;
                        case 8:
                            dto.StageName = "שמינית גמר";
                            break;
                        default:
                            dto.StageName = (numOfBrackets * 2) + " אחרונות";
                            break;
                    }

                    dto.IndexInBracket = game.PlayoffBracket.GamesCycles.ToList().IndexOf(game);

                    switch (game.PlayoffBracket.Type)
                    {
                        case (int)PlayoffBracketType.Root:
                            dto.IsRoot = true;
                            if (isIndividual.HasValue && isIndividual == true)
                            {
                                if (game.TeamsPlayer1 == null)
                                {
                                    dto.HomeTeam = game.HomeTeamPos == null ? "--" : "Position# " + game.HomeTeamPos;
                                }
                                if (game.TeamsPlayer == null)
                                {
                                    dto.GuestTeam = game.GuestTeamPos == null ? "--" : "Position# " + game.GuestTeamPos;
                                }
                            }
                            else
                            {
                                if (game.HomeTeam == null)
                                {
                                    dto.HomeTeam = game.HomeTeamPos == null ? "--" : "Position# " + game.HomeTeamPos;
                                }
                                if (game.GuestTeam == null)
                                {
                                    dto.GuestTeam = game.GuestTeamPos == null ? "--" : "Position# " + game.GuestTeamPos;
                                }
                            }

                            break;

                        case (int)PlayoffBracketType.Winner:

                            if (isIndividual.HasValue && isIndividual == true)
                            {
                                if (game.TeamsPlayer1 == null)
                                {
                                    if (game.PlayoffBracket?.Parent1 != null && game.PlayoffBracket.Parent1.Team2GroupPosition == 0)
                                    {
                                        if (game.PlayoffBracket.Parent1.TeamsPlayer == null)
                                            dto.HomeTeam = "Position #" + game.PlayoffBracket.Parent1.Team1GroupPosition;
                                        else
                                        {
                                            dto.HomeTeam = $"{game.PlayoffBracket.Parent1.TeamsPlayer.User.FullName} " +
                                                $"({game.PlayoffBracket.Parent1.TeamsPlayer.Team.Title})";
                                            dto.HomeAthleteId = game.PlayoffBracket.Parent1.TeamsPlayer.Id;
                                            dto.IsHomeTeamKnown = true;
                                        }
                                    }
                                    else
                                    {
                                        dto.HomeTeam = "מנצחת";
                                    }
                                }
                                if (game.TeamsPlayer == null)
                                {
                                    dto.GuestTeam = "מנצחת";
                                }
                            }
                            else
                            {
                                if (game.HomeTeam == null)
                                {
                                    if (game.PlayoffBracket?.Parent1 != null && game.PlayoffBracket.Parent1.Team2GroupPosition == 0)
                                    {
                                        if (game.PlayoffBracket.Parent1.Team1 == null)
                                            dto.HomeTeam = "Position #" + game.PlayoffBracket.Parent1.Team1GroupPosition;
                                        else
                                        {
                                            dto.HomeTeam = game.PlayoffBracket.Parent1.Team1.Title;
                                            dto.HomeTeamId = game.PlayoffBracket.Parent1.Team1.TeamId;
                                            dto.IsHomeTeamKnown = true;
                                        }
                                    }
                                    else
                                    {
                                        dto.HomeTeam = "מנצחת";
                                    }
                                }
                                if (game.GuestTeam == null)
                                {
                                    dto.GuestTeam = "מנצחת";
                                }
                            }

                            break;
                        case (int)PlayoffBracketType.Loseer:
                        case (int)PlayoffBracketType.Condolence3rdPlaceBracket:
                            if (isIndividual.HasValue && isIndividual == true)
                            {
                                if (game.TeamsPlayer1 == null)
                                {
                                    dto.HomeTeam = "מפסידה";
                                }
                                if (game.TeamsPlayer == null)
                                {
                                    dto.GuestTeam = "מפסידה";
                                }
                            }
                            else
                            {
                                if (game.HomeTeam == null)
                                {
                                    dto.HomeTeam = "מפסידה";
                                }
                                if (game.GuestTeam == null)
                                {
                                    dto.GuestTeam = "מפסידה";
                                }
                            }
                            break;
                    }
                }
                games.Add(dto);
            }

            return games;
        }

        private List<TeamsPlayer> GetTeamPlayersByTeamIds(int userId, List<int> ids, int seasonId)
        {
            return db.TeamsPlayers.Where(tp => ids.Contains(tp.TeamId) && tp.SeasonId == seasonId && tp.UserId == userId).ToList();
        }



        public List<TennisLeagueGameForm> GetAllTennisGamesForUser(int id, int teamId, int seasonId, bool isHebrew)
        {
            var result = new List<TennisLeagueGameForm>();
            var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
            var leagueGames = db.GamesCycles
                .Where(t => (t.TennisLeagueGames.Any(tg =>
                    tg.HomePairPlayerId == id ||
                    tg.GuestPairPlayerId == id ||
                    tg.HomePlayerId == id ||
                    tg.GuestPlayerId == id)) && t.IsPublished)
                .Include(x => x.TennisLeagueGames)
                //.Include(x => x.TennisLeagueGames.Select(g => g.HomePlayer))
                //.Include(x => x.TennisLeagueGames.Select(g => g.HomePairPlayer))
                //.Include(x => x.TennisLeagueGames.Select(g => g.GuestPlayer))
                //.Include(x => x.TennisLeagueGames.Select(g => g.GuestPairPlayer))
                .Include(x => x.Group.Stage.League)
                .ToList();

            var competitionGames = db.TennisGameCycles
                .Where(t =>
                    ((t.FirstPlayerId.HasValue && t.TeamsPlayer.UserId == id) ||
                    (t.SecondPlayerId.HasValue && t.TeamsPlayer1.UserId == id) ||
                    (t.FirstPlayerPairId.HasValue && t.TeamsPlayer11.UserId == id) ||
                    (t.SecondPlayerPairId.HasValue && t.TeamsPlayer3.UserId == id)) && t.IsPublished)
                .Include(x => x.TennisStage.Team.LeagueTeams)
                //.Include(x => x.FirstPlayer)
                //.Include(x => x.FirstPlayer.User)
                //.Include(x => x.FirstPairPlayer)
                //.Include(x => x.FirstPairPlayer.User)
                //.Include(x => x.SecondPlayer)
                //.Include(x => x.SecondPlayer.User)
                //.Include(x => x.SecondPairPlayer)
                //.Include(x => x.SecondPairPlayer.User)
                .ToList();

            var competitionRegistrations = db.CompetitionRegistrations
                .Where(r => r.IsActive &&
                            r.IsRegisteredByExcel &&
                            r.UserId == id)
                .Include(x => x.League)
                .GroupBy(t => t.LeagueId)
                .Select(t => t.FirstOrDefault())
                .ToList();

            if (leagueGames.Any())
            {
                foreach (var leagueGame in leagueGames)
                {
                    foreach (var set in leagueGame.TennisLeagueGames.ToList())
                    {
                        if (set.HomePlayerId == id || set.GuestPlayerId == id || set.HomePairPlayerId == id || set.GuestPairPlayerId == id)
                        {
                            GetSetResultForLeague(id, set, isHebrew, out string setScore, out ResultType resultType);
                            GetNamesOfPlayersForTennisLeague(id, set, out string opponentName, out string partnerName);

                            result.Add(new TennisLeagueGameForm
                            {
                                Id = set.Id,
                                CompetionType = CompetitionType.League,
                                CompetitionName = leagueGame.Group.Stage.League.Name,
                                DateOfGame = leagueGame.StartDate,
                                OpponentName = opponentName,
                                PartnerName = partnerName,
                                ResultScore = setScore,
                                ResultType = resultType
                            });
                        }
                    }
                }
            }

            if (competitionGames.Any())
            {
                foreach (var competitionGame in competitionGames)
                {
                    GetSetResultForCompetition(id, competitionGame, isHebrew, out string setScore, out ResultType resultType);

                    result.Add(new TennisLeagueGameForm
                    {
                        Id = competitionGame.CycleId,
                        CompetitionName = competitionGame?.TennisStage?.Team?.LeagueTeams?.FirstOrDefault()?.Leagues?.Name
                                          ?? string.Empty,
                        DateOfGame = competitionGame.StartDate,
                        OpponentName = competitionGame.FirstPlayer?.UserId == id || competitionGame.FirstPairPlayer?.UserId == id
                            ? competitionGame.SecondPlayerPairId.HasValue
                                ? $"{competitionGame.SecondPlayer?.User?.FullName} / {competitionGame.SecondPairPlayer?.User?.FullName} "
                                : competitionGame.SecondPlayer?.User?.FullName
                            : competitionGame.FirstPlayerPairId.HasValue
                                ? $"{competitionGame.FirstPlayer?.User?.FullName} / {competitionGame.FirstPairPlayer?.User?.FullName} "
                                : competitionGame.FirstPlayer?.User?.FullName,
                        CompetionType = CompetitionType.Competition,
                        PartnerName = GetPartnerNameForCompetitionUser(id, competitionGame),
                        ResultScore = setScore,
                        ResultType = resultType,
                        CompetitionId = competitionGame?.TennisStage?.Team?.LeagueTeams?.FirstOrDefault()?.LeagueId ?? 0,
                        CategoryId = competitionGame?.TennisStage?.Team?.TeamId ?? 0
                    });
                }
            }

            if (competitionRegistrations.Any())
            {
                foreach (var reg in competitionRegistrations)
                {
                    result.Add(new TennisLeagueGameForm
                    {
                        CompetitionName = reg.League.Name,
                        CompetionType = CompetitionType.Competition,
                    });
                }
            }
            if (season.UnionId.HasValue)
            {
                var competitions = GetFinishedTennisCompeitions(season.UnionId.Value, seasonId, 16);
                var teamIds = new List<int>();
                foreach (var competition in competitions)
                {
                    if (competition.IsCompetitionNotLeague)
                    {
                        if (!competition.IsDailyCompetition)
                        {
                            teamIds.AddRange(competition.CategoryIds);
                        }
                    }
                }
                var playersCompetitionParticipation = new Dictionary<int, Dictionary<int, int>>();
                var dailyCompetitions = competitions.Where(c => c.IsDailyCompetition).ToList();

                var dailyCategoriesIds = new List<int>();
                foreach (var competition in dailyCompetitions)
                {
                    dailyCategoriesIds.AddRange(competition.CategoryIds);
                }
                var allTeamPlayersInAllCategories = GetTeamPlayersByTeamIds(id, dailyCategoriesIds, seasonId);
                foreach (var competition in dailyCompetitions)
                {
                    var isUserParticipatingInDaily = false;
                    //var playersInCategory = GetTeamPlayers(categoryId, 0, competition.Id.Value, seasonId);
                    var test = allTeamPlayersInAllCategories.Where(tp => tp.LeagueId == competition.Id.Value && competition.CategoryIds.Contains(tp.TeamId)).ToList();
                    isUserParticipatingInDaily = allTeamPlayersInAllCategories.Any(tp => tp.LeagueId == competition.Id.Value && competition.CategoryIds.Contains(tp.TeamId));
                    /*
                    foreach (var categoryId in competition.CategoryIds)
                    {
                        var playersInCategory = GetTeamPlayers(categoryId, 0, competition.Id.Value, seasonId);
                        isUserParticipatingInDaily = isUserParticipatingInDaily || playersInCategory.Any(t => t.UserId == id);
                    }
                    */
                    if (isUserParticipatingInDaily)
                    {
                        var compLeagueForm = new TennisLeagueGameForm
                        {
                            CompetitionName = competition.Name,
                            CompetionType = CompetitionType.DailyCompetition,
                            DateOfGame = competition.StartDate
                        };
                        result.Add(compLeagueForm);
                    }
                }

            }


            return result;
        }


        private List<TotoCompetition> GetFinishedTennisCompeitions(int unionId, int seasonId, int excelPosition = 0)
        {
            var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
            var prevSeason = db.Seasons.FirstOrDefault(s => s.Id == season.PreviousSeasonId);
            var hasPrevSeason = true;
            if (prevSeason == null)
            {
                hasPrevSeason = false;
            }
            var currentSeasonYear = season.EndDate.Year;
            var currentSeasonMaxDate = new DateTime(currentSeasonYear, 9, 1);
            var previousSeasonMinDate = new DateTime(currentSeasonYear - 1, 9, 1);
            var sectionAlias = db.Unions.Find(unionId)?.Section?.Alias;
            var competitionList = db.Leagues.Where(x => ((x.SeasonId == seasonId && ((x.LeagueStartDate.HasValue && x.LeagueStartDate < currentSeasonMaxDate) || (!x.LeagueStartDate.HasValue && x.EndRegistrationDate < currentSeasonMaxDate))) || (hasPrevSeason && prevSeason.Id == x.SeasonId && ((x.LeagueStartDate.HasValue && x.LeagueStartDate >= previousSeasonMinDate) || (!x.LeagueStartDate.HasValue && x.EndRegistrationDate >= previousSeasonMinDate)))) && (x.UnionId == unionId || x.Club.UnionId == unionId)).ToList();
            return GetTotoCompetitionsValues(competitionList, seasonId, excelPosition);
        }

        private List<TotoCompetition> GetTotoCompetitionsValues(List<League> competitionList, int seasonId, int excelPosition = 0)
        {
            var listOfCompetitions = new List<TotoCompetition>();
            var count = 0;
            if (competitionList.Any())
            {
                foreach (var competition in competitionList)
                {
                    listOfCompetitions.Add(new TotoCompetition
                    {
                        Id = competition.LeagueId,
                        Name = competition.Name,
                        StartDate = competition.LeagueStartDate,
                        ExcelPosition = count + excelPosition,
                        IsCompetitionNotLeague = competition.EilatTournament ?? false,
                        IsDailyCompetition = competition.IsDailyCompetition,
                        CategoryIds = competition.LeagueTeams.Where(t => t.SeasonId == seasonId).Select(t => t.TeamId).ToList()
                    }); ;
                    count++;
                }
            }
            return listOfCompetitions;
        }

        private IEnumerable<TeamPlayerItem> GetTeamPlayers(int teamId, int clubId, int leagueId, int seasonId)
        {
            var league = db.Leagues.Find(leagueId);
            var season = db.Seasons.Find(seasonId);

            var section = season?.Union != null ? season.Union.Section.Alias : null;

            var club = db.Clubs.Find(clubId);
            var IsHandicapEnabled = club?.Union?.IsHadicapEnabled;

            var query = db.Users.SelectMany(user => user.TeamsPlayers, (user, teamsPlayer) => new { user, teamsPlayer })
                .Where(t => !t.user.IsArchive &&
                       t.teamsPlayer.TeamId == teamId &&
                       t.teamsPlayer.SeasonId == seasonId &&
                        (leagueId > 0 ? t.teamsPlayer.LeagueId == leagueId : t.teamsPlayer.LeagueId == null) &&
                        (clubId > 0 ? t.teamsPlayer.ClubId == clubId : t.teamsPlayer.ClubId == null));

            if (section != null && section == GamesAlias.Athletics)
            {
                query = query.Where(t => t.teamsPlayer.IsActive == true);
            }
            var users = query.ToList();

            var usersIds = users.Select(x => x.user.UserId).ToArray();
            var playersActivitiesRegistrations = db.ActivityFormsSubmittedDatas
                .Where(x => usersIds.Contains(x.PlayerId) && x.Activity.SeasonId == seasonId)
                .OrderBy(x => x.ActivityId)
                .ToList();

            var playersFiles = db.PlayerFiles
                .Where(x => usersIds.Contains(x.PlayerId) && x.SeasonId == seasonId)
                .ToList();

            return users.Select(t =>
            {
                ActivityFormsSubmittedData registration = null;
                if ((section == null || section != GamesAlias.Athletics) && clubId > 0 && league == null)
                {
                    var clubRegistrations = playersActivitiesRegistrations
                        .Where(
                            x => x.PlayerId == t.user.UserId &&
                                 x.TeamId == t.teamsPlayer.TeamId &&
                                 x.ClubId == clubId &&
                                 x.Activity.SeasonId == seasonId)
                        .ToList();

                    registration = clubRegistrations.FirstOrDefault(
                                       x => x.Activity.IsAutomatic == true && x.Activity.Type == ActivityType.Personal)
                                   ?? clubRegistrations.FirstOrDefault(
                                       x => x.Activity.IsAutomatic != true && x.Activity.Type == ActivityType.Personal)
                                   ?? clubRegistrations.FirstOrDefault(
                                       x => x.Activity.IsAutomatic == true && x.Activity.Type == ActivityType.UnionPlayerToClub);

                }
                else if (section == null || section != GamesAlias.Athletics)
                {
                    var leagueRegistrations = playersActivitiesRegistrations
                        .Where(
                            x => x.PlayerId == t.user.UserId &&
                                 x.TeamId == t.teamsPlayer.TeamId &&
                                 x.LeagueId == leagueId &&
                                 x.Activity.SeasonId == seasonId)
                        .ToList();

                    registration = leagueRegistrations.FirstOrDefault(
                                       x => x.Activity.IsAutomatic == true && x.Activity.Type == ActivityType.Personal)
                                   ?? leagueRegistrations.FirstOrDefault(
                                       x => x.Activity.IsAutomatic != true && x.Activity.Type == ActivityType.Personal)
                                   ?? leagueRegistrations.FirstOrDefault(
                                       x => x.Activity.IsAutomatic == true && x.Activity.Type == ActivityType.UnionPlayerToClub);
                }

                decimal managerDiscount = 0M;
                if ((section == null || section != GamesAlias.Athletics) && clubId > 0)
                {
                    managerDiscount = t.user.PlayerDiscounts
                                          .FirstOrDefault(
                                              d => d.TeamId == teamId &&
                                                   d.ClubId == clubId &&
                                                   d.SeasonId == seasonId &&
                                                   d.DiscountType == (int)PlayerDiscountTypes.ManagerParticipationDiscount)
                                          ?.Amount ?? 0M;
                }
                else if (section == null || section != GamesAlias.Athletics)
                {
                    managerDiscount = t.user.PlayerDiscounts
                                          .FirstOrDefault(
                                              d => d.TeamId == teamId &&
                                                   d.LeagueId == leagueId &&
                                                   d.SeasonId == seasonId &&
                                                   d.DiscountType == (int)PlayerDiscountTypes.ManagerRegistrationDiscount)
                                          ?.Amount ?? 0M;
                }

                var unionId = league?.UnionId;

                var gymnasticCompetitionCount = (section == GamesAlias.Gymnastic) ? t.user.CompetitionRegistrations
                                    .Where(c => c.SeasonId == seasonId && !c.League.IsArchive && c.IsActive && (c.FinalScore.HasValue || c.Position.HasValue))
                                    .GroupBy(r => r.LeagueId)
                                    .Select(r => r.First())
                                    .Count() : 0;

                int regularCompetitionCount = (section == GamesAlias.WeightLifting) ? t.user.CompetitionDisciplineRegistrations
                    .Where(c => c.CompetitionDiscipline.League.SeasonId == seasonId && !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive && c.IsApproved.HasValue && c.IsApproved.Value)
                    .GroupBy(r => r.CompetitionDiscipline.League.LeagueId)
                    .Select(r => r.First())
                    .Count() : 0;

                int athleticRegistrationCount = 0;
                if (section == GamesAlias.Athletics)
                {
                    int AlternativeResultInt = 3; // value for alternative result column if player did not start/show
                    athleticRegistrationCount = t.user.CompetitionDisciplineRegistrations
                    .Where(c => c.CompetitionDiscipline.League.SeasonId == seasonId && !c.CompetitionDiscipline.League.IsArchive && !c.IsArchive && !c.CompetitionDiscipline.IsDeleted && c.CompetitionResult.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(c.CompetitionResult.FirstOrDefault().Result) && c.CompetitionResult.FirstOrDefault().AlternativeResult != AlternativeResultInt)
                    .GroupBy(r => r.CompetitionDiscipline.League.LeagueId)
                    .Select(r => r.First())
                    .Count();
                }

                var otherCompetitionCount = t.user.SportsRegistrations
                    .Where(c => c.SeasonId == seasonId && !c.League.IsArchive && c.IsApproved || (c.FinalScore.HasValue || c.Position.HasValue))
                    .GroupBy(r => r.LeagueId)
                    .Select(r => r.First())
                    .Count();

                var competitionCount = gymnasticCompetitionCount + athleticRegistrationCount + regularCompetitionCount + otherCompetitionCount;
                if (section == GamesAlias.Tennis)
                {
                    competitionCount = t.teamsPlayer.CompetitionParticipationCount ?? 0;
                }

                var teamReg = db.TeamRegistrations.FirstOrDefault(tr => !tr.IsDeleted && tr.ClubId == clubId && tr.TeamId == teamId && !tr.League.IsArchive);


                decimal _;
                var playerFiles = playersFiles.Where(x => x.PlayerId == t.user.UserId && x.SeasonId == seasonId).ToList();
                var res = new TeamPlayerItem
                {
                    Id = t.teamsPlayer.Id,
                    Weight = t.user.Weight,
                    IsApproveChecked = t.teamsPlayer.IsApprovedByManager == true,
                    IsNotApproveChecked = t.teamsPlayer.IsApprovedByManager == false,
                    WeightUnits = t.user.WeightUnits,
                    UserId = t.teamsPlayer.UserId,
                    ShirtNum = t.teamsPlayer.ShirtNum,
                    PosId = t.teamsPlayer.PosId,
                    FullName = t.user.FullName,
                    IdentNum = t.user.IdentNum,
                    PassportNum = t.user.PassportNum,
                    GenderId = t.user.GenderId,
                    Gender = t.user.Gender,
                    AthletesNumbers = t.user.AthleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId).AthleteNumber1,
                    IsActive = t.teamsPlayer.IsActive,
                    TeamId = teamId,
                    TeamName = t.teamsPlayer.Team?.TeamsDetails?.FirstOrDefault(c => c.SeasonId == seasonId)?.TeamName
                                ?? t.teamsPlayer.Team?.Title ?? "",
                    SeasonId = seasonId,
                    Birthday = t.user.BirthDay,
                    City = t.user.City,
                    Email = t.user.Email,
                    Insurance = t.user.Insurance,
                    InsuranceFile = playerFiles.Where(x => x.FileType == (int)PlayerFileType.Insurance)
                        .Select(x => x.FileName)
                        .FirstOrDefault(),
                    MedicalCertificate = t.user.MedicalCertApprovements.FirstOrDefault(x => x.SeasonId == seasonId)?.Approved == true,
                    MedicalCertificateFile = playerFiles.Where(x => x.FileType == (int)PlayerFileType.MedicalCertificate && !x.IsArchive)
                        .Select(x => x.FileName)
                        .FirstOrDefault(),
                    ShirtSize = t.user.ShirtSize,
                    Telephone = t.user.Telephone,
                    AthleteNumber = t.user.AthleteNumbers.FirstOrDefault(x => x.SeasonId == seasonId).AthleteNumber1,
                    IsLocked = t.teamsPlayer.IsLocked,

                    IsPlayerRegistered = registration != null,

                    IsPlayerRegistrationApproved = registration?.IsActive == true,

                    PlayerImage = playerFiles.Where(x => x.FileType == (int)PlayerFileType.PlayerImage)
                                      .Select(x => x.FileName)
                                      .FirstOrDefault() ?? t.user.Image,

                    Registration = registration,

                    ManagerParticipationDiscount = clubId > 0 ? managerDiscount : 0,
                    ManagerRegistrationDiscount = clubId <= 0 ? managerDiscount : 0,

                    NoInsurancePayment = t.user.NoInsurancePayment,
                    SeasonIdOfCreation = t.user.SeasonIdOfCreation,
                    IsTrainerPlayer = t.teamsPlayer.IsTrainerPlayer,
                    IsEscortPlayer = t.teamsPlayer.IsEscortPlayer,
                    IsApprovedByManager = t.teamsPlayer.IsApprovedByManager,
                    IsBlockaded = t.user?.BlockadeId != null,
                    StartPlaying = t.teamsPlayer.StartPlaying,
                    BaseHandicap = t.teamsPlayer.HandicapLevel,
                    Comment = t.teamsPlayer.Comment,
                    UnionComment = t.teamsPlayer.UnionComment,
                    TeamPlayerPaid = t.teamsPlayer.Paid,
                    IsExceptionalMoved = t.teamsPlayer.IsExceptionalMoved,
                    IsUnderPenalty = (section == null || section != GamesAlias.Athletics) && t.user.PenaltyForExclusions.Where(c => !c.IsCanceled).Any(c => !c.IsEnded),
                    TennisPositionOrder = t.teamsPlayer.TennisPositionOrder,
                    NextTournamentRoster = t.teamsPlayer.NextTournamentRoster,
                    TenicardValidity = t.user.TenicardValidity,
                    MedExamDate = t.user.MedExamDate,
                    CompetitionCount = competitionCount,
                };

                return res;
            }).ToList();

        }





        private string GetPartnerNameForCompetitionUser(int id, TennisGameCycle gc)
        {
            if (gc.FirstPlayer?.UserId == id) return gc.FirstPairPlayer?.User?.FullName;

            else if (gc.FirstPairPlayer?.UserId == id) return gc.FirstPlayer?.User?.FullName;

            else if (gc.SecondPlayer?.UserId == id) return gc.SecondPairPlayer?.User?.FullName;

            else if (gc.SecondPairPlayer?.UserId == id) return gc.SecondPlayer?.User?.FullName;

            else return string.Empty;
        }


        private void GetNamesOfPlayersForTennisLeague(int userId, TennisLeagueGame set, out string opponentName, out string partnerName)
        {
            partnerName = string.Empty;
            opponentName = string.Empty;
            if (set.HomePlayerId == userId)
            {
                opponentName = set.HomePairPlayerId.HasValue
                    ? $"{set.GuestPlayer?.FullName} / {set.GuestPairPlayer?.FullName}"
                    : set.GuestPlayer?.FullName;
                partnerName = $"{set.HomePairPlayer?.FullName}";
            }
            else if (set.GuestPlayerId == userId)
            {
                opponentName = set.GuestPairPlayerId.HasValue
                    ? $"{set.HomePlayer?.FullName} / {set.HomePairPlayer?.FullName}"
                    : set.HomePlayer?.FullName;
                partnerName = $"{set.GuestPairPlayer?.FullName}";
            }
            else if (set.HomePairPlayerId == userId)
            {
                opponentName = $"{set.GuestPlayer?.FullName} / {set.GuestPairPlayer?.FullName}";
                partnerName = $"{set.HomePlayer?.FullName}";
            }
            else if (set.GuestPairPlayerId == userId)
            {
                opponentName = $"{set.HomePlayer?.FullName} / {set.HomePairPlayer?.FullName}";
                partnerName = $"{set.GuestPlayer?.FullName}";
            }
        }

        private void GetSetResultForCompetition(int userId, TennisGameCycle competitionGame, bool isHebrew, out string setScore, out ResultType resultType)
        {
            var homeSetsWon = 0;
            var guestSetsWon = 0;
            setScore = string.Empty;
            resultType = ResultType.None;
            var scores = competitionGame.TennisGameSets.ToList();

            for (var i = 0; i < scores.Count; i++)
            {
                if (scores[i].FirstPlayerScore > scores[i].SecondPlayerScore)
                {
                    homeSetsWon += 1;
                    setScore += isHebrew
                        ? $"{scores[i].SecondPlayerScore} - {scores[i].FirstPlayerScore}"
                        : $"{scores[i].FirstPlayerScore} - {scores[i].SecondPlayerScore}";
                }
                else if (scores[i].SecondPlayerScore > scores[i].FirstPlayerScore)
                {
                    guestSetsWon += 1;
                    setScore += isHebrew
                        ? $"{scores[i].SecondPlayerScore} - {scores[i].FirstPlayerScore}"
                        : $"{scores[i].FirstPlayerScore} - {scores[i].SecondPlayerScore}";
                }
                else
                {
                    if (scores[i].FirstPlayerScore != 0 && scores[i].SecondPlayerScore != 0)
                    {
                        setScore += isHebrew
                            ? $"{scores[i].SecondPlayerScore} - {scores[i].FirstPlayerScore}"
                            : $"{scores[i].FirstPlayerScore} - {scores[i].SecondPlayerScore}";
                    }
                }


                if (scores.ElementAtOrDefault(i + 1) != null && !(scores[i + 1].FirstPlayerScore == 0 && scores[i + 1].SecondPlayerScore == 0))
                {
                    setScore += ", ";
                }
            }

            var isHomePlayer = competitionGame.FirstPlayer?.UserId == userId || competitionGame?.FirstPairPlayer?.UserId == userId;

            if (isHomePlayer && homeSetsWon > guestSetsWon)
            {
                resultType = ResultType.Win;
            }
            else if (isHomePlayer && homeSetsWon < guestSetsWon)
            {
                resultType = ResultType.Lose;
            }
            else if (!isHomePlayer && guestSetsWon > homeSetsWon)
            {
                resultType = ResultType.Win;
            }
            else if (!isHomePlayer && guestSetsWon < homeSetsWon)
            {
                resultType = ResultType.Lose;
            }
        }

        private string GetCompetitionName(int stageId)
        {
            return db.Leagues.Where(t => t.Stages.Any(s => s.StageId == stageId))?.FirstOrDefault()?.Name ?? string.Empty;
        }

        private void GetSetResultForLeague(int userId, TennisLeagueGame set, bool isHebrew, out string setScore, out ResultType resultType)
        {
            var homeSetsWon = 0;
            var guestSetsWon = 0;
            setScore = string.Empty;
            resultType = ResultType.Draw;
            var scores = set.TennisLeagueGameScores.ToList();

            for (var i = 0; i < scores.Count; i++)
            {
                if (scores[i].HomeScore > scores[i].GuestScore)
                {
                    homeSetsWon += 1;
                    setScore += isHebrew
                        ? $"{scores[i].GuestScore}-{scores[i].HomeScore}"
                        : $"{scores[i].HomeScore}-{scores[i].GuestScore}";
                }
                else if (scores[i].GuestScore > scores[i].HomeScore)
                {
                    guestSetsWon += 1;
                    setScore += isHebrew
                        ? $"{scores[i].GuestScore}-{scores[i].HomeScore}"
                        : $"{scores[i].HomeScore}-{scores[i].GuestScore}";
                }
                else
                {
                    if (scores[i].HomeScore != 0 && scores[i].GuestScore != 0)
                    {
                        setScore += isHebrew
                            ? $"{scores[i].GuestScore}-{scores[i].HomeScore}"
                            : $"{scores[i].HomeScore}-{scores[i].GuestScore}";
                    }
                }

                if (scores.ElementAtOrDefault(i + 1) != null && !(scores[i + 1].HomeScore == 0 && scores[i + 1].GuestScore == 0))
                {
                    setScore += ", ";
                }
            }

            var isHomePlayer = set.HomePlayerId == userId || set.HomePairPlayerId == userId;

            if (isHomePlayer && homeSetsWon > guestSetsWon)
            {
                resultType = ResultType.Win;
            }
            else if (isHomePlayer && homeSetsWon < guestSetsWon)
            {
                resultType = ResultType.Lose;
            }
            else if (!isHomePlayer && guestSetsWon > homeSetsWon)
            {
                resultType = ResultType.Win;
            }
            else if (!isHomePlayer && guestSetsWon < homeSetsWon)
            {
                resultType = ResultType.Lose;
            }
        }


        private int GetAthleteRankInCompetitionDiscipline(int disciplineCompetitionId, int regId)
        {
            var disciplineCompetition = db.CompetitionDisciplines.FirstOrDefault(d => d.Id == disciplineCompetitionId);
            var discipline = db.Disciplines.Find(disciplineCompetition.DisciplineId.Value);
            List<CompetitionDisciplineRegistration> result = new List<CompetitionDisciplineRegistration>();
            if (disciplineCompetition.IsResultsManualyRanked)
            {
                var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().Rank ?? int.MaxValue).ToList();
                result = resulted;
            }
            else if (discipline.Format != null && ((discipline.Format.Value >= 6 && discipline.Format.Value <= 8) || discipline.Format.Value == 10 || discipline.Format.Value == 11))
            {
                var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().SortValue);
                if (discipline.Format.Value == 7 || discipline.Format.Value == 10 || discipline.Format.Value == 11)
                {
                    resulted = resulted.ThenByDescending(r => r.GetThrowingsOrderPower());
                }
                if (discipline.Format.Value == 6)
                {
                    resulted = resulted.ThenByDescending(r => r.CompetitionResult.FirstOrDefault()?.LiveSplitOrder ?? int.MinValue);
                }
                var res = resulted.Union(disciplineCompetition.CompetitionDisciplineRegistrations).ToList();
                result = res;
            }
            else
            {
                var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().SortValue);
                var res = resulted.Union(disciplineCompetition.CompetitionDisciplineRegistrations).ToList();
                result = res;
            }
            result = result.Where(x => x.CompetitionResult.Count() > 0 && !string.IsNullOrWhiteSpace(x.CompetitionResult.FirstOrDefault().Result)).ToList();
            int rank = result.FindIndex(r => r.Id == regId);
            return rank;
        }


        private List<int> GetAthletesRanksInCompetitionDiscipline(int disciplineCompetitionId)
        {
            var disciplineCompetition = db.CompetitionDisciplines.FirstOrDefault(d => d.Id == disciplineCompetitionId);
            var discipline = db.Disciplines.Find(disciplineCompetition.DisciplineId.Value);
            List<CompetitionDisciplineRegistration> result = new List<CompetitionDisciplineRegistration>();
            if (disciplineCompetition.IsResultsManualyRanked)
            {
                var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().Rank ?? int.MaxValue).ToList();
                result = resulted;
            }
            else if (discipline.Format != null && ((discipline.Format.Value >= 6 && discipline.Format.Value <= 8) || discipline.Format.Value == 10 || discipline.Format.Value == 11))
            {
                var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderByDescending(x => x.CompetitionResult.FirstOrDefault().SortValue);
                if (discipline.Format.Value == 7 || discipline.Format.Value == 10 || discipline.Format.Value == 11)
                {
                    resulted = resulted.ThenByDescending(r => r.GetThrowingsOrderPower());
                }
                if (discipline.Format.Value == 6)
                {
                    resulted = resulted.ThenByDescending(r => r.CompetitionResult.FirstOrDefault()?.LiveSplitOrder ?? int.MinValue);
                }
                var res = resulted.Union(disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.FirstOrDefault() != null && x.CompetitionResult.FirstOrDefault().AlternativeResult != 3)).ToList(); // added in ends alternative results except DNS
                result = res;
            }
            else
            {
                var resulted = disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.Count() > 0 && x.CompetitionResult.FirstOrDefault().AlternativeResult == 0).OrderBy(x => x.CompetitionResult.FirstOrDefault().SortValue);
                var res = resulted.Union(disciplineCompetition.CompetitionDisciplineRegistrations.Where(x => x.CompetitionResult.FirstOrDefault() != null && x.CompetitionResult.FirstOrDefault().AlternativeResult != 3)).ToList();
                result = res;
            }
            result = result.Where(x => x.CompetitionResult.Count() > 0 && !string.IsNullOrWhiteSpace(x.CompetitionResult.FirstOrDefault().Result)).ToList();
            var ranks = result.Select(r => r.UserId).ToList();
            return ranks;
        }

        public List<AthleteCompetitionAchievementsBySeason> GetAthleteCompetitionsAchievements(int id)
        {
            var records = db.DisciplineRecords.ToList();
            var registrations = db.CompetitionDisciplineRegistrations.AsNoTracking().Where(r => r.UserId == id && !r.IsArchive && !r.CompetitionDiscipline.IsDeleted && !r.CompetitionDiscipline.League.IsArchive && r.CompetitionResult.FirstOrDefault() != null && r.CompetitionResult.FirstOrDefault().Result.Length > 0).OrderBy(r => r.CompetitionDiscipline.League.LeagueStartDate ?? DateTime.MaxValue).ToList().Select(r => new AthleteCompetitionAchievementViewItem
            {
                UserId = r.UserId,
                CompetitionName = r.CompetitionDiscipline.League.Name,
                CompetitionDisciplineId = r.CompetitionDisciplineId,
                RegistrationId = r.Id,
                DisciplineType = db.Disciplines.FirstOrDefault(d => d.DisciplineId == r.CompetitionDiscipline.DisciplineId).DisciplineType,
                Format = db.Disciplines.FirstOrDefault(d => d.DisciplineId == r.CompetitionDiscipline.DisciplineId).Format,
                DisciplineName = db.Disciplines.FirstOrDefault(d => d.DisciplineId == r.CompetitionDiscipline.DisciplineId).Name,
                CompetitionStartDate = r.CompetitionDiscipline.League.LeagueStartDate,
                Heat = r.CompetitionResult.FirstOrDefault().Heat,
                Lane = r.CompetitionResult.FirstOrDefault().Lane,
                SortValue = r.CompetitionResult.FirstOrDefault().SortValue,
                Wind = r.CompetitionResult.FirstOrDefault().Wind,
                Result = r.CompetitionResult.FirstOrDefault().Result,
                AlternativeResult = r.CompetitionResult.FirstOrDefault().AlternativeResult,
                RecordId = GetRecordIdByDisciplineId(records, r.CompetitionDiscipline.DisciplineId),
                Points = r.CompetitionDiscipline.IsMultiBattle ? r.CompetitionResult.FirstOrDefault().CombinedPoint : (int?)r.CompetitionResult.FirstOrDefault().ClubPoints,
                SeasonId = r.CompetitionDiscipline.League.SeasonId ?? 0
            }).ToList();
            List<AthleteCompetitionAchievementsBySeason> registrationBySeasons = new List<AthleteCompetitionAchievementsBySeason>();
            foreach (AthleteCompetitionAchievementViewItem registration in registrations)
            {
                var registrationBySeason =
                    registrationBySeasons.FirstOrDefault(x => x.SeasonId == registration.SeasonId);
                if (registrationBySeason == null)
                {
                    registrationBySeason = new AthleteCompetitionAchievementsBySeason
                    {
                        SeasonId = registration.SeasonId,
                        SeasonName = seasonsRepo.GetById(registration.SeasonId)?.Name,
                        Achievements = new List<AthleteCompetitionAchievementViewItem>()
                    };
                    registrationBySeason.Achievements.Add(registration);
                    registrationBySeasons.Add(registrationBySeason);
                }
                else
                {
                    registrationBySeason.Achievements.Add(registration);
                }
            }

            foreach (var registration in registrations)
            {
                if (registration.AlternativeResult == 0)
                {
                    registration.Rank = GetAthleteRankInCompetitionDiscipline(registration.CompetitionDisciplineId, registration.RegistrationId) + 1;
                }
            }

            return registrationBySeasons.OrderByDescending(x => x.SeasonId).ToList();
        }

        private int? GetRecordIdByDisciplineIdAndCategoryId(List<DisciplineRecord> records, int? disciplineId, int categoryId)
        {
            if (disciplineId.HasValue)
            {
                foreach (var record in records)
                {
                    if (record.isCategorySelected(categoryId) && record.isDisciplineSelected(disciplineId.Value))
                    {
                        return record.Id;
                    }
                }
            }
            return null;
        }

        private List<int> GetRecordIdByDisciplineId(List<DisciplineRecord> records, int? disciplineId)
        {
            var recordIds = new List<int>();
            if (disciplineId.HasValue)
            {
                foreach (var record in records)
                {
                    if (record.isDisciplineSelected(disciplineId.Value))
                    {
                        recordIds.Add(record.Id);
                    }
                }
            }
            return recordIds;
        }


        public List<IGrouping<int, AthleteCompetitionAchievementViewItem>> GetBulkAthleteCompetitionsAchievements(List<int> ids, int seasonId)
        {
            var records = db.DisciplineRecords.ToList();
            var registrations = db.CompetitionDisciplineRegistrations.Include("CompetitionDiscipline").Include("CompetitionResult").AsNoTracking().Where(r => ids.Contains(r.UserId) && r.CompetitionDiscipline.League.SeasonId == seasonId && !r.IsArchive && !r.CompetitionDiscipline.IsDeleted && !r.CompetitionDiscipline.League.IsArchive && r.CompetitionResult.FirstOrDefault() != null && r.CompetitionResult.FirstOrDefault().Result.Length > 0 && (!r.CompetitionResult.FirstOrDefault().Wind.HasValue || r.CompetitionResult.FirstOrDefault().Wind.Value <= 2.00))
            .OrderBy(r => r.CompetitionDiscipline.League.LeagueStartDate ?? DateTime.MaxValue).ToList().Select(r =>
            {
                var discipline = db.Disciplines.FirstOrDefault(d => d.DisciplineId == r.CompetitionDiscipline.DisciplineId);
                return new AthleteCompetitionAchievementViewItem
                {
                    UserId = r.UserId,
                    CompetitionDisciplineId = r.CompetitionDisciplineId,
                    RegistrationId = r.Id,
                    Format = discipline.Format,
                    DisciplineName = discipline.Name,
                    DisciplineType = discipline.DisciplineType,
                    Heat = r.CompetitionResult.FirstOrDefault().Heat,
                    Lane = r.CompetitionResult.FirstOrDefault().Lane,
                    SortValue = r.CompetitionResult.FirstOrDefault().SortValue,
                    Wind = r.CompetitionResult.FirstOrDefault().Wind,
                    Result = r.CompetitionResult.FirstOrDefault().Result,
                    CompetitionStartDate = r.CompetitionDiscipline.League.LeagueStartDate,
                    AlternativeResult = r.CompetitionResult.FirstOrDefault().AlternativeResult,
                    RecordId = GetRecordIdByDisciplineId(records, r.CompetitionDiscipline.DisciplineId)
                };
            }).ToList();

            registrations = registrations.Where(r => !(!r.Wind.HasValue && disciplineTypesThatUsesWinds.Contains(r.DisciplineType))).ToList();

            var removedRegistrations = registrations.Where(r => !r.Wind.HasValue && disciplineTypesThatUsesWinds.Contains(r.DisciplineType)).ToList();


            var groupedRegistrations = registrations.GroupBy(r => r.UserId).ToList();
            return groupedRegistrations;
        }




        public IQueryable<GamesCycle> GetGamesQuery(Expression<Func<GamesCycle, bool>> cond = null)
        {
            if (cond == null)
            {
                cond = PredicateBuilder.New<GamesCycle>(true);
            }
            return db.GamesCycles.Where(cond)
                .Include(t => t.HomeTeam)
                .Include(t => t.TeamsPlayer)
                .Include(t => t.TeamsPlayer1)
                .Include(t => t.GuestTeam)
                .Include(t => t.Auditorium)
                .Include(t => t.Stage)
                .Include(t => t.Stage.League)
                .Include(t => t.GameSets)
                .Include(t => t.NotesGames)
                .Include(t => t.WallThreads)
                .Include(t => t.Users)
                .Include(t => t.TennisLeagueGames)
                .Include(t => t.Group)
                .Include(t => t.Group.Stage.League)
                .Include(t => t.Group.Stage.League.Games)
                .Include(t => t.HomeTeam.TeamsDetails)
                .Include(t => t.GuestTeam.TeamsDetails)
                .OrderBy(t => DbFunctions.TruncateTime(t.StartDate))
                .ThenBy(t => t.Auditorium.Name)
                .ThenBy(t => t.StartDate);
        }

        public IQueryable<TennisGameCycle> GetTennisGamesQuery(Expression<Func<TennisGameCycle, bool>> cond = null)
        {
            if (cond == null)
            {
                cond = PredicateBuilder.New<TennisGameCycle>(true);
            }
            return db.TennisGameCycles.Where(cond)
                //.Include(t => t.TeamsPlayer)
                //.Include(t => t.TeamsPlayer1)
                //.Include(t => t.TeamsPlayer11)
                //.Include(t => t.TeamsPlayer3)               
                //.Include(t => t.TeamsPlayer.User)
                //.Include(t => t.TeamsPlayer1.User)

                .Include(t => t.Auditorium)
                //.Include(t => t.TennisGroup)
                //.Include(t => t.TennisPlayoffBracket)
                //.Include(t => t.TennisGameSets)
                //.Include(t => t.TennisGroup.TennisStage.TennisGames)
                //.Include(t => t.TennisStage)
                //.Include(t => t.TennisStage.Team)
                .OrderBy(t => DbFunctions.TruncateTime(t.StartDate))
                .ThenBy(t => t.Auditorium.Name)
                .ThenBy(t => t.StartDate);
        }

        public IEnumerable<ExcelRefereeDto> GetRefereesExcel(int unionId)
        {
            var seasonId = seasonsRepo.GetCurrentByUnionId(unionId);
            var gamesCycles = seasonId.HasValue
                ? db.GamesCycles.Where(t => t.Stage.League.UnionId == unionId && t.Stage.League.SeasonId == seasonId).ToList()
                : db.GamesCycles.Where(t => t.Stage.League.UnionId == unionId).ToList();
            var excelRefereeDtos = new List<ExcelRefereeDto>();
            foreach (var t in gamesCycles)
            {
                excelRefereeDtos.Add(new ExcelRefereeDto
                {
                    League = t.Stage.League.Name,
                    StartDate = t.StartDate,
                    HomeTeam = t.HomeTeam?.Title ?? "",
                    GuestTeam = t.GuestTeam?.Title,
                    Auditorium = t.Auditorium?.Name ?? "",
                    AuditoriumAddress = t.Auditorium?.Address ?? "",
                    Referees = GetRefereesNames(t.RefereeIds),
                });
            }

            return excelRefereeDtos.OrderBy(t => t.Referees == null)
                .ThenBy(t => t.Referees)
                .ThenBy(t => t.StartDate).ToList();
        }

        private string GetRefereesNames(string refereeIds)
        {
            var names = refereeIds != null ? usersRepo.GetUserNamesStringByIds(refereeIds.Split(',')) : "";
            return names;
        }

        public int ToggleTeams(int cycleId)
        {
            var c = db.GamesCycles.Find(cycleId);
            //Toggle teams
            int? tempId = c.GuestTeamId;
            c.GuestTeamId = c.HomeTeamId;
            c.HomeTeamId = tempId;
            //Toggle Scores
            int tempScore = c.GuestTeamScore;
            c.GuestTeamScore = c.HomeTeamScore;
            c.HomeTeamScore = tempScore;

            //Toogle teams in PlayoffBracket
            if (c.PlayoffBracket != null)
            {
                var playOffTemp = c.PlayoffBracket.Team2Id;
                c.PlayoffBracket.Team2Id = c.PlayoffBracket.Team1Id;
                c.PlayoffBracket.Team1Id = playOffTemp;
            }

            //Toogle Parents
            if (c.PlayoffBracket != null && c.PlayoffBracket.Parent1 != null)
            {
                var tempParent1 = c.PlayoffBracket.Parent1.Team1Id;
                c.PlayoffBracket.Parent1.Team1Id = c.PlayoffBracket.Parent1.Team2Id;
                c.PlayoffBracket.Parent1.Team2Id = tempParent1;
            }

            if (c.PlayoffBracket != null && c.PlayoffBracket.Parent2 != null)
            {
                var tempParent1 = c.PlayoffBracket.Parent2.Team1Id;
                c.PlayoffBracket.Parent2.Team1Id = c.PlayoffBracket.Parent2.Team2Id;
                c.PlayoffBracket.Parent2.Team2Id = tempParent1;
            }

            foreach (var gs in c.GameSets)
            {
                int tempS = gs.GuestTeamScore;
                gs.GuestTeamScore = gs.HomeTeamScore;
                gs.HomeTeamScore = tempS;
            }

            c.AuditoriumId = tRepo.GetMainOrFirstAuditoriumForTeam(c.HomeTeamId);

            db.SaveChanges();

            return c.Stage.LeagueId;
        }

        public int ToogleAthletes(int cycleId)
        {
            var c = db.GamesCycles.Find(cycleId);
            //Toggle athletes
            int? tempId = c.GuestAthleteId;
            c.GuestAthleteId = c.HomeAthleteId;
            c.HomeAthleteId = tempId;
            //Toggle Scores
            int tempScore = c.GuestTeamScore;
            c.GuestTeamScore = c.HomeTeamScore;
            c.HomeTeamScore = tempScore;

            foreach (var gs in c.GameSets)
            {
                int tempS = gs.GuestTeamScore;
                gs.GuestTeamScore = gs.HomeTeamScore;
                gs.HomeTeamScore = tempS;
            }

            if (c.AuditoriumId.HasValue)
            {
                c.AuditoriumId = tRepo.GetMainOrFirstAuditoriumForTeam(c.HomeTeamId);
            }
            db.SaveChanges();

            return c.Stage.LeagueId;
        }

        public void Update(GamesCycle gc)
        {
            db.Entry(gc).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void UpdateTennis(TennisGameCycle gc)
        {
            db.Entry(gc).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void Update(IEnumerable<GamesCycle> gamesCycles)
        {
            foreach (var gameCycle in gamesCycles)
            {
                db.Entry(gameCycle).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        public void UpdateTennis(IEnumerable<TennisGameCycle> gamesCycles)
        {
            foreach (var gameCycle in gamesCycles)
            {
                db.Entry(gameCycle).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        public void RemoveGameCycleReportFile(int cycleId)
        {
            GamesCycle gc = db.GamesCycles.Find(cycleId);
            gc.ReportFileName = null;
            db.SaveChanges();
        }

        public void SaveGameCycleReportFile(int cycleId, string fileName)
        {
            GamesCycle gc = db.GamesCycles.Find(cycleId);
            gc.ReportFileName = fileName;
            db.SaveChanges();
        }

        public GamesCycle GetGameCycleById(int cycleId)
        {
            return db.GamesCycles.Find(cycleId);
        }

        public TennisGameCycle GetTennisGameCycleById(int cycleId)
        {
            return db.TennisGameCycles.Find(cycleId);
        }

        public GameSet GetGameSetById(int gameSetId)
        {
            return db.GameSets.FirstOrDefault(t => t.GameSetId == gameSetId);
        }

        public IEnumerable<GameSet> GetGameSets(int cycleId)
        {
            return db.GameSets.Where(t => t.GameCycleId == cycleId).ToList();
        }

        public IEnumerable<GameSet> GetGameSetsByGameId(int gameId)
        {
            return db.GameSets.Where(t => t.GameCycleId == gameId).ToList();
        }

        public void RemoveCycle(GamesCycle item)
        {
            string gameType = item.Group.GamesType.Name;

            //  Prohibit deleting Knockout and Playoff games on server level 
            if (gameType == GameType.Knockout || gameType == GameType.Playoff)
            {
                throw new Exception($"It is prohibited to delete games of \"{GameType.Knockout}\" and \"{GameType.Playoff}\" types. Only the whole group can be deleted.");
            }

            db.GameSets.RemoveRange(item.GameSets);
            foreach (var user in item.Users)
            {
                user.UsersGamesCycles = null;
            }
            db.GamesCycles.Remove(item);
            item.Users = null;
        }

        public void RemoveTennisCycle(TennisGameCycle item)
        {
            int gameType = item.TennisGroup.TypeId;

            //  Prohibit deleting Knockout and Playoff games on server level 
            /*if (gameType == 2 || gameType == 3)
            {
                throw new Exception($"It is prohibited to delete games of \"{GameType.Knockout}\" and \"{GameType.Playoff}\" types. Only the whole group can be deleted.");
            }*/

            db.TennisGameSets.RemoveRange(item.TennisGameSets);
            db.TennisGameCycles.Remove(item);
        }

        public void AddGameSet(GameSet set)
        {
            GamesCycle gc = GetGameCycleById(set.GameCycleId);
            var lastSetNumebr = gc.GameSets.OrderBy(c => c.SetNumber).Select(c => c.SetNumber).LastOrDefault();
            set.SetNumber = ++lastSetNumebr;
            db.GameSets.Add(set);
            db.SaveChanges();
        }

        public void DeleteSet(GameSet set)
        {
            db.GameSets.Remove(set);
            db.SaveChanges();
        }

        public GamesCycle UpdateGameScore(int id)
        {
            GamesCycle gc = GetGameCycleById(id);
            return UpdateGameScore(gc);
        }

        public GamesCycle UpdateGameScore(GamesCycle gc)
        {
            int hScore = 0;
            int gScore = 0;
            var gameAlias = GetSectionAlias(gc);
            var isPenaltySection = gameAlias.Equals(GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase)
                || gameAlias.Equals(GamesAlias.Soccer, StringComparison.OrdinalIgnoreCase)
                || gameAlias.Equals(GamesAlias.Rugby, StringComparison.OrdinalIgnoreCase)
                || gameAlias.Equals(GamesAlias.Handball, StringComparison.OrdinalIgnoreCase);
            if (isPenaltySection)
            {
                foreach (var set in gc.GameSets.Where(c => !c.IsPenalties))
                {
                    if (set.HomeTeamScore > set.GuestTeamScore)
                        hScore++;
                    else if (set.HomeTeamScore < set.GuestTeamScore)
                        gScore++;
                }
                if (hScore == gScore)
                {
                    var penalty = gc.GameSets.FirstOrDefault(c => c.IsPenalties);
                    if (penalty != null)
                    {
                        if (penalty.HomeTeamScore > penalty.GuestTeamScore)
                            hScore++;
                        else if (penalty.HomeTeamScore < penalty.GuestTeamScore)
                            gScore++;
                    }
                }
            }
            else
            {
                foreach (var set in gc.GameSets)
                {
                    if (set.HomeTeamScore > set.GuestTeamScore)
                        hScore++;
                    else if (set.HomeTeamScore < set.GuestTeamScore)
                        gScore++;
                }
            }


            gc.HomeTeamScore = hScore;
            gc.GuestTeamScore = gScore;
            Update(gc);
            return gc;
        }

        public GamesCycle UpdateBasketBallWaterPoloScore(int id)
        {
            var gc = GetGameCycleById(id);
            return UpdateBasketBallWaterPoloScore(gc);
        }

        public void UpdateWatPoloScore(int id)
        {
            var gc = GetGameCycleById(id);
            UpdateWaterPoloScore(gc);
        }

        public GamesCycle UpdateGameScoreEndGameLsApp(GamesCycle gc)
        {
            var currentGameStatistics = db.Statistics.Where(x => x.GameId == gc.CycleId);

            var guestStatistics = currentGameStatistics.Where(x => x.TeamId == gc.GuestTeamId);
            var homeStatistics = currentGameStatistics.Where(x => x.TeamId == gc.HomeTeamId);

            var guestPoints = CalculateTeamScores(guestStatistics);
            var homePoints = CalculateTeamScores(homeStatistics);

            gc.HomeTeamScore = homePoints;
            gc.GuestTeamScore = guestPoints;

            Update(gc);
            return gc;
        }

        public GamesCycle UpdateBasketBallWaterPoloScore(GamesCycle gc)
        {
            var lastQuarter = gc.GameSets.OrderByDescending(x => x.GameSetId).FirstOrDefault();
            if (lastQuarter != null)
            {
                if (lastQuarter.HomeTeamScore > lastQuarter.GuestTeamScore && !lastQuarter.IsPenalties)
                {
                    gc.HomeTeamScore = 1;
                    gc.GuestTeamScore = 0;
                }
                else if (lastQuarter.HomeTeamScore < lastQuarter.GuestTeamScore && !lastQuarter.IsPenalties)
                {
                    gc.GuestTeamScore = 1;
                    gc.HomeTeamScore = 0;
                }
            }
            else
            {
                gc.HomeTeamScore = 0;
                gc.GuestTeamScore = 0;
            }

            Update(gc);
            return gc;
        }

        private void UpdateWaterPoloScore(GamesCycle gc)
        {
            if (gc.GameSets.Count > 1)
            {
                var lastSet = gc.GameSets.OrderBy(c => c.SetNumber).LastOrDefault();
                if (lastSet != null)
                {
                    DeleteSet(lastSet);
                }
            }


            int hScore = 0;
            int gScore = 0;
            foreach (var set in gc.GameSets)
            {
                if (set.HomeTeamScore > set.GuestTeamScore)
                {
                    hScore++;
                    set.GuestTeamScore = 0;
                    set.HomeTeamScore = 5;
                }
                else if (set.HomeTeamScore < set.GuestTeamScore)
                {
                    gScore++;
                    set.GuestTeamScore = 5;
                    set.HomeTeamScore = 0;
                }
            }
            gc.HomeTeamScore = hScore;
            gc.GuestTeamScore = gScore;

            Update(gc);
        }

        public GamesCycle StartGame(int id)
        {
            GamesCycle gc = GetGameCycleById(id);
            gc.GameStatus = GameStatus.Started;
            Update(gc);
            return gc;
        }

        public GamesCycle EndGame(int id)
        {
            GamesCycle gc = GetGameCycleById(id);
            return EndGame(gc);
        }

        private void checkAllLeagueGamesExcludedDone(PenaltyForExclusion penaltyForExclusion)
        {
            var prevPenalisedGames = db.GamesCycles.Where(g =>
                (g.GuestTeam.TeamsPlayers.Select(c => c.UserId).Contains(penaltyForExclusion.UserId) ||
                 g.HomeTeam.TeamsPlayers.Select(c => c.UserId).Contains(penaltyForExclusion.UserId)) &&
                 g.AppliedExclusionId.HasValue && g.AppliedExclusionId.Value == penaltyForExclusion.Id && g.GameStatus == GameStatus.Ended).ToList();
            if(prevPenalisedGames.Count() >= penaltyForExclusion.ExclusionNumber)
            {
                penaltyForExclusion.IsEnded = true;
            }
        }
        public GamesCycle EndGame(GamesCycle gc)
        {
            gc.GameStatus = GameStatus.Ended;
            UpdateGameScore(gc);

            if(gc.AppliedExclusionId.HasValue)
            {
                checkAllLeagueGamesExcludedDone(gc.PenaltyForExclusion);
            }
            bracketsRepo.GameEndedEvent(gc);

            if (gc.Stage?.GamesCycles?.Any() == true &&
                gc.Stage?.GamesCycles?.All(x => x.GameStatus == GameStatus.Ended) == true &&
                gc.Stage?.CreateCrossesStage == true &&
                gc.Stage?.ChildStages?.Any() == true)
            {
                FillTeamsInCrossesStage(gc.Stage);
            }
            return gc;
        }

        private void FillTeamsInCrossesStage(Stage parentStage)
        {
            if (parentStage?.GamesCycles?.Any() == true &&
                parentStage.GamesCycles.All(x => x.GameStatus == GameStatus.Ended) &&
                parentStage.CreateCrossesStage &&
                parentStage.ChildStages.Any() &&
                parentStage.Groups.Count == 2)
            {
                var crossesStage = parentStage.ChildStages?.FirstOrDefault(x => x.IsCrossesStage && !x.IsArchive);
                if (crossesStage != null)
                {
                    var rankService = new LeagueRankService(parentStage.LeagueId);
                    var rankTable = rankService.CreateLeagueRankTable(parentStage.League.SeasonId);

                    var parentStageRanks = rankTable.Stages.FirstOrDefault(x => x.StageId == parentStage.StageId);

                    var crossesGroup = crossesStage.Groups.FirstOrDefault(x => !x.IsArchive);

                    if (parentStageRanks != null && crossesGroup != null)
                    {
                        db.GroupsTeams.RemoveRange(crossesGroup.GroupsTeams);

                        var teamIndex = 0;
                        var groupTeamPos = 1;

                        while (true)
                        {
                            var firstGroupTeam = parentStageRanks.Groups.ElementAtOrDefault(0)
                                ?.Teams
                                .OrderByDescending(x => x.Points)
                                .ElementAtOrDefault(teamIndex);

                            var secondGroupTeam = parentStageRanks.Groups.ElementAtOrDefault(1)
                                ?.Teams
                                .OrderByDescending(x => x.Points)
                                .ElementAtOrDefault(teamIndex);

                            var gameCycle = crossesStage.GamesCycles.ElementAtOrDefault(teamIndex);

                            if (firstGroupTeam != null && secondGroupTeam != null)
                            {
                                crossesGroup.GroupsTeams.Add(new GroupsTeam
                                {
                                    TeamId = firstGroupTeam.Id,
                                    Pos = groupTeamPos++
                                });
                                crossesGroup.GroupsTeams.Add(new GroupsTeam
                                {
                                    TeamId = secondGroupTeam.Id,
                                    Pos = groupTeamPos++
                                });

                                if (gameCycle != null)
                                {
                                    gameCycle.HomeTeamId = firstGroupTeam.Id;
                                    gameCycle.GuestTeamId = secondGroupTeam.Id;
                                }
                                else
                                {
                                    crossesStage.GamesCycles.Add(new GamesCycle
                                    {
                                        StartDate = DateTime.Now,
                                        HomeTeamId = firstGroupTeam.Id,
                                        GuestTeamId = secondGroupTeam.Id,
                                        Group = crossesGroup
                                    });
                                }
                            }
                            else
                            {
                                break;
                            }

                            teamIndex++;
                        }

                        db.SaveChanges();
                    }
                }
            }
        }

        public void TechnicalWin(int gameCycleId, int teamId)
        {
            GamesCycle gc = GetGameCycleById(gameCycleId);
            db.GameSets.RemoveRange(gc.GameSets);
            db.SaveChanges();
            for (int i = 0; i < 2; i++)
            {
                GameSet set = null;
                if (teamId == gc.HomeTeamId)
                {
                    set = new GameSet
                    {
                        HomeTeamScore = 25,
                        GuestTeamScore = 0,
                        GameCycleId = gameCycleId
                    };
                }
                else
                {
                    set = new GameSet
                    {
                        HomeTeamScore = 0,
                        GuestTeamScore = 25,
                        GameCycleId = gameCycleId
                    };
                }
                AddGameSet(set);
            }
            gc.TechnicalWinnnerId = teamId;
            EndGame(gc);
        }

        public void TechnicalWinForAthlete(int gameCycleId, int athleteId)
        {
            GamesCycle gc = GetGameCycleById(gameCycleId);
            db.GameSets.RemoveRange(gc.GameSets);
            db.SaveChanges();
            for (int i = 0; i < 2; i++)
            {
                GameSet set = null;
                if (athleteId == gc.HomeAthleteId)
                {
                    set = new GameSet
                    {
                        HomeTeamScore = 25,
                        GuestTeamScore = 0,
                        GameCycleId = gameCycleId
                    };
                }
                else
                {
                    set = new GameSet
                    {
                        HomeTeamScore = 0,
                        GuestTeamScore = 25,
                        GameCycleId = gameCycleId
                    };
                }
                AddGameSet(set);
            }
            gc.TechnicalWinnnerId = athleteId;
            EndGame(gc);
        }

        /** add cheng for comment of EditGamePartial */
        public void GameCycleComments(int gameCycleId, String comments)
        {
            GamesCycle gc = GetGameCycleById(gameCycleId);
            gc.Note = comments;
            db.SaveChanges();
        }
        public void WaterPoloTechnicalWin(int gameCycleId, int teamId)
        {
            GamesCycle gc = GetGameCycleById(gameCycleId);
            db.GameSets.RemoveRange(gc.GameSets);
            db.SaveChanges();

            GameSet set = null;
            if (teamId == gc.HomeTeamId)
            {
                set = new GameSet
                {
                    HomeTeamScore = 5,
                    GuestTeamScore = 0,
                    GameCycleId = gameCycleId
                };
            }
            else
            {
                set = new GameSet
                {
                    HomeTeamScore = 0,
                    GuestTeamScore = 5,
                    GameCycleId = gameCycleId
                };
            }
            AddGameSet(set);
            gc.TechnicalWinnnerId = teamId;
            EndGame(gc);
        }

        public void SoftballTechnicalWin(int gameCycleId, int teamId)
        {
            GamesCycle gc = GetGameCycleById(gameCycleId);
            db.GameSets.RemoveRange(gc.GameSets);
            db.SaveChanges();

            GameSet set = null;
            if (teamId == gc.HomeTeamId)
            {
                set = new GameSet
                {
                    HomeTeamScore = 7,
                    GuestTeamScore = 0,
                    GameCycleId = gameCycleId
                };
            }
            else
            {
                set = new GameSet
                {
                    HomeTeamScore = 0,
                    GuestTeamScore = 7,
                    GameCycleId = gameCycleId
                };
            }
            AddGameSet(set);
            gc.TechnicalWinnnerId = teamId;
            EndGame(gc);
        }

        public void BasketBallTechnicalWin(int gameCycleId, int teamId)
        {
            GamesCycle gc = GetGameCycleById(gameCycleId);
            db.GameSets.RemoveRange(gc.GameSets);
            db.SaveChanges();

            GameSet set = null;
            if (teamId == gc.HomeTeamId)
            {
                set = new GameSet
                {
                    HomeTeamScore = 20,
                    GuestTeamScore = 0,
                    GameCycleId = gameCycleId
                };
            }
            else
            {
                set = new GameSet
                {
                    HomeTeamScore = 0,
                    GuestTeamScore = 20,
                    GameCycleId = gameCycleId
                };
            }
            AddGameSet(set);
            gc.TechnicalWinnnerId = teamId;
            EndGame(gc);
        }

        public void UpdateGameSet(GameSet set)
        {
            db.GameSets.Attach(set);
            var entry = db.Entry(set);
            entry.State = EntityState.Modified;
            db.SaveChanges();
        }

        public void ResetGame(int id)
        {
            GamesCycle gc = GetGameCycleById(id);
            ResetGame(gc);
        }

        public void ResetGame(GamesCycle gc, bool includingTeams = false, bool? isIndividual = false)
        {
            gc.TechnicalWinnnerId = null;
            db.GameSets.RemoveRange(gc.GameSets);
            gc.HomeTeamScore = 0;
            gc.GuestTeamScore = 0;
            if (gc.PlayoffBracket != null)
            {
                if (isIndividual.HasValue && isIndividual == true)
                {
                    gc.PlayoffBracket.WinnerAthleteId = null;
                    gc.PlayoffBracket.LoserAthleteId = null;
                }
                else
                {
                    gc.PlayoffBracket.WinnerId = null;
                    gc.PlayoffBracket.LoserId = null;
                }
            }
            if (includingTeams)
            {
                if (isIndividual.HasValue && isIndividual == true)
                {
                    gc.HomeAthleteId = null;
                    gc.GuestAthleteId = null;
                }
                else
                {
                    gc.HomeTeamId = null;
                    gc.GuestTeamId = null;
                }
            }
            bracketsRepo.GameEndedEvent(gc);
            gc.GameStatus = null;
            db.SaveChanges();
        }

        internal void SaveGames(List<GamesCycle> gamesList)
        {
            db.GamesCycles.AddRange(gamesList.ToList());
            db.SaveChanges();
        }

        internal void SaveTennisGames(List<TennisGameCycle> gamesList)
        {
            db.TennisGameCycles.AddRange(gamesList.ToList());
            db.SaveChanges();
        }

        internal void ValidateTimePlaceContradictions(List<GamesCycle> gamesList, TimeSpan gamesInterval)
        {
            foreach (var gameC in gamesList)
            {
                DateTime nGameDate = gameC.StartDate;
                while (db.GamesCycles.Any(g =>
                           g.StartDate == nGameDate &&
                           g.AuditoriumId == gameC.AuditoriumId &&
                           g.AuditoriumId != null &&
                           g.AuditoriumId != 0)
                       ||
                       gamesList.Any(g =>
                           g.StartDate == nGameDate &&
                           g.AuditoriumId == gameC.AuditoriumId &&
                           g.AuditoriumId != null &&
                           g.AuditoriumId != 0 &&
                           g != gameC))
                {
                    nGameDate = nGameDate.Add(gamesInterval);
                }
                gameC.StartDate = nGameDate;
            }
        }

        internal void ValidateTennisTimePlaceContradictions(List<TennisGameCycle> gamesList, TimeSpan gamesInterval)
        {
            foreach (var gameC in gamesList)
            {
                DateTime nGameDate = gameC.StartDate;
                while (db.TennisGameCycles.Any(g =>
                           g.StartDate == nGameDate &&
                           g.FieldId == gameC.FieldId &&
                           g.FieldId != null &&
                           g.FieldId != 0)
                       ||
                       gamesList.Any(g =>
                           g.StartDate == nGameDate &&
                           g.FieldId == gameC.FieldId &&
                           g.FieldId != null &&
                           g.FieldId != 0 &&
                           g != gameC))
                {
                    nGameDate = nGameDate.Add(gamesInterval);
                }
                gameC.StartDate = nGameDate;
            }
        }

        internal void AddGame(GamesCycle item)
        {
            //  Prohibit adding by hand Playoff and Knockout games
            string gameType = item?.Group?.GamesType?.Name;
            if (string.IsNullOrEmpty(gameType))
            {
                gameType = db.Groups.Where(g => g.GroupId == item.GroupId).First().GamesType.Name;
            }
            if (gameType == GameType.Knockout || gameType == GameType.Playoff)
            {
                throw new Exception($"It is prohibited to delete games of \"{GameType.Knockout}\" and \"{GameType.Playoff}\" types. Only the whole group can be deleted.");
            }

            db.GamesCycles.Add(item);
            db.SaveChanges();
        }

        public IEnumerable<GamesCycle> GetGroupsCyclesByGameIds(int[] gameIds)
        {
            return _gameCycles
                .Where(t => gameIds.Contains(t.CycleId))
                .OrderBy(g => g.StartDate)
                .ThenBy(g => g.Group.Name)
                .ThenBy(g => g.CycleNum)
                .ToList();
        }

        public void UpdateGroupCyclesFromExcelImport(List<ExcelGameDto> cyclesForUpdate)
        {
            foreach (var gamesCycle in cyclesForUpdate)
            {
                var gc = db.GamesCycles.FirstOrDefault(g => g.CycleId == gamesCycle.GameId);
                if (gc != null)
                {
                    UpdateIfDifferentObject(gamesCycle, gc);
                }
            }
            db.SaveChanges();
        }

        private void UpdateIfDifferentObject(ExcelGameDto dto, GamesCycle ctxModel)
        {
            string guestTeamTitle = null;
            string homeTeamTitle = null;
            if (ctxModel.Group.IsIndividual)
            {
                guestTeamTitle = $"{ctxModel.TeamsPlayer?.User?.FullName} ({ctxModel.TeamsPlayer?.Team?.Title})";
                homeTeamTitle = $"{ctxModel.TeamsPlayer1?.User?.FullName} ({ctxModel.TeamsPlayer1?.Team?.Title})";
            }
            #region If/Else
            if (ctxModel.StartDate != dto.Date)
            {
                ctxModel.StartDate = dto.Date;
            }

            else if (ctxModel.Auditorium != null && ctxModel.Auditorium.Name != dto.Auditorium)
            {
                ctxModel.StartDate = dto.Date;
            }

            else if (ctxModel.Group != null && ctxModel.Group.Name != dto.Groupe)
            {
                ctxModel.StartDate = dto.Date;
            }

            else if (ctxModel.GuestTeam != null && ctxModel.GuestTeam.Title != dto.GuestTeam ||
                ctxModel.TeamsPlayer != null && !String.IsNullOrEmpty(guestTeamTitle) && guestTeamTitle == dto.GuestCompetitor)
            {
                ctxModel.StartDate = dto.Date;
            }

            else if (ctxModel.GuestTeamScore != dto.GuestTeamScore)
            {
                ctxModel.StartDate = dto.Date;
            }
            else if (ctxModel.HomeTeamScore != dto.HomeTeamScore)
            {
                ctxModel.StartDate = dto.Date;
            }
            else if (GetRefereesNames(ctxModel.RefereeIds) != dto.Referees)
            {
                ctxModel.StartDate = dto.Date;
            }
            else if (GetRefereesNames(ctxModel.SpectatorIds) != dto.Referees)
            {
                ctxModel.StartDate = dto.Date;
            }
            else if (ctxModel.HomeTeam != null && ctxModel.HomeTeam.Title != dto.HomeTeam ||
                ctxModel.TeamsPlayer1 != null && !String.IsNullOrEmpty(homeTeamTitle) && homeTeamTitle == dto.HomeCompetitor)
            {
                ctxModel.StartDate = dto.Date;
            }
            else if (ctxModel.GameSets.Any())
            {
                var sets = ctxModel.GameSets.ToArray();

                var set1 = sets.Length > 0 && sets[0].HomeTeamScore > 0 && sets[0].GuestTeamScore > 0 ? string.Format("{0} - {1}", sets[0].HomeTeamScore, sets[0].GuestTeamScore) : "";
                var set2 = sets.Length > 1 && sets[1].HomeTeamScore > 0 && sets[1].GuestTeamScore > 0 ? string.Format("{0} - {1}", sets[1].HomeTeamScore, sets[1].GuestTeamScore) : "";
                var set3 = sets.Length > 2 && sets[2].HomeTeamScore > 0 && sets[2].GuestTeamScore > 0 ? string.Format("{0} - {1}", sets[2].HomeTeamScore, sets[2].GuestTeamScore) : "";
                if (set1 != dto.Set1 || set2 != dto.Set2 || set3 != dto.Set3)
                {
                    ctxModel.StartDate = dto.Date;
                }
            }
            #endregion
        }


        public void UpdateGamesDate(List<ExcelGameDto> games)
        {
            foreach (var game in games)
            {
                var entity = db.GamesCycles.FirstOrDefault(g => g.CycleId == game.GameId);

                if (entity != null)
                {
                    var date = game.Date.Date;
                    TimeSpan time;
                    if (TimeSpan.TryParse(game.Time, out time))
                    {
                        date = date.AddHours(time.Hours).AddMinutes(time.Minutes);
                        entity.StartDate = date;
                    }
                }
            }
            db.SaveChanges();
        }

        public void UpdateTeamStandingsFromModel(List<StandingDTO> standings, int standingId, string newUrl)
        {
            //TODO clear previous data if url is different.
            var existedTeamStandingGame = db.TeamStandings.Where(x => x.TeamStandingGamesId == standingId && x.TeamStandingGame.GamesUrl != newUrl).ToList();
            if (existedTeamStandingGame.Count > 0)
            {
                db.TeamStandings.RemoveRange(existedTeamStandingGame);
                Save();
            }


            foreach (var standing in standings)
            {
                var strToIntRank = Convert.ToInt32(standing.Rank.Replace(".", string.Empty));

                var dbStanding =
                    db.TeamStandings.FirstOrDefault(
                        x =>
                            x.Team == standing.Team && x.TeamStandingGamesId == standingId);

                //if we find the team update it
                if (dbStanding != null)
                {
                    dbStanding.Rank = strToIntRank;
                    dbStanding.Games = standing.Games.ToByte();
                    dbStanding.Wins = standing.Win.ToByte();
                    dbStanding.Lost = standing.Lost.ToByte();
                    dbStanding.Pts = standing.Pts.ToByte();
                    dbStanding.Papf = standing.PaPf;
                    dbStanding.Home = standing.Home;
                    dbStanding.Road = standing.Road;
                    dbStanding.ScoreHome = standing.ScoreHome;
                    dbStanding.ScoreRoad = standing.ScoreRoad;
                    dbStanding.Last5 = standing.Last5;
                    dbStanding.PlusMinusField = standing.PlusMinusField;
                }
                //else create new teamStanding
                else
                {
                    var newTeamStnading = new TeamStanding
                    {
                        Team = standing.Team,
                        Rank = strToIntRank,
                        Games = standing.Games.ToByte(),
                        Wins = standing.Win.ToByte(),
                        Lost = standing.Lost.ToByte(),
                        Pts = standing.Pts.ToByte(),
                        Papf = standing.PaPf,
                        Home = standing.Home,
                        Road = standing.Road,
                        ScoreHome = standing.ScoreHome,
                        ScoreRoad = standing.ScoreRoad,
                        Last5 = standing.Last5,
                        TeamStandingGamesId = standingId,
                        PlusMinusField = standing.PlusMinusField
                    };



                    db.TeamStandings.Add(newTeamStnading);
                }
            }
            try
            {
                Save();
            }

            catch (DbEntityValidationException e)
            {

            }
            catch (Exception)
            {

                throw;
            }
        }

        public void UpdateTeamStandingsFromScrapper(IList<string> gamesUrl)
        {
            var standings = new List<StandingDTO>();
            var service = new ScrapperService();

            foreach (var standingUrl in gamesUrl)
            {
                var items = service.StandingScraper(standingUrl);
                standings.AddRange(items);
            }

            foreach (var standing in standings)
            {
                //var strToIntRank = Convert.ToInt32(standing.Rank.Replace(".", string.Empty));
                int outVal;
                int.TryParse(standing.Rank.Replace(".", string.Empty), out outVal);

                var dbStanding =
                    db.TeamStandings.FirstOrDefault(
                        x =>
                            x.Team == standing.Team);

                //if we find the team update it
                if (dbStanding != null)
                {
                    dbStanding.Rank = outVal;
                    dbStanding.Games = standing.Games.ToByte();
                    dbStanding.Wins = standing.Win.ToByte();
                    dbStanding.Lost = standing.Lost.ToByte();
                    dbStanding.Pts = standing.Pts.ToByte();
                    dbStanding.Papf = standing.PaPf;
                    dbStanding.Home = standing.Home;
                    dbStanding.Road = standing.Road;
                    dbStanding.ScoreHome = standing.ScoreHome;
                    dbStanding.ScoreRoad = standing.ScoreRoad;
                    dbStanding.Last5 = standing.Last5;
                    dbStanding.PlusMinusField = standing.PlusMinusField;
                }
                //else create new teamStanding
                else
                {
                    var standingGameId = db.TeamStandingGames.FirstOrDefault(x => x.GamesUrl == standing.Url)?.Id;

                    var newTeamStnading = new TeamStanding
                    {
                        Team = standing.Team,
                        Rank = outVal,
                        Games = standing.Games.ToByte(),
                        Wins = standing.Win.ToByte(),
                        Lost = standing.Lost.ToByte(),
                        Pts = standing.Pts.ToByte(),
                        Papf = standing.PaPf,
                        Home = standing.Home,
                        Road = standing.Road,
                        ScoreHome = standing.ScoreHome,
                        ScoreRoad = standing.ScoreRoad,
                        Last5 = standing.Last5,
                        TeamStandingGamesId = standingGameId,
                        PlusMinusField = standing.PlusMinusField,
                    };



                    db.TeamStandings.Add(newTeamStnading);
                }
            }
            try
            {
                Save();
                service.Quit();
            }

            catch (DbEntityValidationException e)
            {

            }
            catch (Exception)
            {

                throw;
            }

        }

        public void UpdateGamesSchedulesFromDto(List<SchedulerDTO> gameCycles, int clubId, int scheduleId, string newUrl, bool isScraper = false)
        {
            if (!isScraper)
            {
                var existedSchedulerGame = db.TeamScheduleScrappers.Where(x => x.SchedulerScrapperGamesId == scheduleId).ToList();

                if (existedSchedulerGame.Count > 0)
                {
                    db.TeamScheduleScrappers.RemoveRange(existedSchedulerGame);
                    Save();
                }
            }
            
            foreach (var game in gameCycles)
            {
                var startDate = DateTime.Parse(game.Time, new CultureInfo("he-IL"));
                var newGame = new TeamScheduleScrapper
                    {
                        Auditorium = game.Auditorium,
                        GuestTeam = game.GuestTeam,
                        HomeTeam = game.HomeTeam,
                        SchedulerScrapperGamesId = scheduleId,
                        Score = string.Format("{0}:{1}", game.HomeTeamScore, game.GuestTeamScore),
                        StartDate = startDate,
                    };
                 db.TeamScheduleScrappers.Add(newGame);
            }
            Save();
        }

        public void UpdateGamesSchedulersFromScrapper(List<string> gamesUrl)
        {
            var gameCycles = new List<SchedulerDTO>();

            var service = new ScrapperService();

            foreach (var gameUrl in gamesUrl)
            {
                var gameCycle = service.SchedulerScraper(gameUrl);
                gameCycles.AddRange(gameCycle);

            }

            var gamesToUpdate = db.TeamScheduleScrapperGames.Where(x => gamesUrl.Contains(x.GameUrl)).SelectMany(x => x.TeamScheduleScrappers).ToList();
            foreach (var game in gameCycles)
            {
                DateTime outDate;
                DateTime.TryParse(game.Time, new CultureInfo("he-IL"), DateTimeStyles.None, out outDate);
                var existedGame = gamesToUpdate.FirstOrDefault(x => x.GuestTeam == game.GuestTeam && x.HomeTeam == game.HomeTeam && x.Auditorium == game.Auditorium);
                if (existedGame != null)
                {
                    existedGame.Score = string.Format("{0}:{1}", game.HomeTeamScore, game.GuestTeamScore);
                }
                else
                {
                    var schedullerScrapperId = db.TeamScheduleScrapperGames.FirstOrDefault(x => x.GameUrl == game.Url)?.Id;
                    if (schedullerScrapperId.HasValue)
                    {
                        var newGame = new TeamScheduleScrapper
                        {
                            Auditorium = game.Auditorium,
                            GuestTeam = game.GuestTeam,
                            HomeTeam = game.HomeTeam,
                            SchedulerScrapperGamesId = schedullerScrapperId.Value,
                            Score = string.Format("{0}:{1}", game.HomeTeamScore, game.GuestTeamScore),
                            StartDate = outDate,
                        };

                        db.TeamScheduleScrappers.Add(newGame);
                    }
                }
            }
            Save();
            service.Quit();
        }

        public int SaveTeamGameUrl(int teamId, string gameUrl, int clubId, string externalTeamName, int seasonId)
        {
            try
            {
                var teamShcedule = db.TeamScheduleScrapperGames.FirstOrDefault(x => x.ClubId == clubId && x.TeamId == teamId);
                if (teamShcedule != null)
                {
                    teamShcedule.GameUrl = gameUrl;
                    teamShcedule.ExternalTeamName = externalTeamName;
                    teamShcedule.SeasonId = seasonId;
                    db.SaveChanges();
                    return teamShcedule.Id;
                }
                else
                {
                    var newTeamSchedule = new TeamScheduleScrapperGame
                    {
                        ClubId = clubId,
                        GameUrl = gameUrl,
                        TeamId = teamId,
                        ExternalTeamName = externalTeamName,
                        SeasonId = seasonId
                    };
                    db.TeamScheduleScrapperGames.Add(newTeamSchedule);
                    db.SaveChanges();

                    return newTeamSchedule.Id;
                }


            }
            catch (Exception)
            {

                return -1;
            }
        }

        public void UpdateGamesInGameSchedule(int groupId, int stageId, int?[] teamsIds)
        {
            var isIndividualGroup = db.Groups.Find(groupId).IsIndividual;
            var gamesByGroup = db.GamesCycles.Where(g => g.GroupId == groupId && g.StageId == stageId).ToList();
            for (int i = 0; i < teamsIds.Count(); i++)
            {
                var id = teamsIds[i];
                var teamPosition = i + 1;
                var teamGamesInScheduleGuest = gamesByGroup.Where(g => g.GuestTeamPos == teamPosition);
                foreach (var game in teamGamesInScheduleGuest)
                {
                    if (isIndividualGroup)
                        game.GuestAthleteId = id;
                    else
                        game.GuestTeamId = id;
                }

                var teamGamesInScheduleHome = gamesByGroup.Where(g => g.HomeTeamPos == teamPosition);
                foreach (var game in teamGamesInScheduleHome)
                {
                    if (isIndividualGroup)
                        game.HomeAthleteId = id;
                    else
                        game.HomeTeamId = id;
                }

                var team1Bracket = db.PlayoffBrackets.SingleOrDefault(b => b.GroupId == groupId && b.Team1GroupPosition == teamPosition);
                if (team1Bracket != null)
                {
                    if (isIndividualGroup)
                        team1Bracket.Athlete1Id = id;
                    else
                        team1Bracket.Team1Id = id;
                }

                var team2Bracket = db.PlayoffBrackets.SingleOrDefault(b => b.GroupId == groupId && b.Team2GroupPosition == teamPosition);
                if (team2Bracket != null)
                {
                    if (isIndividualGroup)
                        team2Bracket.Athlete2Id = id;
                    else
                        team2Bracket.Team2Id = id;
                }
            }
            db.SaveChanges();
        }

        public void UpdateTennisGamesInGameSchedule(int groupId, int stageId, string[] teamsIds, bool isPairs)
        {
            var isIndividualGroup = db.TennisGroups.Find(groupId).IsIndividual;
            var gamesByGroup = db.TennisGameCycles.Where(g => g.GroupId == groupId && g.StageId == stageId).ToList();
            for (int i = 0; i < teamsIds.Count(); i++)
            {
                int? id = null;
                int? pairId = null;
                if (isPairs)
                {
                    var ids = teamsIds[i]?.Split('/');
                    if (ids.Any())
                    {
                        id = !string.IsNullOrEmpty(ids[0]) ? (int?)int.Parse(ids[0]) : null;
                        pairId = !string.IsNullOrEmpty(ids[0]) ? (int?)int.Parse(ids[1]) : null;
                    }
                }
                else
                {
                    id = !string.IsNullOrEmpty(teamsIds[i]) ? (int?)int.Parse(teamsIds[i]) : null;
                }
                var teamPosition = i + 1;
                var teamGamesInScheduleGuest = gamesByGroup.Where(g => g.SecondPlayerPos == teamPosition);
                foreach (var game in teamGamesInScheduleGuest)
                {
                    game.SecondPlayerId = id;
                    if (isPairs) game.SecondPlayerPairId = pairId;
                }

                var teamGamesInScheduleHome = gamesByGroup.Where(g => g.FirstPlayerPos == teamPosition);
                foreach (var game in teamGamesInScheduleHome)
                {
                    game.FirstPlayerId = id;
                    if (isPairs) game.FirstPlayerPairId = pairId;
                }

                var team1Bracket = db.TennisPlayoffBrackets.Where(b => b.GroupId == groupId).FirstOrDefault();
                if (team1Bracket != null)
                {
                    team1Bracket.FirstPlayerId = id;
                    if (isPairs) team1Bracket.FirstPlayerPairId = pairId;
                }

                var team2Bracket = db.TennisPlayoffBrackets.Where(b => b.GroupId == groupId).FirstOrDefault();
                if (team2Bracket != null)
                {
                    team2Bracket.SecondPlayerId = id;
                    if (isPairs) team2Bracket.SecondPlayerPairId = pairId;
                }
            }
            db.SaveChanges();
        }

        public int GetTeamsNumberInGroup(int groupId)
        {
            return db.GroupsTeams.Count(g => g.GroupId == groupId);
        }

        public int GetTennisTeamsNumberInGroup(int groupId)
        {
            return db.TennisGroupTeams.Count(g => g.GroupId == groupId);
        }

        public void UpdateGameAndBracket(int gameCycleId, int? gameHomeTeamId)
        {
            var game = db.GamesCycles.SingleOrDefault(g => g.CycleId == gameCycleId);
            if (game != null)
            {
                game.HomeTeamId = gameHomeTeamId;
                if (game.BracketId.HasValue)
                {
                    var bracket = db.PlayoffBrackets.SingleOrDefault(b => b.Id == game.BracketId);
                    if (bracket != null)
                    {
                        bracket.Team1Id = gameHomeTeamId;
                    }
                }
            }

            db.SaveChanges();
        }

        public void UpdateGameAndBracketGuest(int gameCycleId, int? teamId)
        {
            var game = db.GamesCycles.SingleOrDefault(g => g.CycleId == gameCycleId);
            if (game != null)
            {
                game.GuestTeamId = teamId;
                if (game.BracketId.HasValue)
                {
                    var bracket = db.PlayoffBrackets.SingleOrDefault(b => b.Id == game.BracketId);
                    if (bracket != null)
                    {
                        bracket.Team2Id = teamId;
                    }
                }
            }

            db.SaveChanges();
        }

        public void UpdateGameAndBracketForAthlete(int gameCycleId, int? gameHomeAthleteId)
        {
            var game = db.GamesCycles.SingleOrDefault(g => g.CycleId == gameCycleId);
            if (game != null)
            {
                game.HomeAthleteId = gameHomeAthleteId;
                if (game.BracketId.HasValue)
                {
                    var bracket = db.PlayoffBrackets.SingleOrDefault(b => b.Id == game.BracketId);
                    if (bracket != null)
                    {
                        bracket.Athlete1Id = gameHomeAthleteId;
                    }
                }
            }
            db.SaveChanges();
        }

        public bool UpdateTennisGameAndBracketForFirstPlayer(TennisGameCycle game, int? firstPlayerId, int? firsPairPlayerId)
        {
            bool shouldSave = false;
            if (game != null)
            {
                if (game.FirstPlayerId != firstPlayerId || game.FirstPlayerPairId != firsPairPlayerId)
                {
                    shouldSave = true;
                }
                game.FirstPlayerId = firstPlayerId;
                game.FirstPlayerPairId = firsPairPlayerId;
                if (game.BracketId.HasValue)
                {
                    var bracket = game.TennisPlayoffBracket;
                    if (bracket == null)
                    {
                        bracket = db.TennisPlayoffBrackets.SingleOrDefault(b => b.Id == game.BracketId);
                    }
                    if (bracket != null)
                    {
                        if (bracket.FirstPlayerId != firstPlayerId || bracket.FirstPlayerPairId != firsPairPlayerId)
                        {
                            shouldSave = true;
                        }
                        bracket.FirstPlayerId = firstPlayerId;
                        bracket.FirstPlayerPairId = firsPairPlayerId;
                    }
                }
            }
            return shouldSave;
        }

        public bool UpdateTennisGameAndBracketForSecondPlayer(TennisGameCycle game, int? secondPlayerId, int? secondPairPlayerId)
        {
            bool shouldSave = false;
            if (game != null)
            {
                if (game.SecondPlayerId != secondPlayerId || game.SecondPlayerPairId != secondPairPlayerId)
                {
                    shouldSave = true;
                }
                game.SecondPlayerId = secondPlayerId;
                game.SecondPlayerPairId = secondPairPlayerId;
                if (game.BracketId.HasValue)
                {
                    var bracket = game.TennisPlayoffBracket;
                    if (bracket == null)
                    {
                        bracket = db.TennisPlayoffBrackets.SingleOrDefault(b => b.Id == game.BracketId);
                    }
                    if (bracket != null)
                    {
                        if (bracket.SecondPlayerId != secondPlayerId || bracket.SecondPlayerPairId != secondPairPlayerId)
                        {
                            shouldSave = true;
                        }
                        bracket.SecondPlayerId = secondPlayerId;
                        bracket.SecondPlayerPairId = secondPairPlayerId;
                    }
                }
            }
            return shouldSave;
        }

        public IEnumerable<GameDto> GetByClubId(int clubId, DateTime date)
        {
            var club = db.Clubs.Find(clubId);
            if (club == null) return Enumerable.Empty<GameDto>();

            var games = GetGamesForClub(clubId).ToList();

            if (!club.UnionId.HasValue)
            {
                games.AddRange(GetGamesForSectionClub(clubId));
            }

            return games;
        }

        private IEnumerable<GameDto> GetGamesForSectionClub(int clubId)
        {
            var games = from g in db.TeamScheduleScrappers
                        where g.Score == "0:0" && g.TeamScheduleScrapperGame.ClubId == clubId
                        let team = g.TeamScheduleScrapperGame
                        select new GameDto
                        {
                            GameId = -g.Id,
                            //GameCycleStatus = g.GameStatus,
                            StartDate = g.StartDate,
                            HomeTeamId = g.HomeTeam == team.ExternalTeamName ? team.TeamId : -1,
                            HomeTeamTitle = g.HomeTeam,
                            //HomeTeamScore = g.HomeTeamScore,
                            GuestTeamTitle = g.GuestTeam,
                            GuestTeamId = g.GuestTeam == team.ExternalTeamName ? team.TeamId : -1,
                            //GuestTeamScore = g.GuestTeamScore,
                            Auditorium = g.Auditorium,
                            AuditoriumAddress = g.Auditorium,
                            //HomeTeamLogo = homeTeam.Logo,
                            //GuestTeamLogo = guestTeam.Logo,
                            //CycleNumber = g.CycleNum,
                            //LeagueId = stage.LeagueId,
                            //LeagueName = league.Name,
                            //MaxHandicap = league.MaximumHandicapScoreValue,
                            //IsHandicapEnabled = league.Union.IsHadicapEnabled
                        };
            return games;
        }

        private IEnumerable<GameDto> GetGamesForClub(int clubId)
        {
            var clubLeagues = db.Leagues.Where(l => l.ClubId == clubId)?.Select(l => l.LeagueId);
            var games = db.GamesCycles.Where(gc => gc.GameStatus != GameStatus.Ended && gc.IsPublished
                && (gc.HomeTeam.ClubTeams.Any(c => c.ClubId == clubId) || gc.GuestTeam.ClubTeams.Any(c => c.ClubId == clubId)
                || gc.HomeTeam.LeagueTeams.Any(x => clubLeagues.Contains(x.LeagueId)) || gc.GuestTeam.LeagueTeams.Any(x => clubLeagues.Contains(x.LeagueId))));


            if (games.Any())
            {
                foreach (var game in games)
                {
                    var homeTeamDetails = game.HomeTeam?.TeamsDetails.OrderByDescending(t => t.SeasonId).FirstOrDefault();
                    var guestTeamDetails = game.GuestTeam?.TeamsDetails.OrderByDescending(t => t.SeasonId).FirstOrDefault();
                    yield return new GameDto
                    {
                        GameId = game.CycleId,
                        GameCycleStatus = game.GameStatus,
                        StartDate = game.StartDate,
                        HomeTeamId = game.HomeTeamId,
                        HomeTeamTitle = homeTeamDetails == null ? game.HomeTeam?.Title : homeTeamDetails.TeamName,
                        HomeTeamScore = game.HomeTeamScore,
                        GuestTeamTitle = guestTeamDetails == null ? game.GuestTeam?.Title : guestTeamDetails.TeamName,
                        GuestTeamId = game.GuestTeamId,
                        GuestTeamScore = game.GuestTeamScore,
                        Auditorium = game.Auditorium != null ? game.Auditorium.Name : string.Empty,
                        AuditoriumAddress = game.Auditorium != null ? game.Auditorium.Address : string.Empty,
                        HomeTeamLogo = game.HomeTeam?.Logo,
                        GuestTeamLogo = game.GuestTeam?.Logo,
                        CycleNumber = game.CycleNum,
                        LeagueId = game.Stage.LeagueId,
                        LeagueName = game.Stage.League.Name,
                        MaxHandicap = game.Stage.League.MaximumHandicapScoreValue,
                        IsHandicapEnabled = game.Stage.League.Union != null && game.Stage.League.Union.IsHadicapEnabled
                    };
                }
            }
        }

        public IQueryable<GameDto> GetByUnionId(int unionId, DateTime date)
        {
            var games = from l in db.Leagues
                        from s in l.Stages
                        from gameCycle in s.GamesCycles
                        where l.UnionId == unionId && gameCycle.GameStatus != GameStatus.Ended && gameCycle.IsPublished
                        join homeTeam in db.Teams on gameCycle.HomeTeamId equals homeTeam.TeamId
                        join guestTeam in db.Teams on gameCycle.GuestTeamId equals guestTeam.TeamId
                        join auditorium in db.Auditoriums on gameCycle.AuditoriumId equals auditorium.AuditoriumId into aud
                        join stage in db.Stages on gameCycle.StageId equals stage.StageId
                        join league in db.Leagues on stage.LeagueId equals league.LeagueId

                        from gameCycleAuditorion in aud.DefaultIfEmpty()
                        orderby gameCycle.StartDate

                        let homeTeamDetails = homeTeam.TeamsDetails.OrderByDescending(t => t.SeasonId).FirstOrDefault()
                        let guestTeamDetails = guestTeam.TeamsDetails.OrderByDescending(t => t.SeasonId).FirstOrDefault()

                        select new GameDto
                        {
                            GameId = gameCycle.CycleId,
                            GameCycleStatus = gameCycle.GameStatus,
                            StartDate = gameCycle.StartDate,
                            HomeTeamId = homeTeam.TeamId,
                            HomeTeamTitle = homeTeamDetails == null ? homeTeam.Title : homeTeamDetails.TeamName,
                            HomeTeamScore = gameCycle.HomeTeamScore,
                            GuestTeamTitle = guestTeamDetails == null ? guestTeam.Title : guestTeamDetails.TeamName,
                            GuestTeamId = guestTeam.TeamId,
                            GuestTeamScore = gameCycle.GuestTeamScore,
                            Auditorium = gameCycle.Auditorium != null ? gameCycle.Auditorium.Name : string.Empty,
                            AuditoriumAddress = gameCycle.Auditorium != null ? gameCycle.Auditorium.Address : string.Empty,
                            HomeTeamLogo = homeTeam.Logo,
                            GuestTeamLogo = guestTeam.Logo,
                            CycleNumber = gameCycle.CycleNum,
                            LeagueId = stage.LeagueId,
                            LeagueName = league.Name,
                            MaxHandicap = league.MaximumHandicapScoreValue,
                            IsHandicapEnabled = league.Union.IsHadicapEnabled
                        };

            return games;
        }
        public void UpdateRefereesInCycle(int cycleId, string refereesIds)
        {
            var gameCycle = db.GamesCycles.Find(cycleId);

            if (String.IsNullOrEmpty(refereesIds))
                gameCycle.RefereeIds = null;
            else
                gameCycle.RefereeIds = refereesIds;

            db.SaveChanges();
        }

        public void UpdateDesksInCycle(int cycleId, string desksIds)
        {
            var gameCycle = db.GamesCycles.Find(cycleId);

            if (String.IsNullOrEmpty(desksIds))
                gameCycle.DeskIds = null;
            else
                gameCycle.DeskIds = desksIds;

            db.SaveChanges();
        }

        public IEnumerable<string> GetRefereesIds(int cycleId)
        {
            var value = db.GamesCycles.Find(cycleId);
            return value.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<string> GetDesksIds(int cycleId)
        {
            var value = db.GamesCycles.Find(cycleId);
            return value.DeskIds?.Split(',');
        }

        public List<string> GetRefereesNames(int cycleId)
        {
            var gameReferees = db.GamesCycles.FirstOrDefault(g => g.CycleId == cycleId).RefereeIds;
            if (gameReferees != null)
            {
                var gameRefereesIds = gameReferees.Split(',');
                var listOfNames = new List<string>();
                foreach (var id in gameRefereesIds)
                {
                    int testInt;
                    var success = int.TryParse(id, out testInt);
                    if (id == "" || id == null || !success)
                        listOfNames.Add("");
                    else
                        listOfNames.Add(db.Users.FirstOrDefault(u => u.UserId == testInt)?.FullName);
                }
                return listOfNames;
            }
            return null;
        }

        public List<string> GetDeskNames(int cycleId)
        {
            var gameDesks = db.GamesCycles.FirstOrDefault(g => g.CycleId == cycleId).DeskIds;
            if (gameDesks != null)
            {
                var gameDesksIds = gameDesks.Split(',');
                var listOfNames = new List<string>();
                foreach (var id in gameDesksIds)
                {
                    int testInt;
                    var success = int.TryParse(id, out testInt);
                    if (id == "" || id == null || !success)
                        listOfNames.Add("");
                    else
                        listOfNames.Add(db.Users.FirstOrDefault(u => u.UserId == testInt)?.FullName);
                }
                return listOfNames;
            }
            return null;
        }

        public List<string> GetSpectatorsNames(List<string> spectatorsIds, int cycleId)
        {
            var gameSpectators = db.GamesCycles.Find(cycleId).SpectatorIds;
            if (gameSpectators != null)
            {
                var gameSpectatorsIds = gameSpectators.Split(',');
                var listOfNames = new List<string>();
                foreach (var id in gameSpectatorsIds)
                {
                    int testInt;
                    var success = int.TryParse(id, out testInt);
                    if (id == "" || id == null || !success)
                        listOfNames.Add("");
                    else
                        listOfNames.Add(db.Users.FirstOrDefault(u => u.UserId == testInt)?.FullName);
                }
                return listOfNames;
            }
            return null;
        }

        public void AddStatistic(Statistic model)
        {
            db.Statistics.Add(model);
            db.SaveChanges();
        }

        private int CalculateTeamScores(IEnumerable<Statistic> statistics)
        {
            int totalPoints = 0;

            foreach (var statistic in statistics)
            {
                string abbr = statistic.Abbreviation.Replace(" ", string.Empty); // Remove all whitespaces
                switch (abbr)
                {
                    case "+1":
                        totalPoints++;
                        break;
                    case "+2":
                        totalPoints += 2;
                        break;
                    default:
                        break;
                }
            }

            return totalPoints;
        }

        public IEnumerable<string> GetOfficials(int gameId)
        {
            var game = db.GamesCycles.Find(gameId);
            var refereesStr = game?.RefereeIds;
            var referees = !string.IsNullOrEmpty(refereesStr)
                ? refereesStr.Split(',').Select(int.Parse)
                : Enumerable.Empty<int>();
            var spectatorsStr = game?.SpectatorIds;
            var spectators = !string.IsNullOrEmpty(spectatorsStr)
                ? spectatorsStr.Split(',').Select(int.Parse)
                : Enumerable.Empty<int>();

            var ids = referees.Union(spectators);
            var officials = (from id in ids select db.Users.Find(id) into user where user != null select user.FullName);

            return officials;
        }


        public WorkerReportDTO GetWorkerReportInfoDetails(int userId, int relevantId, LogicaName logicalName,
            DateTime? reportStartDate, DateTime? reportEndDate, string officialType, int seasonId,
            string distanceSettings, int userJobId)
        {
            var season = db.Seasons.FirstOrDefault(s => s.Id == seasonId);
            var jobRole = db.JobsRoles.FirstOrDefault(jr => jr.RoleName == officialType);
            var jobRoleId = jobRole?.RoleId;
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            var userJob = db.UsersJobs.FirstOrDefault(uj =>
                uj.UserId == userId && uj.SeasonId == seasonId && uj.Job.RoleId == jobRoleId);

            return new WorkerReportDTO
            {
                WorkerId = userId,
                WorkerIdentNum = user?.IdentNum,
                WorkerFullName = user != null ? user.FullName : "",
                SeasonName = season != null ? season.Name : "",
                OfficialTypeId = jobRoleId,
                WithholdingTax = userJob?.WithhodlingTax,
                WorkerRate = userJob?.RateType,
                JobId = userJobId,
                City = user?.City,
                Address = user?.Address,
                UserJobId = userJob?.Id ?? 0
            };
        }

        public WorkerReportDTO GetWorkerGames(int userId, int relevantId, LogicaName logicalName,
        DateTime? reportStartDate, DateTime? reportEndDate, string officialType, int seasonId,
        string distanceSettings, int userJobId, bool isSaturdayTariff = false)
        {
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            var report = GetWorkerReportInfoDetails(userId, relevantId, logicalName, reportStartDate, reportEndDate, officialType, seasonId, distanceSettings, userJobId);
            if (user == null) return report;
            report.GamesAssigned = GetAllGamesForOfficial(userId, relevantId, seasonId, logicalName, reportStartDate,
                reportEndDate, officialType, distanceSettings, userJobId, isSaturdayTariff);
            report.GamesCount = report.GamesAssigned.Count();
            if (report.GamesAssigned.FirstOrDefault()?.League?.Union?.Section?.IsIndividual == true)
            {
                var allDaysWorked = new List<DateTime?>();
                foreach (var game in report.GamesAssigned)
                {
                    allDaysWorked.AddRange(game.DaysWorked);
                }
                report.DaysCount = allDaysWorked.Distinct().Count();
            }
            else
            {
                report.DaysCount = report.GamesAssigned.Where(c => c.StartDate.HasValue)
                    .Select(c => c.StartDate.Value.ToShortDateString()).Distinct().Count();
            }
            report.LeaguesGrouped = report.GamesAssigned?.GroupBy(c => c.League);
            report.TotalFeeCount = report.GamesAssigned.Sum(r => r.LeagueFee ?? 0);

            return report;
        }

        private List<GameShortDto> GetAllGamesForOfficial(int userId, int relevantId, int seasonId, LogicaName logicalName,
            DateTime? reportStartDate, DateTime? reportEndDate, string officialType, string distanceSettings, int userJobId, bool isSaturdayTariff)
        {
            Union union = null;
            var removeTravelForUnpublishedGames = false;

            if (logicalName == LogicaName.Union)
            {
                union = db.Unions.FirstOrDefault(u => u.UnionId == relevantId);
                removeTravelForUnpublishedGames = union?.ReportRemoveTravelDistance == true;
            }
            else if (logicalName == LogicaName.Club)
            {
                var club = db.Clubs.FirstOrDefault(c => c.ClubId == relevantId);
                union = club?.Union;
                removeTravelForUnpublishedGames = club?.ReportRemoveTravelDistance == true;
            }

            if (union != null)
            {
                var isUnionReportEnabled = union.IsOfficialSettingsChecked;
                var unionLeagues = union.Leagues.Where(league => league.SeasonId == seasonId);
                var games = new List<GamesCycle>();
                foreach (var league in unionLeagues)
                {
                    var leaguesStages = league.Stages?.Where(s => s.IsArchive == false);

                    if (leaguesStages == null || !leaguesStages.Any()) continue;

                    foreach (var stage in leaguesStages)
                    {
                        if (stage.GamesCycles != null && stage.GamesCycles.Any())
                        {
                            var selectedGames = stage.GamesCycles.Where(cycle =>
                                  cycle.StartDate >= reportStartDate && cycle.StartDate <= reportEndDate);
                            if (selectedGames != null && selectedGames.Any())
                            {
                                games.AddRange(selectedGames);
                            }
                        }
                    }
                }
                return GetGameRecords(userId, relevantId, seasonId, logicalName, distanceSettings, userJobId, games, officialType, isUnionReportEnabled, removeTravelForUnpublishedGames, isSaturdayTariff);
            }
            return new List<GameShortDto>();
        }

        private List<GameShortDto> GetGameRecords(int userId, int relevantId, int seasonId, LogicaName logicalName,
            string distanceSettings, int userJobId, List<GamesCycle> games, string officialType, bool isUnionReportEnabled, bool removeTravelForUnpublishedGames, bool isSaturdayTariff = false)
        {
            var result = new List<GameShortDto>();
            var gamesOfUser = games.Where(game => !string.IsNullOrEmpty(game.RefereeIds) && game?.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.Select(int.Parse)?.Contains(userId) == true
                                               || !string.IsNullOrEmpty(game.SpectatorIds) && game?.SpectatorIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.Select(int.Parse)?.Contains(userId) == true
                                               || !string.IsNullOrEmpty(game.DeskIds) && game?.DeskIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.Select(int.Parse)?.Contains(userId) == true)
                                               .OrderBy(c => c.StartDate)
                                               .ToList();

            if (gamesOfUser != null && gamesOfUser.Any())
            {
                foreach (var game in gamesOfUser)
                {
                    var roleName = GetUserRoleAtGame(game, userId);
                    var officialsSetting = isUnionReportEnabled
                                ? (dynamic)game.Stage?.League?.Union?.UnionOfficialSettings?.FirstOrDefault(ls => ls.JobsRole?.RoleName == roleName)
                                    ?? (dynamic)game.Stage?.League?.Club?.Union?.UnionOfficialSettings?.FirstOrDefault(ls => ls.JobsRole?.RoleName == roleName)
                                : (dynamic)game.Stage?.League?.LeagueOfficialsSettings?.FirstOrDefault(ls => ls.JobsRole?.RoleName == roleName);
                    var official = db.UsersJobs.FirstOrDefault(j => j.Id == userJobId);
                    var gamesAtCycle = gamesOfUser.Where(c => c.StartDate.ToShortDateString() == game.StartDate.ToShortDateString()).OrderBy(c => c.StartDate).ToList();
                    var reportDetails = game.OfficialGameReportDetails.FirstOrDefault(x => x.UserJobId == userJobId);
                    var union = game.Group?.Stage?.League?.Union ?? game?.Group?.Stage?.League?.Club?.Union;
                    result.Add(new GameShortDto
                    {
                        Id = game.CycleId,
                        StartDate = game.StartDate,
                        AuditoriumName = game.Auditorium?.Name ?? "",
                        OfficialAddress = official?.User?.Address,
                        OfficialCity = official?.User?.City,
                        Role = roleName,
                        HomeTeamName = game.HomeTeam?.Title ?? "Home team",
                        GuestTeamName = game.GuestTeam?.Title ?? "Guest team",
                        League = game.Group?.Stage?.League,
                        LeagueFee = getJobFee(GetLeaguePayment(officialsSetting, official?.RateType, "Game"), game.StartDate, isSaturdayTariff, union?.SaturdaysTariffFromTime, union?.SaturdaysTariffToTime),
                        TravelDistance = GetTravelDistance(userId, relevantId, seasonId, logicalName, distanceSettings, removeTravelForUnpublishedGames, official, game, reportDetails, gamesAtCycle),
                        Union = union,
                        IsUnionReport = isUnionReportEnabled,
                        Comment = reportDetails?.Comment,
                        DaysWorked = official.TravelInformations.Where(x => x.FromHour.HasValue).Select(x => x.FromHour).ToList()
                    });
                }
            }

            return result;
        }

        private double? GetTravelDistance(int userId, int relevantId, int seasonId, LogicaName logicalName, string distanceSettings, bool removeTravelForUnpublishedGames, UsersJob official, GamesCycle game, OfficialGameReportDetail reportDetails, List<GamesCycle> gamesAtCycle)
        {
            var travelDistance = 0D;
            if (!game.IsPublished && removeTravelForUnpublishedGames)
            {
                return (double?)null;
            }

            if (official.TravelInformations.Any(x => x?.NoTravel == true))
            {
                return travelDistance;
            }

            travelDistance += reportDetails?.TravelDistance
                              ?? GetTravelDistanceInKm(userId, relevantId, logicalName, seasonId, distanceSettings,
                                  game,
                                  gamesAtCycle) ?? 0D;

            return travelDistance;
        }

        private string GetUserRoleAtGame(GamesCycle game, int workerId)
        {
            if (game?.RefereeIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).Contains(workerId) == true)
                return "referee";
            else if (game?.SpectatorIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).Contains(workerId) == true)
                return "spectator";
            else if (game?.DeskIds?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).Contains(workerId) == true)
                return "desk";
            else return string.Empty;
        }

        public decimal? GetLeaguePayment(dynamic officialsSetting, string rateType, string type)
        {
            switch (rateType)
            {
                case "RateA":
                    return string.Equals(type, "Game") ? officialsSetting?.RateAPerGame : officialsSetting?.RateAForTravel;
                case "RateB":
                    return string.Equals(type, "Game") ? officialsSetting?.RateBPerGame : officialsSetting?.RateBForTravel;
                case "RateC":
                    return string.Equals(type, "Game") ? officialsSetting?.RateCPerGame : officialsSetting?.RateCForTravel;
                default:
                    return string.Equals(type, "Game") ? officialsSetting?.PaymentPerGame : officialsSetting?.PaymentTravel;
            }
        }

        private double? GetTravelDistanceInKm(int workerId, int relevantId, LogicaName logicalName, int? seasonId,
            string distanceSettings, GamesCycle currentGame, List<GamesCycle> gamesAtCycle)
        {
            var worker = db.Users.SingleOrDefault(w => w.UserId == workerId);
            double? distance = null;
            switch (distanceSettings)
            {
                case "table":
                    {
                        var distanceTableInstance = logicalName == LogicaName.Union
                            ? db.DistanceTables.Where(c => c.UnionId == relevantId && c.SeasonId == seasonId)
                            : db.DistanceTables.Where(c => c.ClubId == relevantId && c.SeasonId == seasonId);
                        var uniqueCities = GetUniqueCities(distanceTableInstance);
                        GetCities(worker?.Address, currentGame, gamesAtCycle, uniqueCities, out string homeCity, out string guestCity, out string workerHomeCity);
                        distance = gamesAtCycle.Count == 1
                            ? GetDistanceByCities(homeCity, guestCity, distanceTableInstance) * 2
                            : string.IsNullOrEmpty(workerHomeCity)
                                ? GetDistanceByCities(homeCity, guestCity, distanceTableInstance)
                                : GetDistanceByCities(homeCity, guestCity, distanceTableInstance, workerHomeCity); //sorry for this construction %)
                        break;
                    }
                case "googleMapsApi":
                    {
                        GetAddresses(worker?.Address, currentGame, gamesAtCycle, out string homeAddress, out string guestAddress, out string workerHomeAddress);
                        distance = gamesAtCycle.Count == 1
                            ? googleMapsApiService.GetDistance(homeAddress, guestAddress) + googleMapsApiService.GetDistance(guestAddress, homeAddress)
                            : string.IsNullOrEmpty(workerHomeAddress)
                                ? googleMapsApiService.GetDistance(homeAddress, guestAddress)
                                : googleMapsApiService.GetDistance(homeAddress, guestAddress, workerHomeAddress); //sorry for this construction %)
                        break;
                    }
            }
            return distance;
        }

        private void GetAddresses(string workerAddress, GamesCycle currentGame, IList<GamesCycle> gamesAtCycle,
            out string homeAddress, out string guestAddress, out string workerHomeAddress)
        {
            homeAddress = string.Empty;
            guestAddress = string.Empty;
            workerHomeAddress = string.Empty;

            if (gamesAtCycle != null && gamesAtCycle.Count == 1)
            {
                homeAddress = workerAddress;
                guestAddress = currentGame?.Auditorium?.Address;
            }
            else if (gamesAtCycle != null && gamesAtCycle.Count > 1)
            {
                if (gamesAtCycle.IsFirst(currentGame))
                {
                    homeAddress = workerAddress;
                    guestAddress = currentGame?.Auditorium?.Address;
                }
                else if (!gamesAtCycle.IsFirst(currentGame) && !gamesAtCycle.IsLast(currentGame))
                {
                    homeAddress = currentGame?.Auditorium?.Address;
                    var cityIndex = gamesAtCycle.IndexOf(currentGame);
                    guestAddress = gamesAtCycle[cityIndex - 1]?.Auditorium?.Address;
                }
                else if (gamesAtCycle.IsLast(currentGame))
                {
                    var cityIndex = gamesAtCycle.IndexOf(currentGame);
                    homeAddress = gamesAtCycle[cityIndex - 1]?.Auditorium?.Address;
                    guestAddress = currentGame?.Auditorium?.Address;
                    workerHomeAddress = workerAddress;
                }
            }

        }

        private void GetCities(string workerHomeAddress, GamesCycle currentGame, IList<GamesCycle> gamesAtCycle, IEnumerable<string> uniqueCities,
            out string homeCity, out string guestCity, out string workerHomeCity)
        {
            homeCity = string.Empty;
            guestCity = string.Empty;
            workerHomeCity = string.Empty;
            var homeAddress = string.Empty;
            var guestAddress = string.Empty;
            var workerAddress = string.Empty;

            if (gamesAtCycle != null && gamesAtCycle.Count == 1)
            {
                homeAddress = workerHomeAddress;
                guestAddress = currentGame?.Auditorium?.Address;
            }
            else if (gamesAtCycle != null && gamesAtCycle.Count > 1)
            {
                if (gamesAtCycle.IsFirst(currentGame))
                {
                    homeAddress = workerHomeAddress;
                    guestAddress = currentGame?.Auditorium?.Address;
                }
                else if (!gamesAtCycle.IsFirst(currentGame) && !gamesAtCycle.IsLast(currentGame))
                {
                    homeAddress = currentGame?.Auditorium?.Address;
                    var cityIndex = gamesAtCycle.IndexOf(currentGame);
                    guestAddress = gamesAtCycle[cityIndex - 1]?.Auditorium?.Address;
                }
                else if (gamesAtCycle.IsLast(currentGame))
                {
                    var cityIndex = gamesAtCycle.IndexOf(currentGame);
                    homeAddress = gamesAtCycle[cityIndex - 1]?.Auditorium?.Address;
                    guestAddress = currentGame?.Auditorium?.Address;
                    workerAddress = workerHomeAddress;
                }
            }

            foreach (var uniqueCity in uniqueCities)
            {
                if (homeAddress != null && homeAddress.IndexOf(uniqueCity) != -1)
                {
                    homeCity = uniqueCity;
                }
                if (guestAddress != null && guestAddress.IndexOf(uniqueCity) != -1)
                {
                    guestCity = uniqueCity;
                }
                if (workerAddress != null && workerAddress.IndexOf(uniqueCity) != -1)
                {
                    workerHomeCity = uniqueCity;
                }
            }
        }


        private int? GetDistanceByCities(string homeCity, string guestCity, IEnumerable<DistanceTable> distanceTableInstance, string workerHomeCity = null)
        {
            if (string.IsNullOrEmpty(workerHomeCity))
            {
                return distanceTableInstance.FirstOrDefault(dt => dt.CityFromName == homeCity && dt.CityToName == guestCity)?.Distance
                       ?? distanceTableInstance.FirstOrDefault(dt => dt.CityFromName == guestCity && dt.CityToName == homeCity)?.Distance;
            }
            else
            {
                var distanceArena1Arena2 = distanceTableInstance.SingleOrDefault(dt => dt.CityFromName == homeCity && dt.CityToName == guestCity)?.Distance
                       ?? distanceTableInstance.FirstOrDefault(dt => dt.CityFromName == guestCity && dt.CityToName == homeCity)?.Distance ?? 0;
                var distanceArena2Home = distanceTableInstance.SingleOrDefault(dt => dt.CityFromName == guestCity && dt.CityToName == workerHomeCity)?.Distance
                       ?? distanceTableInstance.FirstOrDefault(dt => dt.CityFromName == workerHomeCity && dt.CityToName == guestCity)?.Distance ?? 0;
                return distanceArena1Arena2 + distanceArena2Home;
            }
        }

        private IEnumerable<string> GetUniqueCities(IEnumerable<DistanceTable> instanceCities)
        {
            var allCities = new List<string>();

            foreach (var city in instanceCities)
            {
                allCities.Add(city.CityFromName);
                allCities.Add(city.CityToName);
            }

            return allCities.Distinct();
        }

        private decimal? GetLeaguePaymentPerKm(League league, string officialType)
        {
            var leagueFee = league?.LeagueOfficialsSettings?.FirstOrDefault(ls => ls.JobsRole?.RoleName == officialType);
            var leagueTravelMetricType = leagueFee?.TravelMetricType;
            switch (leagueTravelMetricType)
            {
                case 0: //Kilometers
                        //TODO: Convert in one type of valute
                    return leagueFee?.PaymentTravel;
                case 1: //Miles
                        //TODO: Convert in one type of valute and miles into km
                    return leagueFee?.PaymentTravel;
                default:
                    throw new NotImplementedException();
            }
        }

        public void DeleteCycle(int stageId, int cycleNum)
        {
            var stage = db.Stages.Find(stageId);
            var currentCycles = stage.GamesCycles.Where(g => g.CycleNum == cycleNum).ToList();
            foreach (var gameC in currentCycles)
            {
                RemoveCycle(gameC);
            }

            db.SaveChanges();
        }

        public void UpdateGameStatistics()
        {
            var statistics = db.Statistics.Where(s => !s.IsProcessed);
            if (statistics.Any())
            {
                var gamesIds = statistics.Select(c => c.GameId).Distinct();
                var playerIds = statistics.Select(c => c.PlayerId).Distinct();
                if (gamesIds.Any())
                {
                    foreach (var gameId in gamesIds)
                    {
                        foreach (var playerId in playerIds)
                        {
                            var gameStatistics = db.Statistics.Where(c => c.GameId == gameId && c.PlayerId == playerId);
                            if (gameStatistics.Any())
                            {
                                var teamId = db.Statistics.Where(c => c.GameId == gameId && c.PlayerId == playerId).FirstOrDefault().TeamId;
                                var team = db.Teams.Where(t => t.TeamId == teamId).FirstOrDefault();
                                var secRepo = new SectionsRepo();
                                var section = secRepo.GetSectionByTeamId(teamId);
                                var isBasketBall = section?.Alias == GamesAlias.BasketBall;
                                var isWaterPolo = section?.Alias == GamesAlias.WaterPolo;

                                if (isBasketBall)
                                {
                                    var stat = db.GameStatistics.FirstOrDefault(c => c.GameId == gameId && c.PlayerId == playerId)
                                        ?? new GameStatistic { PlayerId = playerId, GameId = gameId };

                                    UpdateGameStatistic(stat, gameStatistics.ToList(), gameId, playerId);
                                    db.GameStatistics.AddOrUpdate(stat);
                                }
                                else if (isWaterPolo)
                                {
                                    var stat = db.WaterpoloStatistics.FirstOrDefault(c => c.GameId == gameId && c.PlayerId == playerId)
                                        ?? new WaterpoloStatistic { PlayerId = playerId, GameId = gameId };

                                    UpdateWaterpoloStatistic(stat, gameStatistics.ToList(), gameId, playerId);
                                    db.WaterpoloStatistics.AddOrUpdate(stat);
                                }
                            }
                        }
                    }
                    foreach (var stat in statistics)
                    {
                        stat.IsProcessed = true;
                    }

                    db.SaveChanges();
                }
            }
        }

        private void UpdateWaterpoloStatistic(WaterpoloStatistic wStat, List<Statistic> gamesStatistics, int gameId, int playerId)
        {
            var game = db.GamesCycles.Find(gameId);
            var oppoTeamInfo = db.WaterpoloStatistics.Where(x => x.TeamId == game.GuestTeamId && x.GameId == game.CycleId);
            var oppoGoal = oppoTeamInfo.Sum(x => x.GOAL) ?? 0;
            var oppoMiss = oppoTeamInfo.Sum(x => x.Miss) ?? 0;

            var user = db.TeamsPlayers.FirstOrDefault(c => c.Id == playerId);

            int gameTime = (int)db.Statistics.Where(c => c.GameId == gameId).First().GameTime;
            int lastTime = (int)db.Statistics.Where(c => c.TeamId == game.HomeTeamId && c.GameId == gameId).First().GameTime;
            wStat.MinutesPlayed = CountPlayersMinutes(gamesStatistics, gameTime, lastTime);

            wStat.TeamId = gamesStatistics.FirstOrDefault()?.TeamId;

            wStat.GOAL = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.NGoal, StringComparison.OrdinalIgnoreCase))
                + gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Goal5m, StringComparison.OrdinalIgnoreCase))
                + gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.GoalCA, StringComparison.OrdinalIgnoreCase))
                + gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.GoalCF, StringComparison.OrdinalIgnoreCase))
                + gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.GoalD, StringComparison.OrdinalIgnoreCase));

            wStat.PGOAL = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.PGoal, StringComparison.OrdinalIgnoreCase));

            wStat.Miss = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Miss, StringComparison.OrdinalIgnoreCase));

            wStat.PMISS = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.PMiss, StringComparison.OrdinalIgnoreCase));

            wStat.AST = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Assist, StringComparison.OrdinalIgnoreCase));
            
            wStat.STL = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Steal, StringComparison.OrdinalIgnoreCase));
            
            wStat.BLK = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Block, StringComparison.OrdinalIgnoreCase));
            
            wStat.TO = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.TurnOver, StringComparison.OrdinalIgnoreCase));
            
            wStat.OFFS = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Offs, StringComparison.OrdinalIgnoreCase));
            
            wStat.EXC = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Exc, StringComparison.OrdinalIgnoreCase));
            
            wStat.BFOUL = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.BFoul, StringComparison.OrdinalIgnoreCase));

            wStat.FOUL = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Foul, StringComparison.OrdinalIgnoreCase))
                + wStat.EXC + wStat.BFOUL;

            wStat.SSAVE = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.SSave, StringComparison.OrdinalIgnoreCase));
            
            wStat.YC = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.YC, StringComparison.OrdinalIgnoreCase));
            
            wStat.RD = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.RD, StringComparison.OrdinalIgnoreCase));

            wStat.EFF = wStat.GOAL + wStat.PGOAL + wStat.AST + wStat.BLK + wStat.STL - (wStat.Miss + wStat.PMISS + wStat.TO);

            wStat.DIFF = CalculateDifferenceForWPlayer(gameId, playerId);
        }

        private void UpdateGameStatistic(GameStatistic gameStat, List<Statistic> gamesStatistics, int gameId, int playerId)
        {
            var game = db.GamesCycles.Find(gameId);
            var user = db.TeamsPlayers.FirstOrDefault(c => c.Id == playerId);

            int gameTime = (int)db.Statistics.Where(c => c.GameId == gameId).First().GameTime;
            int lastTime = (int)db.Statistics.Where(c => c.TeamId == game.HomeTeamId && c.GameId == gameId).First().GameTime;
            gameStat.MinutesPlayed = CountPlayersMinutes(gamesStatistics, gameTime, lastTime);

            gameStat.TeamId = gamesStatistics.FirstOrDefault()?.TeamId;

            gameStat.FG = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Made2Points, StringComparison.OrdinalIgnoreCase)
                                              || string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Made3Points, StringComparison.OrdinalIgnoreCase));

            gameStat.FGA = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Miss2Points, StringComparison.OrdinalIgnoreCase)
                                          || string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Miss3Points, StringComparison.OrdinalIgnoreCase)) + gameStat.FG;

            gameStat.ThreePT = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Made3Points, StringComparison.OrdinalIgnoreCase));

            gameStat.ThreePA = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Made3Points, StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Miss3Points, StringComparison.OrdinalIgnoreCase));

            gameStat.TwoPT = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Made2Points, StringComparison.OrdinalIgnoreCase));

            gameStat.TwoPA = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Made2Points, StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Miss2Points, StringComparison.OrdinalIgnoreCase));

            gameStat.FT = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.MadeFreeThrow, StringComparison.OrdinalIgnoreCase));

            gameStat.FTA = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.MadeFreeThrow, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.MissFreeThrow, StringComparison.OrdinalIgnoreCase));

            gameStat.OREB = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.OffensiveRebound, StringComparison.OrdinalIgnoreCase));

            gameStat.DREB = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.DefensiveRebound, StringComparison.OrdinalIgnoreCase));

            gameStat.REB = gameStat.OREB + gameStat.DREB;

            gameStat.AST = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Assist, StringComparison.OrdinalIgnoreCase));

            gameStat.TO = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.TurnOver, StringComparison.OrdinalIgnoreCase));

            gameStat.STL = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Steal, StringComparison.OrdinalIgnoreCase));

            gameStat.BLK = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Block, StringComparison.OrdinalIgnoreCase));

            gameStat.PF = gamesStatistics.Count(gs => string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.PersonalFoul, StringComparison.OrdinalIgnoreCase)
                || string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.OFoul, StringComparison.OrdinalIgnoreCase)
                || string.Equals(gs.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Tecf, StringComparison.OrdinalIgnoreCase));

            gameStat.PTS = gameStat.FT + (gameStat.TwoPT * 2) + (gameStat.ThreePT * 3);

            gameStat.EFF = (gameStat.PTS + gameStat.REB + gameStat.AST + gameStat.STL + gameStat.BLK - ((gameStat.TwoPA - gameStat.TwoPT) + (gameStat.FTA - gameStat.FT) + gameStat.TO));

            gameStat.DIFF = CalculateDifferenceForPlayer(gameId, playerId);

        }

        private double? CalculateDifferenceForWPlayer(int gameId, int playerId)
        {
            var game = db.GamesCycles.FirstOrDefault(gameCycle => gameCycle.CycleId == gameId);
            if (game != null)
            {
                if (game.Statistics.Any())
                {
                    var playerTeamId = db.TeamsPlayers.FirstOrDefault(teamPlayer => teamPlayer.Id == playerId)?.TeamId;
                    if (playerTeamId.HasValue)
                    {
                        var playersTeamsStats = game.Statistics.Where(gameStat => gameStat.TeamId == playerTeamId);
                        var opponentTeamsStats = game.Statistics.Where(gameStat => gameStat.TeamId != playerTeamId);
                        var intervals = GetAllWGamesIntervals(playersTeamsStats, playerId);
                        double homePoints = CalculateWTeamPoints(playersTeamsStats, playerId, intervals);
                        double guestPoints = CalculateWTeamPoints(opponentTeamsStats, playerId, intervals);
                        return homePoints - guestPoints;
                    }
                    else
                    {
                        return 0D;
                    }
                }
                else
                {
                    return 0D;
                }
            }
            else
            {
                return 0D;
            }
        }

        private double CalculateWTeamPoints(IEnumerable<Statistic> playersTeamsStats, int playerId, IEnumerable<TimeOnField> intervals)
        {
            int points = 0;
            if (intervals.Any())
            {
                foreach (var interval in intervals)
                {
                    var intervalStats = playersTeamsStats.Where(s => s.Timestamp >= interval.PlayerOnFieldTimeStamp && s.Timestamp <= interval.PlayerOffFieldTimeStamp);

                    var goal5m = intervalStats
                        .Count(s => string.Equals(s.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Goal5m.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));
                    var goalca = intervalStats
                        .Count(s => string.Equals(s.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.GoalCA.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));
                    var goalcf = intervalStats
                        .Count(s => string.Equals(s.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.GoalCF.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));
                    var goald = intervalStats
                        .Count(s => string.Equals(s.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.GoalD.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));
                    var pgoal = intervalStats
                        .Count(s => string.Equals(s.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.PGoal.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));
                    var ngoal = intervalStats
                        .Count(s => string.Equals(s.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.NGoal.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));

                    points += goal5m + goalca + goalcf + goald + pgoal + ngoal;
                }
            }
            return Convert.ToDouble(points);
        }

        private double? CalculateDifferenceForPlayer(int gameId, int playerId)
        {
            var game = db.GamesCycles.FirstOrDefault(gameCycle => gameCycle.CycleId == gameId);
            if (game != null)
            {
                if (game.Statistics.Any())
                {
                    var playerTeamId = db.TeamsPlayers.FirstOrDefault(teamPlayer => teamPlayer.Id == playerId)?.TeamId;
                    if (playerTeamId.HasValue)
                    {
                        var playersTeamsStats = game.Statistics.Where(gameStat => gameStat.TeamId == playerTeamId);
                        var opponentTeamsStats = game.Statistics.Where(gameStat => gameStat.TeamId != playerTeamId);
                        var intervals = GetAllGamesIntervals(playersTeamsStats, playerId);
                        double homePoints = CalculateTeamPoints(playersTeamsStats, playerId, intervals);
                        double guestPoints = CalculateTeamPoints(opponentTeamsStats, playerId, intervals);
                        return homePoints - guestPoints;
                    }
                    else
                    {
                        return 0D;
                    }
                }
                else
                {
                    return 0D;
                }
            }
            else
            {
                return 0D;
            }
        }

        private double CalculateTeamPoints(IEnumerable<Statistic> playersTeamsStats, int playerId, IEnumerable<TimeOnField> intervals)
        {
            int points = 0;
            if (intervals.Any())
            {
                foreach (var interval in intervals)
                {
                    var intervalStats = playersTeamsStats.Where(s => s.Timestamp >= interval.PlayerOnFieldTimeStamp && s.Timestamp <= interval.PlayerOffFieldTimeStamp);

                    var madeOnePoints = intervalStats
                        .Count(s => string.Equals(s.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.MadeFreeThrow.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));
                    var madeTwoPoints = intervalStats
                        .Count(s => string.Equals(s.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Made2Points.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)) * 2;
                    var madeThreePoints = intervalStats
                        .Count(s => string.Equals(s.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.Made3Points.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)) * 3;

                    points += madeOnePoints + madeTwoPoints + madeThreePoints;
                }
            }
            return Convert.ToDouble(points);
        }



        public List<TennisPlayoffRanksGroup> UpdateTennisPlayoffRanksGroup(int categoryId, int leagueId, int seasonId, List<LevelPointsSetting> listLevels)
        {
            var cond = new GameFilterConditions
            {
                seasonId = seasonId,
                auditoriums = new List<AuditoriumShort>(),
                leagueId = leagueId
            };
            cond.dateFrom = null;
            cond.dateTo = null;

            var ranksToRemove = db.TennisCategoryPlayoffRanks.Where(t => t.LeagueId == leagueId && t.SeasonId == seasonId && t.CategoryId == categoryId).ToList();
            var tennisStages = db.TennisStages.Where(ts => ts.CategoryId == categoryId).Select(ts => new TennisStageShort { StageId = ts.StageId, Number = ts.Number, CategoryId = categoryId }).ToList();
            var tennisStageIds = tennisStages.Select(ts => ts.StageId).ToList();
            var games = GetTennisCyclesByFilterConditions(cond, true, false).Where(x => x.CategoryId == categoryId).ToList();
            foreach (var tennisGameCycle in games)
            {
                tennisGameCycle.GameTypeId = tennisGameCycle.TennisGroup.TypeId;
                tennisGameCycle.CategoryId = tennisStageIds.Contains(tennisGameCycle.StageId) ? categoryId : 0;
            }
            var groups = games.GroupBy(gc => gc.GameTypeId == GameTypeId.Division ? "Division" : gc.TennisGroup.Name)
                        .Select(g => new TennisScheduleGroup
                        {
                            GroupName = g.Key,
                            GameTypeId = g.First().GameTypeId,
                            IsIndividual = g.First().TennisGroup.IsIndividual,
                            Stages = g.GroupBy(st => st.TennisStage.StageId).Select(st => new TennisScheduleStage
                            {
                                StageId = st.Key,
                                StageNumber = st.First().TennisStage.Number,
                                Items = st.ToList()
                            }).OrderBy(s => s.StageNumber).ToList(),
                            BracketsCount = g.GroupBy(st => st.TennisStage.StageId).Select(st => new TennisScheduleStage
                            {
                                StageId = st.Key,
                                StageNumber = st.First().TennisStage.Number,
                                Items = st.ToList()
                            }).OrderBy(s => s.StageNumber).FirstOrDefault()?.Items?.Count ?? 0,
                            Rounds = g.First().TennisGroup.NumberOfCycles
                        }).ToList();
            groups = groups.Where(t => t.GroupName != "Division").ToList();
            List<TennisPlayoffRanksGroup> playerRanksList = new List<TennisPlayoffRanksGroup>();
            foreach (var group in groups)
            {
                List<TennisPlayoffRank> playerRanks = new List<TennisPlayoffRank>();
                var stageCount = group.Stages.Count();
                var stageIndex = 1;
                bool isAllGamesEnded = true;

                foreach (var stage in group.Stages)
                {
                    foreach (var gameCycle in stage.Items)
                    {
                        var isGameNotEnded = false;
                        if (gameCycle.TennisPlayoffBracket.WinnerId == null && gameCycle.TennisPlayoffBracket.WinnerPlayerPairId == null)
                        {
                            isGameNotEnded = true;
                            //continue;
                            //goto End;
                        }

                        int playoffBracketType = gameCycle.TennisPlayoffBracket.Type;
                        int gameType = gameCycle.TennisGroup.TypeId;
                        int winnerId = gameCycle.TennisPlayoffBracket.WinnerPlayerPairId.HasValue && gameCycle.TennisPlayoffBracket.WinnerPlayerPairId.Value != 0 ? gameCycle.TennisPlayoffBracket.WinnerPlayerPairId.Value : gameCycle.TennisPlayoffBracket.WinnerId.HasValue && gameCycle.TennisPlayoffBracket.WinnerId.Value != 0 ? gameCycle.TennisPlayoffBracket.WinnerId.Value : 0;
                        bool isFinishedRank = false;
                        int gamesInThisColumn = stage.Items.Where(g => g.MaxPlayoffPos == gameCycle.MaxPlayoffPos && g.MinPlayoffPos == gameCycle.MinPlayoffPos && g.TennisPlayoffBracket.Stage == gameCycle.TennisPlayoffBracket.Stage).Count();

                        var player1 = playerRanks.FirstOrDefault(p => (gameCycle.FirstPlayerId.HasValue && p.PlayerId == gameCycle.FirstPlayerId) || (gameCycle.FirstPlayerPairId.HasValue && p.PairPlayerId == gameCycle.FirstPlayerPairId));
                        var player2 = playerRanks.FirstOrDefault(p => (gameCycle.SecondPlayerId.HasValue && p.PlayerId == gameCycle.SecondPlayerId) || (gameCycle.SecondPlayerPairId.HasValue && p.PairPlayerId == gameCycle.SecondPlayerPairId));
                        if (!gameCycle.FirstPlayerId.HasValue && !gameCycle.FirstPlayerPairId.HasValue && !gameCycle.SecondPlayerId.HasValue && !gameCycle.SecondPlayerPairId.HasValue)
                        {
                            continue;
                        }
                        if (player1 == null)
                        {
                            var player = new TennisPlayoffRank
                            {
                                PlayerId = gameCycle.FirstPlayerId,
                                PairPlayerId = gameCycle.FirstPlayerPairId,
                                PlayerName = gameCycle.FirstPlayerPairId.HasValue && gameCycle.FirstPlayerPairId.Value != 0 ? gameCycle.FirstPairPlayer?.User?.FullName : gameCycle.FirstPlayer?.User?.FullName ?? "",
                                Rank = gameCycle.TennisPlayoffBracket.MinPos,
                                RealMaxPos = gameCycle.TennisPlayoffBracket.MaxPos,
                                RealMinPos = gameCycle.TennisPlayoffBracket.MinPos,
                                PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {(gameCycle.TennisPlayoffBracket.MinPos / 2) + 1}"
                            };
                            playerRanks.Add(player);
                        }
                        else
                        {
                            //if (player1.Rank >= gameCycle.TennisPlayoffBracket.MinPos)
                            //{
                            player1.Rank = gameCycle.TennisPlayoffBracket.MinPos;
                            if ((gameCycle.TennisPlayoffBracket.MaxPos == 1 || gameCycle.TennisPlayoffBracket.MaxPos == 3) && (gameType != GameTypeId.Knockout34ConsolencesQuarterRound || gamesInThisColumn < 4))
                            {
                                if (gameType == GameTypeId.Knockout34ConsolencesQuarterRound && gameCycle.TennisPlayoffBracket.MinPos > 4)
                                {
                                    player1.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {(gameCycle.TennisPlayoffBracket.MinPos / 2) + 1}";
                                    player1.RealMaxPos = 9;
                                    player1.RealMinPos = gameCycle.TennisPlayoffBracket.MinPos - gameCycle.TennisPlayoffBracket.MinPos / 4;
                                }
                                else
                                {
                                    player1.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {(gameCycle.TennisPlayoffBracket.MinPos / 2) + 1}";
                                    player1.RealMaxPos = (gameCycle.TennisPlayoffBracket.MinPos / 2) + 1;
                                    player1.RealMinPos = gameCycle.TennisPlayoffBracket.MinPos;
                                }
                            }
                            else
                            {
                                if (gameType == GameTypeId.Knockout34Consolences1Round)
                                {
                                    player1.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                    if (winnerId == 0 || (winnerId != player1.PlayerId && winnerId != player1.PairPlayerId))
                                    {
                                        int realMax = ((gameCycle.TennisPlayoffBracket.MaxPos + gameCycle.TennisPlayoffBracket.MinPos) / 2) + 1;
                                        player1.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {realMax}";
                                        player1.RealMaxPos = realMax;
                                        player1.RealMinPos = gameCycle.TennisPlayoffBracket.MinPos;
                                        isFinishedRank = true;
                                    }
                                }
                                else if (gameType == GameTypeId.Knockout34ConsolencesQuarterRound)
                                {
                                    player1.RealMaxPos = 5;
                                    if (gameCycle.TennisPlayoffBracket.MinPos - gameCycle.TennisPlayoffBracket.MaxPos < 4)
                                    {
                                        if (gameCycle.TennisPlayoffBracket.MinPos - gameCycle.TennisPlayoffBracket.MaxPos < 2)
                                        {
                                            if (winnerId != 0 && (winnerId != player1.PlayerId && winnerId != player1.PairPlayerId))
                                            {
                                                player1.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                                player1.RealMaxPos = 6;
                                                player1.RealMinPos = 6;
                                                isFinishedRank = true;
                                            }
                                            else
                                            {
                                                player1.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                                player1.RealMaxPos = 5;
                                                player1.RealMinPos = 5;
                                                isFinishedRank = true;
                                            }
                                        }
                                        else
                                        {
                                            if (winnerId != 0 && (winnerId != player1.PlayerId && winnerId != player1.PairPlayerId))
                                            {
                                                player1.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                                player1.RealMaxPos = 7;
                                                player1.RealMinPos = 8;
                                                isFinishedRank = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (winnerId != 0 && (winnerId != player1.PlayerId && winnerId != player1.PairPlayerId))
                                        {
                                            player1.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                            player1.RealMaxPos = player1.RealMinPos - gamesInThisColumn + 1;
                                            isFinishedRank = true;
                                        }
                                        else
                                        {
                                            player1.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                            player1.RealMaxPos = 5;
                                            player1.RealMinPos = player1.RealMinPos - gamesInThisColumn;
                                            isFinishedRank = true;
                                        }
                                    }
                                }
                                else
                                {
                                    player1.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                    player1.RealMaxPos = gameCycle.TennisPlayoffBracket.MaxPos;
                                    player1.RealMinPos = gameCycle.TennisPlayoffBracket.MinPos;
                                }
                            }

                            if (gameCycle.TennisPlayoffBracket.MinPos - gameCycle.TennisPlayoffBracket.MaxPos == 1 && !isFinishedRank)
                            {
                                if ((gameCycle.FirstPlayerId.HasValue && player1.PlayerId == winnerId) || (gameCycle.FirstPlayerPairId.HasValue && player1.PairPlayerId == winnerId))
                                {
                                    player1.Rank = gameCycle.TennisPlayoffBracket.MaxPos;
                                    player1.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MaxPos}";
                                    player1.RealMaxPos = gameCycle.TennisPlayoffBracket.MaxPos;
                                    player1.RealMinPos = gameCycle.TennisPlayoffBracket.MaxPos;
                                }
                                else //if (winnerId != 0)
                                {
                                    player1.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos}";
                                    player1.RealMaxPos = gameCycle.TennisPlayoffBracket.MinPos;
                                    player1.RealMinPos = gameCycle.TennisPlayoffBracket.MinPos;
                                }
                            }
                        }
                        isFinishedRank = false;
                        if (player2 == null)
                        {
                            var player = new TennisPlayoffRank
                            {
                                PlayerId = gameCycle.SecondPlayerId,
                                PairPlayerId = gameCycle.SecondPlayerPairId,
                                PlayerName = gameCycle.SecondPlayerPairId.HasValue && gameCycle.SecondPlayerPairId.Value != 0 ? gameCycle.SecondPairPlayer?.User?.FullName : gameCycle.SecondPlayer?.User?.FullName ?? "",
                                Rank = gameCycle.TennisPlayoffBracket.MinPos,
                                RealMaxPos = gameCycle.TennisPlayoffBracket.MaxPos,
                                RealMinPos = gameCycle.TennisPlayoffBracket.MinPos,
                                PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {(gameCycle.TennisPlayoffBracket.MinPos / 2) + 1}"
                            };
                            playerRanks.Add(player);
                        }
                        else
                        {
                            //if (player2.Rank >= gameCycle.TennisPlayoffBracket.MinPos)
                            //{
                            player2.Rank = gameCycle.TennisPlayoffBracket.MinPos;
                            if ((gameCycle.TennisPlayoffBracket.MaxPos == 1 || gameCycle.TennisPlayoffBracket.MaxPos == 3) && (gameType != GameTypeId.Knockout34ConsolencesQuarterRound || gamesInThisColumn < 4))
                            {
                                if (gameType == GameTypeId.Knockout34ConsolencesQuarterRound && gameCycle.TennisPlayoffBracket.MinPos > 4)
                                {
                                    player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {(gameCycle.TennisPlayoffBracket.MinPos / 2) + 1}";
                                    player2.RealMaxPos = 9;
                                    player2.RealMinPos = gameCycle.TennisPlayoffBracket.MinPos - gameCycle.TennisPlayoffBracket.MinPos / 4;
                                }
                                else
                                {
                                    player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {(gameCycle.TennisPlayoffBracket.MinPos / 2) + 1}";
                                    player2.RealMaxPos = (gameCycle.TennisPlayoffBracket.MinPos / 2) + 1;
                                    player2.RealMinPos = gameCycle.TennisPlayoffBracket.MinPos;
                                }
                            }
                            else
                            {
                                if (gameType == GameTypeId.Knockout34Consolences1Round)
                                {
                                    player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                    if (winnerId == 0 || (winnerId != player2.PlayerId && winnerId != player2.PairPlayerId))
                                    {
                                        int realMax = ((gameCycle.TennisPlayoffBracket.MaxPos + gameCycle.TennisPlayoffBracket.MinPos) / 2) + 1;
                                        player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {realMax}";
                                        player2.RealMaxPos = realMax;
                                        player2.RealMinPos = gameCycle.TennisPlayoffBracket.MinPos;
                                    }
                                }
                                else if (gameType == GameTypeId.Knockout34ConsolencesQuarterRound)
                                {
                                    player2.RealMaxPos = 5;
                                    if (gameCycle.TennisPlayoffBracket.MinPos - gameCycle.TennisPlayoffBracket.MaxPos < 4)
                                    {
                                        if (gameCycle.TennisPlayoffBracket.MinPos - gameCycle.TennisPlayoffBracket.MaxPos < 2)
                                        {
                                            if (winnerId != 0 && (winnerId != player2.PlayerId && winnerId != player2.PairPlayerId))
                                            {
                                                player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                                player2.RealMaxPos = 6;
                                                player2.RealMinPos = 6;
                                                isFinishedRank = true;
                                            }
                                            else
                                            {
                                                player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                                player2.RealMaxPos = 5;
                                                player2.RealMinPos = 5;
                                                isFinishedRank = true;
                                            }
                                        }
                                        else
                                        {
                                            if (winnerId != 0 && (winnerId != player2.PlayerId && winnerId != player2.PairPlayerId))
                                            {
                                                player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                                player2.RealMaxPos = 7;
                                                player2.RealMinPos = 8;
                                                isFinishedRank = true;
                                            }
                                        }
                                    }
                                    else
                                    {

                                        if (winnerId != 0 && (winnerId != player2.PlayerId && winnerId != player2.PairPlayerId))
                                        {
                                            if (gameCycle.TennisPlayoffBracket.MaxPos == 1)
                                            {
                                                player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                                player2.RealMaxPos = 5;
                                                player2.RealMinPos = player2.RealMinPos - gamesInThisColumn;
                                                isFinishedRank = true;
                                            }
                                            else
                                            {
                                                player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                                player2.RealMaxPos = player2.RealMinPos - gamesInThisColumn + 1;
                                                isFinishedRank = true;
                                            }
                                        }
                                        else
                                        {
                                            player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                            player2.RealMaxPos = 5;
                                            player2.RealMinPos = player2.RealMinPos - gamesInThisColumn;
                                            isFinishedRank = true;
                                        }
                                    }
                                }
                                else
                                {
                                    player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos} - {gameCycle.TennisPlayoffBracket.MaxPos}";
                                    player2.RealMaxPos = gameCycle.TennisPlayoffBracket.MaxPos;
                                    player2.RealMinPos = gameCycle.TennisPlayoffBracket.MinPos;
                                }
                            }
                            //}
                            if (gameCycle.TennisPlayoffBracket.MinPos - gameCycle.TennisPlayoffBracket.MaxPos == 1 && !isFinishedRank)
                            {
                                if ((gameCycle.SecondPlayerId.HasValue && player2.PlayerId == winnerId) || (gameCycle.SecondPlayerPairId.HasValue && player2.PairPlayerId == winnerId))
                                {
                                    player2.Rank = gameCycle.TennisPlayoffBracket.MaxPos;
                                    player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MaxPos}";
                                    player2.RealMaxPos = gameCycle.TennisPlayoffBracket.MaxPos;
                                    player2.RealMinPos = gameCycle.TennisPlayoffBracket.MaxPos;
                                }
                                else //if (winnerId != 0)
                                {
                                    player2.PlayerLastRank = $"{gameCycle.TennisPlayoffBracket.MinPos}";
                                    player2.RealMaxPos = gameCycle.TennisPlayoffBracket.MinPos;
                                    player2.RealMinPos = gameCycle.TennisPlayoffBracket.MinPos;
                                }
                            }
                        }
                    }
                    stageIndex += 1;
                }
            End:
                if (isAllGamesEnded)
                {
                    playerRanks = playerRanks.OrderBy(t => t.RealMinPos).ThenBy(t => t.RealMaxPos).ToList();
                    var groupList = new TennisPlayoffRanksGroup
                    {
                        PlayersRanks = playerRanks,
                        GroupName = group.GroupName
                    };

                    playerRanksList.Add(groupList);
                }

            }


            foreach (var playerRankGroup in playerRanksList)
            {
                for (int i = 0; i < playerRankGroup.PlayersRanks.Count; i++)
                {
                    var playerRank = playerRankGroup.PlayersRanks.ElementAt(i);
                    var score = listLevels.ElementAtOrDefault(i)?.Points ?? 0;
                    playerRank.Points = score;
                    UpdatePlayerRankScoreInCategory(categoryId, leagueId, seasonId, playerRankGroup.GroupName, playerRank, ranksToRemove);
                }
            }

            db.TennisCategoryPlayoffRanks.RemoveRange(ranksToRemove);

            Save();


            return playerRanksList;
        }



        public void SetManualTennisRankCalculation(int categoryId, int leagueId, int seasonId, List<LevelPointsSetting> listLevels)
        {
            var currentRanks = db.TennisCategoryPlayoffRanks.Where(p => p.LeagueId == leagueId && p.SeasonId == seasonId && p.CategoryId == categoryId).ToList();
            db.TennisCategoryPlayoffRanks.RemoveRange(currentRanks);
            var resList = GetTeamPlayers(categoryId, 0, leagueId, seasonId);

            var ranksToRemove = new List<TennisCategoryPlayoffRank>();
            for (int i = 0; i < resList.Count(); i++)
            {
                var player = resList.ElementAt(i);
                var score = listLevels.ElementAtOrDefault(i)?.Points ?? 0;
                var playerRank = new TennisPlayoffRank
                {
                    PlayerId = player.Id,
                    Rank = i + 1,
                    Points = score,
                    Correction = 0
                };
                UpdatePlayerRankScoreInCategory(categoryId, leagueId, seasonId, string.Empty, playerRank, currentRanks);
            }
            db.TennisCategoryPlayoffRanks.RemoveRange(ranksToRemove);
            Save();
        }



        public void ManualTennisRankSwap(int categoryId, int leagueId, int seasonId, string groupName, int swap1, int swap2)
        {

            var ranks = db.TennisCategoryPlayoffRanks.Where(p => p.LeagueId == leagueId && p.SeasonId == seasonId && p.CategoryId == categoryId && p.GroupName == groupName).ToList();

            var swap1Rank = ranks.ElementAtOrDefault(swap1 - 1);
            var swap2Rank = ranks.ElementAtOrDefault(swap2 - 1);

            if (swap1Rank == null || swap2Rank == null)
                return;
            var tempPlayerId1 = swap1Rank.PlayerId;
            var tempPairPlayerId1 = swap1Rank.PairPlayerId;
            swap1Rank.PlayerId = swap2Rank.PlayerId;
            swap1Rank.PairPlayerId = swap2Rank.PairPlayerId;
            swap2Rank.PlayerId = tempPlayerId1;
            swap2Rank.PairPlayerId = tempPairPlayerId1;
            Save();
        }






        private void UpdatePlayerRankScoreInCategory(int categoryId, int leagueId, int seasonId, string groupName, TennisPlayoffRank rank, List<TennisCategoryPlayoffRank> ranksToRemove)
        {
            var dbrank = db.TennisCategoryPlayoffRanks.FirstOrDefault(p => p.LeagueId == leagueId && p.SeasonId == seasonId && p.CategoryId == categoryId && p.GroupName == groupName && ((rank.PlayerId.HasValue && p.PlayerId == rank.PlayerId) || (rank.PairPlayerId.HasValue && p.PairPlayerId == rank.PairPlayerId)));

            if (!rank.PlayerId.HasValue && !rank.PairPlayerId.HasValue)
            {
                return;
            }
            if (dbrank == null)
            {
                db.TennisCategoryPlayoffRanks.Add(new TennisCategoryPlayoffRank
                {
                    LeagueId = leagueId,
                    SeasonId = seasonId,
                    CategoryId = categoryId,
                    GroupName = groupName,
                    Points = rank.Points,
                    Rank = rank.Rank,
                    PlayerId = rank.PlayerId,
                    PairPlayerId = rank.PairPlayerId,
                    RealMaxPos = rank.RealMaxPos,
                    RealMinPos = rank.RealMinPos,
                    Correction = 0
                });
            }
            else
            {
                ranksToRemove.Remove(dbrank);
                dbrank.Points = rank.Points;
                dbrank.Rank = rank.Rank;
                dbrank.RealMaxPos = rank.RealMaxPos;
                dbrank.RealMinPos = rank.RealMinPos;
            }
        }

        public List<TennisPlayoffRanksGroup> GetTennisPlayoffRanksGroup(int categoryId, int leagueId, int seasonId, bool isFinalPoints = false)
        {
            List<TennisPlayoffRanksGroup> playerRanksList = new List<TennisPlayoffRanksGroup>();
            var ranks = db.TennisCategoryPlayoffRanks.Where(p => p.CategoryId == categoryId && p.LeagueId == leagueId && p.SeasonId == seasonId).ToList().Select(p => new TennisPlayoffRank
            {
                PlayerId = p.PlayerId,
                Points = isFinalPoints ? ((p.Points ?? 0) + (p.Correction ?? 0)) : (p.Points ?? 0),
                Correction = p.Correction,
                PairPlayerId = p.PairPlayerId,
                Rank = p.Rank,
                GroupName = p.GroupName,
                PlayerName = p.PlayerId.HasValue && p.PlayerId > 0 ? p.TeamsPlayer.User.FullName : p.TeamsPlayer1.User.FullName,
                RealMaxPos = p.RealMaxPos,
                RealMinPos = p.RealMinPos
            }).ToList();
            var ranksByGroup = ranks.Count() > 0 ? ranks.GroupBy(p => p.GroupName ?? string.Empty).ToList() : new List<IGrouping<string, TennisPlayoffRank>>();
            if (ranksByGroup.Count() > 0)
            {
                foreach (var group in ranksByGroup)
                {
                    var playerRanks = group.OrderBy(t => t.RealMinPos).ThenBy(t => t.RealMaxPos).AsEnumerable().ToList();
                    if (isFinalPoints)
                    {
                        playerRanks = playerRanks.OrderByDescending(p => p.Points).ToList();
                    }
                    var groupList = new TennisPlayoffRanksGroup
                    {
                        PlayersRanks = playerRanks,
                        GroupName = group.Key
                    };

                    playerRanksList.Add(groupList);
                }
            }
            //var playerRanksList = UpdateTennisPlayoffRanksGroup(categoryId, leagueId, seasonId, listLevels);
            return playerRanksList;
        }


        public Dictionary<string, int> GetTopCompetitionsRanks(List<GymnasticTotoValue> sportsmen)
        {
            var result = new Dictionary<string, int>();
            var compsDone = new List<int>();
            foreach (var sportsman in sportsmen)
            {
                foreach (var competitionId in sportsman.AthleticsCompetitionsIds)
                {
                    if (!compsDone.Contains(competitionId))
                    {
                        var compDisciplines = db.CompetitionDisciplines.Where(cd => !cd.IsDeleted && cd.CompetitionId == competitionId).ToList();
                        foreach (var compDiscipline in compDisciplines)
                        {
                            var usersInRanks = GetAthletesRanksInCompetitionDiscipline(compDiscipline.Id);
                            for (int i = 0; i < usersInRanks.Count; i++)
                            {
                                var userId = usersInRanks[i];
                                if (result.ContainsKey($"{userId}_{competitionId}"))
                                {
                                    if (result[$"{userId}_{competitionId}"] > i + 1)
                                    {
                                        result[$"{userId}_{competitionId}"] = i + 1;
                                    }
                                }
                                else
                                {
                                    result[$"{userId}_{competitionId}"] = i + 1;
                                }
                            }
                        }
                        compsDone.Add(competitionId);
                    }
                }
            }
            return result;
        }

        public void SetTennisCategoryRankCorrection(int playerId, int leagueId, int categoryId, int seasonId, int correction, string groupName)
        {
            var rank = db.TennisCategoryPlayoffRanks.FirstOrDefault(p => p.LeagueId == leagueId && p.SeasonId == seasonId && p.CategoryId == categoryId && p.GroupName == groupName && (p.PlayerId == playerId || p.PairPlayerId == playerId));
            if (rank != null)
            {
                rank.Correction = correction;
            }
            Save();
        }


        private IEnumerable<TimeOnField> GetAllGamesIntervals(IEnumerable<Statistic> playersTeamsStats, int playerId)
        {
            var timeIntervals = playersTeamsStats.Where(s => s.PlayerId == playerId &&
                                (s.Abbreviation.ToLowerInvariant().Contains(StatisticButtonsTypes.OnField.ToLowerInvariant())
                                || s.Abbreviation.ToLowerInvariant().Contains(StatisticButtonsTypes.OffField.ToLowerInvariant())))
                                .OrderBy(s => s.Timestamp).ToList();

            for (int i = 0; i < timeIntervals.Count; i++)
            {
                if (i + 1 < timeIntervals.Count)
                {
                    yield return new TimeOnField
                    {
                        PlayerOnFieldTimeStamp = timeIntervals[i]?.Timestamp,
                        PlayerOffFieldTimeStamp = timeIntervals[i + 1]?.Timestamp
                    };
                }
                else if (String.Equals(timeIntervals[i].Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.OnField, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new TimeOnField
                    {
                        PlayerOnFieldTimeStamp = timeIntervals[i]?.Timestamp,
                        PlayerOffFieldTimeStamp = timeIntervals[i]?.Timestamp.AddMinutes(10)
                    };
                }
            }
        }

        private IEnumerable<TimeOnField> GetAllWGamesIntervals(IEnumerable<Statistic> playersTeamsStats, int playerId)
        {
            var timeIntervals = playersTeamsStats.Where(s => s.PlayerId == playerId &&
                                (s.Abbreviation.ToLowerInvariant().Contains(StatisticButtonsTypes.OnField.ToLowerInvariant())
                                || s.Abbreviation.ToLowerInvariant().Contains(StatisticButtonsTypes.OffField.ToLowerInvariant())))
                                .OrderBy(s => s.Timestamp).ToList();

            for (int i = 0; i < timeIntervals.Count; i++)
            {
                if (i + 1 < timeIntervals.Count)
                {
                    yield return new TimeOnField
                    {
                        PlayerOnFieldTimeStamp = timeIntervals[i]?.Timestamp,
                        PlayerOffFieldTimeStamp = timeIntervals[i + 1]?.Timestamp
                    };
                }
                else if (String.Equals(timeIntervals[i].Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.OnField, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new TimeOnField
                    {
                        PlayerOnFieldTimeStamp = timeIntervals[i]?.Timestamp,
                        PlayerOffFieldTimeStamp = timeIntervals[i]?.Timestamp.AddMinutes(8)
                    };
                }
            }
        }

        private long? CountPlayersMinutes(List<Statistic> gamesStatistics, int gameTime, int lastTime)
        {
            long playedSeconds = 0;
            int i = 0;
            var minStat = gamesStatistics.Where(s => string.Equals(s.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.OnField.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(s.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.OffField.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.Timestamp)
                .ToList();

            var gameQuarters = gamesStatistics.Select(c => c.TimeSegmentName).Distinct();
            playedSeconds += gameTime * (gameQuarters.Count());
            for (i = 0;i < minStat.Count();i++)
            {
                if (string.Equals(minStat.ElementAt(i).Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.OnField.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                {
                    if (i == 0 || minStat.ElementAt(i).TimeSegmentName != minStat.ElementAt(i - 1).TimeSegmentName)
                    {
                        playedSeconds -= (gameTime - minStat.ElementAt(i).GameTime);
                    }
                    else
                    {
                        playedSeconds += minStat.ElementAt(i).GameTime;
                    }
                }
                if (string.Equals(minStat.ElementAt(i).Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.OffField.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                {
                    playedSeconds -= minStat.ElementAt(i).GameTime;
                }
            }

            if (!string.Equals(gamesStatistics.Last().Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.OnField.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(minStat.Last().Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.OffField.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                {
                    playedSeconds += (gameTime - gamesStatistics.Last().GameTime);
                }
                else
                {
                    playedSeconds += (gameTime - lastTime);
                }
            }

                //foreach (var gameStat in minStat)
                //{
                //    if (string.Equals(gameStat.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.OnField.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                //    {
                //        var gameQuarters = gamesStatistics.Select(c => c.TimeSegmentName).Distinct();
                //        playedSeconds += gameStat.GameTime * gameQuarters.Count();
                //    }
                //    if (string.Equals(gameStat.Abbreviation.Replace(" ", string.Empty), StatisticButtonsTypes.OffField.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                //    {
                //        playedSeconds -= gameStat.GameTime;
                //    }
                //}
                //if (minStat.Count % 2 == 1)
                //{
                //    playedSeconds -= gamesStatistics.Where()
                //}
                /*if (minStat.Count > 0)
                {
                    for (int i = 0; i <= minStat.Count / 2; i++)
                    {
                        var start = minStat[i != 0 ? i * 2 - 1 : 0];
                        Statistic end;

                        if (minStat.Count > i * 2 + 1)
                        {
                            end = minStat[i * 2 + 1];
                        }
                        else
                        {
                            end = gamesStatistics.Last();
                        }

                        playedSeconds += start.GameTime - end.GameTime;
                    }
                }*/

                return playedSeconds;
        }
    }

    public class TennisLeagueGameForm
    {
        public int Id { get; set; }
        public CompetitionType CompetionType { get; set; }
        public string CompetitionName { get; set; }
        public DateTime? DateOfGame { get; set; }
        public ResultType ResultType { get; set; }
        public string OpponentName { get; set; }
        public string ResultScore { get; set; }
        public string PartnerName { get; set; }
        public int CompetitionId { get; set; }
        public int CategoryId { get; set; }
    }

    public class TimeOnField
    {
        public DateTime? PlayerOnFieldTimeStamp { get; set; }
        public DateTime? PlayerOffFieldTimeStamp { get; set; }
    }
}

