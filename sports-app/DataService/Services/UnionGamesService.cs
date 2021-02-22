using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;
using DataService.DTO;

namespace DataService.Services
{
    public class UnionGamesService
    {
        private int unionId;
        private int seasonId;

        private DataEntities db;
        private DisciplinesRepo disciplineDb;
        private LeagueRepo leagueDb;
        private TeamsRepo teamsDb;

        public UnionGamesService(int unionId, int seasonId)
        {
            this.unionId = unionId;
            this.seasonId = seasonId;
            db = new DataEntities();
            disciplineDb = new DisciplinesRepo(db);
            leagueDb = new LeagueRepo();
            teamsDb = new TeamsRepo();
        }

        private IEnumerable<DisciplineDTO> GetAllDisciplines()
        {
            return disciplineDb.GetAllByUnionId(unionId);
        }

        private IEnumerable<LeagueShort> GetAllLeagues()
        {
            var disciplines = GetAllDisciplines();
            var listOfLeagues = new List<LeagueShort>();

            foreach (var discipline in disciplines.ToList())
            {
                var disciplineLeagues = leagueDb
                    .GetLeaguesFilterList(unionId, seasonId, discipline.DisciplineId).ToList();

                foreach (var league in disciplineLeagues)
                {
                    league.DisciplineId = discipline.DisciplineId;
                    league.DisciplineName = discipline.Name;
                }
                listOfLeagues.AddRange(disciplineLeagues);
            }

            return listOfLeagues;
        }

        private IEnumerable<TeamDto> GetAllTeams()
        {
            var leagues = GetAllLeagues();
            var listOfLeaguesTeams = new List<TeamDto>();
            foreach (var league in leagues.ToList())
            {
                var leagueTeams = teamsDb.GetTeams(seasonId, league.Id);
                foreach (var team in leagueTeams)
                {
                    listOfLeaguesTeams.Add(new TeamDto
                    {
                        DisciplineId = league.DisciplineId,
                        DisciplineName = league.DisciplineName,
                        TeamId = team.TeamId,
                        Title = team.Title,
                        LeagueId = league.Id
                    });
                }
            }
            return listOfLeaguesTeams;
        }

