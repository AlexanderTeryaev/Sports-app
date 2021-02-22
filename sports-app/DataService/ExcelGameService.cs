using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;
using DataService.DTO;

namespace DataService
{
    public class ExcelGameService
    {
        private UsersRepo _usersRepo;
        private GamesRepo _gamesRepo;
        private LeagueRepo _leagueRepo;

        protected UsersRepo usersRepo
        {
            get
            {
                if (_usersRepo == null)
                {
                    _usersRepo = new UsersRepo();
                }
                return _usersRepo;
            }
        }
        protected GamesRepo gamesRepo
        {
            get
            {
                if (_gamesRepo == null)
                {
                    _gamesRepo = new GamesRepo();
                }
                return _gamesRepo;
            }
        }
        protected LeagueRepo leagueRepo
        {
            get
            {
                if (_leagueRepo == null)
                {
                    _leagueRepo = new LeagueRepo();
                }
                return _leagueRepo;
            }
        }

        protected IEnumerable<ExcelGameDto> MapResults(IEnumerable<GamesCycle> gamesList, int? seasonId = null)
        {
            var dtos = new List<ExcelGameDto>();
            foreach (var gc in gamesList)
            {
                var dto = new ExcelGameDto
                {
                    League = gc.Stage.League.Name,
                    LeagueId = gc.Stage.LeagueId,
                    Stage = gc.Stage.Number,
                    Groupe = gc.Group != null ? gc.Group.Name : "",
                    CycleNumber = gc.CycleNum + 1,
                    Date = gc.StartDate,
                    HomeTeamId = gc.HomeTeam?.TeamId ?? 0,
                    GuestTeamId = gc.GuestTeam?.TeamId ?? 0,
                    HomeCompetitorId = gc.TeamsPlayer1?.Id ?? 0,
                    GuestCompetitorId = gc.TeamsPlayer1 != null ? gc.TeamsPlayer.Id : 0,
                    IsIndividual = gc.Group.IsIndividual,
                    GuestTeamTechnicalWinner = gc.TechnicalWinnnerId == gc.GuestTeamId,
                    HomeTeamTechnicalWinner = gc.TechnicalWinnnerId == gc.HomeTeamId,
                    TennisLeagueGames = gc.TennisLeagueGames?.ToList(),
                    RoundNum = gc.RoundNum?.ToString(),
                    CycleNum = gc.CycleNum,
                    GroupId = gc.GroupId.Value
                };

                var section = gc.Stage.League?.Season?.Union?.Section?.Alias ?? gc.Stage.League?.Season?.Club?.Section?.Alias;
                switch (section)
                {
                    case GamesAlias.WaterPolo:
                    case GamesAlias.Soccer:
                    case GamesAlias.Rugby:
                    case GamesAlias.BasketBall:
                        dto.HomeTeamScore = gc.GameSets.Sum(x => x.HomeTeamScore);
                        dto.GuestTeamScore = gc.GameSets.Sum(x => x.GuestTeamScore);
                        break;
                    default:
                        dto.HomeTeamScore = gc.HomeTeamScore;
                        dto.GuestTeamScore = gc.GuestTeamScore;
                        break;
                }

                dto.IsTennisLeagueGame = gc.Stage.League.EilatTournament == null || gc.Stage.League.EilatTournament == false
                    && section.Equals(GamesAlias.Tennis);
                dto.Section = section;
                dto.GameId = gc.CycleId;
                dto.Time = gc.StartDate.ToString("HH:mm");

                if (seasonId.HasValue)
                {
                    var homeTeamDetails = gc.HomeTeam?.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId.Value);
                    var homeAthleteDetails = gc.TeamsPlayer1;
                    if (homeTeamDetails != null)
                    {
                        dto.HomeTeam = homeTeamDetails.TeamName;
                    }
                    else if (homeAthleteDetails != null)
                    {
                        dto.HomeCompetitor = $"{homeAthleteDetails.User.FullName} ({homeAthleteDetails.Team.Title})";
                    }
                    else
                    {
                        dto.HomeTeam = gc.HomeTeam?.Title ?? "";
                        dto.HomeCompetitor = "";
                    }

                    var guesTeamDetails = gc.GuestTeam?.TeamsDetails.FirstOrDefault(t => t.SeasonId == seasonId.Value);
                    var guestAthleteDetails = gc.TeamsPlayer;
                    if (guesTeamDetails != null)
                    {
                        dto.GuestTeam = guesTeamDetails.TeamName;
                    }
                    else if (guestAthleteDetails != null)
                    {
                        dto.GuestCompetitor = $"{guestAthleteDetails.User.FullName} ({guestAthleteDetails.Team.Title})";
                    }
                    else
                    {
                        dto.GuestTeam = gc.GuestTeam?.Title ?? "";
                    }
                }
                else
                {
                    dto.HomeTeam = gc.HomeTeam != null ? gc.HomeTeam.Title : "";
                    dto.GuestTeam = gc.GuestTeam != null ? gc.GuestTeam.Title : "";
                    dto.HomeCompetitor = gc.TeamsPlayer1 != null ? $"{gc.TeamsPlayer1.User.FullName} ({gc.TeamsPlayer1.Team.Title})" : "";
                    dto.GuestCompetitor = gc.TeamsPlayer != null ? $"{gc.TeamsPlayer.User.FullName} ({gc.TeamsPlayer.Team.Title})" : "";
                }

                if (gc.GameSets.Any())
                {
                    var sets = gc.GameSets.ToArray();

                    dto.Set1 = sets.Length > 0 && sets[0].HomeTeamScore >= 0 && sets[0].GuestTeamScore >= 0 ? string.Format("{0} - {1}", sets[0].HomeTeamScore, sets[0].GuestTeamScore) : "";
                    dto.Set2 = sets.Length > 1 && sets[1].HomeTeamScore >= 0 && sets[1].GuestTeamScore >= 0 ? string.Format("{0} - {1}", sets[1].HomeTeamScore, sets[1].GuestTeamScore) : "";
                    dto.Set3 = sets.Length > 2 && sets[2].HomeTeamScore >= 0 && sets[2].GuestTeamScore >= 0 ? string.Format("{0} - {1}", sets[2].HomeTeamScore, sets[2].GuestTeamScore) : "";
                    dto.Set4 = sets.Length > 3 && sets[3].HomeTeamScore >= 0 && sets[3].GuestTeamScore >= 0 ? string.Format("{0} - {1}", sets[3].HomeTeamScore, sets[3].GuestTeamScore) : "";

                }

                if (gc.AuditoriumId.HasValue)
                {
                    dto.Auditorium = gc.Auditorium.Name;
                    dto.AuditoriumId = gc.AuditoriumId.Value;
                }
                if (!string.IsNullOrEmpty(gc.RefereeIds))
                {
                    dto.Referees =
                        usersRepo.GetUserNamesStringByIds(
                            gc.RefereeIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    dto.RefereeIds = gc.RefereeIds;
                }
                if (!string.IsNullOrEmpty(gc.SpectatorIds))
                {
                    dto.Spectators =
                        usersRepo.GetUserNamesStringByIds(
                            gc.SpectatorIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    dto.SpectatorIds = gc.SpectatorIds;
                }

                if (!string.IsNullOrEmpty(gc.DeskIds))
                {
                    dto.DesksNames =
                        usersRepo.GetUserNamesStringByIds(
                            gc.DeskIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    dto.DesksIds = gc.DeskIds;
                }


                dtos.Add(dto);
            }

            return dtos;
        }

        public IEnumerable<ExcelGameDto> GetGameCyclesByIdSet(int[] gameIds)
        {
            var resList = gamesRepo.GetGroupsCyclesByGameIds(gameIds);

            var mappedGames = MapResults(resList);
            return GetGamesByCycles(mappedGames);
        }

        private IEnumerable<ExcelGameDto> GetGamesByCycles(IEnumerable<ExcelGameDto> games)
        {
            var gamesGroupedByLeague = games.GroupBy(r => r.LeagueId);
            var roundCounter = 0;
            foreach (var gamesInLeague in gamesGroupedByLeague)
            {
                var roundStartCycle = gamesRepo.GetGameSettings(gamesInLeague.Key)?.RoundStartCycle;
                foreach (var gamesInRound in games.GroupBy(t => t.RoundNum))
                {
                    var cycleNum = 0;
                    roundCounter++;
                    var differences = new Dictionary<int?, int>();
                    if (roundStartCycle == RoundStartCycle.StartEachRoundFromCycleOne)
                    {
                        var min = gamesInRound.First().CycleNum;
                        foreach (var game in gamesInRound)
                        {
                            if (!differences.ContainsKey(game.GroupId))
                            {
                                differences.Add(game.GroupId, game.CycleNum - min);
                            }

                            game.CycleNum -= differences[game.GroupId];
                        }
                    }
                    foreach (var cycleGames in gamesInRound.GroupBy(t => t.CycleNum))
                    {
                        cycleNum++;
                        foreach (var game in cycleGames)
                        {
                            if (roundStartCycle == RoundStartCycle.StartEachRoundFromCycleOne)
                            {
                                game.CycleNumber = cycleNum;
                            }
                        }
                    }
                }
            }

            return games;
        }

        public IEnumerable<ExcelGameDto> GetLeagueGames(int leagueId, int? seasonId)
        {
            return GetLeagueGames(leagueId, true, null, null, seasonId);
        }
        public IEnumerable<ExcelGameDto> GetLeagueGames(int leagueId, bool userIsEditor, DateTime? dateFrom, DateTime? dateTo, int? seasonId)
        {
            var league = leagueRepo.GetById(leagueId);
            var cond = new GamesRepo.GameFilterConditions
            {
                auditoriums = new List<AuditoriumShort>(),
                leagues = new List<LeagueShort>
                {
                    new LeagueShort
                    {
                        Check = true,
                        Id = leagueId,
                        UnionId = league.UnionId,
                        Name = league.Name
                    }
                },
                dateFrom = dateFrom,
                dateTo = dateTo,
                seasonId = league.SeasonId ?? 0
            };
            var resList = gamesRepo.GetCyclesByFilterConditions(cond, userIsEditor, false);

            var mappedGames = MapResults(resList, seasonId ?? league.SeasonId);
            return GetGamesByCycles(mappedGames);
        }

        public IEnumerable<ExcelGameDto> GetTeamGames(int teamId, int? seasonId)
        {
            var resList = gamesRepo.GetTeamCycles(teamId);
            return MapResults(resList, seasonId);
        }

        public IEnumerable<ExcelGameDto> GetTeamGames(int teamId, int leagueId, int? seasonId)
        {
            var resList = gamesRepo.GetTeamCycles(teamId, leagueId);
            return MapResults(resList, seasonId);
        }

        public IEnumerable<ExcelGameDto> GetLeagueGames(int leagueId, int[] gameIds)
        {
            var resList = gamesRepo.GetGroupsCycles(leagueId);

            if (gameIds.Length > 0)
                resList = resList.Where(w => gameIds.Any(a => a == w.CycleId));

            return MapResults(resList);
        }

        public IEnumerable<ExcelGameDto> GetTeamGames(int teamId, int[] gameIds, int? seasonId = null)
        {
            var resList = gamesRepo.GetTeamCycles(teamId);

            if (gameIds.Length > 0)
                resList = resList.Where(w => gameIds.Any(a => a == w.CycleId));

            return MapResults(resList, seasonId);
        }

        public IEnumerable<ExcelGameDto> GetTeamGames(int teamId, int leagueId, int[] gameIds, int? seasonId = null)
        {
            var resList = gamesRepo.GetTeamCycles(teamId, leagueId).OrderBy(g => g.StartDate).AsEnumerable();
            if (gameIds.Length > 0)
                resList = resList.Where(w => gameIds.Any(a => a == w.CycleId)).OrderBy(g => g.StartDate);

            return MapResults(resList, seasonId);
        }
    }
}