        private IEnumerable<GameDto> GetGameCyclesForNonIndividualSection()
        {
            List<GamesCycle> gamesCycles = new List<GamesCycle>();
            var leagues = leagueDb.GetByUnion(this.unionId, this.seasonId);
            foreach (var league in leagues)
            {
                var stages = league.Stages;
                foreach (var stage in stages)
                {
                    gamesCycles.AddRange(stage.GamesCycles);
                }
            }

            string sectionAlias = leagues.FirstOrDefault()?.Union?.Section?.Alias;
            bool isPenaltySection = string.Equals(sectionAlias, GamesAlias.WaterPolo, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(sectionAlias, GamesAlias.Soccer, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(sectionAlias, GamesAlias.Rugby, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(sectionAlias, GamesAlias.Handball, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(sectionAlias, GamesAlias.BasketBall, StringComparison.OrdinalIgnoreCase);
            return GetGameCycleDtosFromGameCycles(gamesCycles, isPenaltySection);
        }

        private static IEnumerable<GameDto> GetGameCycleDtosFromGameCycles(List<GamesCycle> gamesCycles, bool isPenaltySection)
        {
            var gamesDtos = new List<GameDto>();
            foreach (var gamesCycle in gamesCycles)
            {
                var homeTeamTitle = gamesCycle.HomeTeam == null ? "" : gamesCycle.HomeTeam.Title;
                var guestTeamTitle = gamesCycle.GuestTeam == null ? "" : gamesCycle.GuestTeam.Title;
                var penalty = gamesCycle.GameSets.FirstOrDefault(c => c.IsPenalties);
                gamesDtos.Add(new GameDto
                {
                    StartDate = gamesCycle.StartDate,
                    Auditorium = gamesCycle.Auditorium?.Name,
                    AuditoriumAddress = gamesCycle.Auditorium?.Address,
                    HomeTeamDetail = new TeamDetailsDto
                    {
                        TeamId = gamesCycle.HomeTeamId,
                        TeamName = homeTeamTitle,
                        TeamScore = gamesCycle.HomeTeamScore
                    },
                    GuestTeamDetail = new TeamDetailsDto
                    {
                        TeamId = gamesCycle.GuestTeamId,
                        TeamName = guestTeamTitle,
                        TeamScore = gamesCycle.GuestTeamScore
                    },
                    GameCycleStatus = gamesCycle.GameStatus ?? "",
                    StageId = gamesCycle.StageId,
                    LeagueName = gamesCycle?.Stage?.League?.Name ?? "",
                    HomeTeamScore = (penalty != null || !isPenaltySection)? gamesCycle.HomeTeamScore : gamesCycle.GameSets.Where(c => !c.IsPenalties).Sum(x => x.HomeTeamScore),
                    GuestTeamScore = (penalty != null || !isPenaltySection) ? gamesCycle.GuestTeamScore : gamesCycle.GameSets.Where(c => !c.IsPenalties).Sum(x => x.GuestTeamScore)
                });
            }

            return gamesDtos;
        }

        public IEnumerable<GameDto> GetAllGames(bool? isIndividual)
        {
            var allGamesByDisciplines = new List<GameDto>();
            IEnumerable<TeamDto> teams;
            if (isIndividual == true)
            {
                teams = GetAllTeams();
                if (teams.Count() == 0)
                    return null;
                foreach (var team in teams.ToList())
                {
                    var gamesOfTeam = teamsDb.GetTeamGames(team.TeamId);
                    foreach (var game in gamesOfTeam.ToList())
                    {
                        game.DisciplineId = team.DisciplineId;
                        game.DisciplineName = team.DisciplineName;
                    }
                    allGamesByDisciplines.AddRange(gamesOfTeam);
                }
                return allGamesByDisciplines;
            }
            else
            {
                return GetGameCyclesForNonIndividualSection();
            }
        }

        public IEnumerable<GameDto> GetAllClubGames(int clubId)
        {
            var allGamesByDisciplines = new List<GameDto>();
                var teams = teamsDb.GetClubTeamsByClubAndSeasonId(clubId, seasonId);
                if (teams.Count() == 0)
                    return null;
                foreach (var team in teams.ToList())
                {
                    var gamesOfTeam = teamsDb.GetTeamGames(team.TeamId);
                    foreach (var game in gamesOfTeam.ToList())
                    {
                        game.DisciplineId = team.LeagueTeams.FirstOrDefault()?.Leagues.DisciplineId ?? 0;
                        game.DisciplineName = team.LeagueTeams.FirstOrDefault()?.Leagues.Discipline?.Name ?? "Discipline";
                        game.ClubTeamId = team.TeamId;
                    }
                    allGamesByDisciplines.AddRange(gamesOfTeam);
                }
                return allGamesByDisciplines;
        }


        public IEnumerable<GameDto> GetAllCompetitions(bool isTennis = false)
        {
            var competitions = leagueDb.GetByUnion(unionId, seasonId);
            var result = new List<GameDto>();

            foreach(var league in competitions)
            {
                DateTime? start;
                DateTime? end;
                bool isFromCategoryDates = false;
                if (league.LeagueStartDate == null && !isTennis) continue;
                else
                {
                    if(league.LeagueStartDate == null)
                    {
                        //var t = league.LeagueTeams.Select(x => x.Teams);
                        //t.Min(x => x.CategoriesPlaceDates.Min(c => c.QualificationStartDate));
                        start = league.LeagueTeams.Min(x => x.Teams.CategoriesPlaceDates.Min(t => t.QualificationStartDate));
                        if(start == null) start = league.LeagueTeams.Min(x => x.Teams.CategoriesPlaceDates.Min(t => t.FinalStartDate));
                        end = league.LeagueTeams.Max(x => x.Teams.CategoriesPlaceDates.Max(t => t.FinalEndDate));
                        if(start == null)
                        {
                            if(end != null)
                            {
                                start = end;
                                end = null;
                            }
                            else
                            {                                
                                start = league.LeagueEndDate;
                            }
                        }
                        isFromCategoryDates = true;
                    }
                    else
                    {
                        start = league.LeagueStartDate.Value;
                        end = league.LeagueEndDate;
                    }
                }
                if (start == null) continue;
                result.Add(new GameDto()
                {
                    LeagueId = league.LeagueId,
                    LeagueName = league.Name,
                    StartDate = start.Value,
                    EndDate = end,
                    //AuditoriumAddress = league.Auditorium?.Address ?? "",
                    //Auditorium = league.Auditorium?.Name ?? "",
                    AuditoriumAddress = league.PlaceOfCompetition,
                    DisciplineName = league.Discipline?.Name ?? "Discipline",
                    DisciplineId = league.Discipline?.DisciplineId ?? 0,
                    isFromCategoryDates = isFromCategoryDates
                });
            }

            return result;
        } 

    }
}