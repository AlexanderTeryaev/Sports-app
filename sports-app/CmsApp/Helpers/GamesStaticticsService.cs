using DataService;
using DataService.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Helpers
{
    public class GamesStaticticsService
    {
        GamesRepo gamesRepo;
        LeagueRepo leagueRepo;

        public GamesStaticticsService()
        {
            gamesRepo = new GamesRepo();
            leagueRepo = new LeagueRepo();
        }

        public List<PlayersStatisticsDTO> GetWGamesStatistics(int cycleId, string gameType)
        {
            var cycle = gamesRepo.GetGameCycleById(cycleId);
            var games = new List<PlayersStatisticsDTO>();
            if (cycle != null)
            {
                switch (gameType)
                {
                    case "home":
                        var homeTeamId = cycle.HomeTeamId;
                        if (homeTeamId.HasValue)
                            games = gamesRepo.GetStatisticsOfWGame(cycle, homeTeamId.Value);
                        break;
                    case "guest":
                        var guestTeamId = cycle.GuestTeamId;
                        if (guestTeamId.HasValue)
                            games = gamesRepo.GetStatisticsOfWGame(cycle, guestTeamId.Value);
                        break;
                }
            }
            return games;
        }

        public List<PlayersStatisticsDTO> GetGamesStatistics(int cycleId, string gameType)
        {
            var cycle = gamesRepo.GetGameCycleById(cycleId);
            var games = new List<PlayersStatisticsDTO>();
            if (cycle != null)
            {
                switch (gameType)
                {
                    case "home":
                        var homeTeamId = cycle.HomeTeamId;
                        if (homeTeamId.HasValue)
                            games = gamesRepo.GetStatisticsOfGame(cycle, homeTeamId.Value);
                        break;
                    case "guest":
                        var guestTeamId = cycle.GuestTeamId;
                        if (guestTeamId.HasValue)
                            games = gamesRepo.GetStatisticsOfGame(cycle, guestTeamId.Value);
                        break;
                }
            }
            return games;
        }

        public LeagueStatisticsViewModel GetLeagueStatistics(int leagueId, int seasonId)
        {
            var league = leagueRepo.GetById(leagueId);
            var leagueStatistics = new LeagueStatisticsViewModel();
            if (league != null)
            {
                var leagueGamesStatistics = leagueRepo.GetLeagueStatistics(leagueId, seasonId).ToList();
                if (leagueGamesStatistics.Count > 0)
                {
                    leagueStatistics.Points = CalculatePoints(leagueGamesStatistics).OrderByDescending(s => s.Percentage).ThenByDescending(s => s.CategoryPoints).ThenBy(s => s.PlayerFullName).ToArray();
                    leagueStatistics.Rebounds = CalculateRebounds(leagueGamesStatistics).OrderByDescending(s => s.Percentage).ThenByDescending(s => s.CategoryPoints).ThenBy(s => s.PlayerFullName).ToArray();
                    leagueStatistics.Assists = CalculateAssists(leagueGamesStatistics).OrderByDescending(s => s.Percentage).ThenByDescending(s => s.CategoryPoints).ThenBy(s => s.PlayerFullName).ToArray();
                    leagueStatistics.Blocks = CalculateBlocks(leagueGamesStatistics).OrderByDescending(s => s.Percentage).ThenByDescending(s => s.CategoryPoints).ThenBy(s => s.PlayerFullName).ToArray();
                    leagueStatistics.Steals = CalculateSteals(leagueGamesStatistics).OrderByDescending(s => s.Percentage).ThenByDescending(s => s.CategoryPoints).ThenBy(s => s.PlayerFullName).ToArray();
                    leagueStatistics.Turnovers = CalculateTurnovers(leagueGamesStatistics).OrderByDescending(s => s.Percentage).ThenByDescending(s => s.CategoryPoints).ThenBy(s => s.PlayerFullName).ToArray();
                    leagueStatistics.FGPercent = CalculateFGPercent(leagueGamesStatistics).OrderByDescending(s => s.Percentage).ThenBy(s => s.PlayerFullName).ToArray();
                    leagueStatistics.FTPercent = CalculateFTPercent(leagueGamesStatistics).OrderByDescending(s => s.Percentage).ThenBy(s => s.PlayerFullName).ToArray();
                    leagueStatistics.ThreePTPercent = CalculateThreePTPercent(leagueGamesStatistics).OrderByDescending(s => s.Percentage).ThenBy(s => s.PlayerFullName).ToArray();
                    leagueStatistics.EFF = CalculateEff(leagueGamesStatistics).OrderByDescending(s => s.Percentage).ThenBy(s => s.PlayerFullName).ToArray();
                }
            }
            return leagueStatistics;
        }

        private IEnumerable<LeagueStatisticEntity> CalculateEff(IEnumerable<PlayersStatisticsDTO> leagueGamesStatistics)
        {
            if (leagueGamesStatistics != null)
            {
                foreach (var stats in leagueGamesStatistics)
                {
                    yield return new LeagueStatisticEntity
                    {
                        PlayerId = stats.PlayersId ?? 0,
                        PlayerFullName = stats.PlayersName,
                        CategoryPoints = Convert.ToInt32(stats.EFF),
                        Percentage = stats.EFF / (double)stats.GamesCount,
                        PlayersImage = stats.PlayersImage
                    };
                }
            }
        }

        private IEnumerable<LeagueStatisticEntity> CalculateThreePTPercent(IEnumerable<PlayersStatisticsDTO> leagueGamesStatistics)
        {
            if (leagueGamesStatistics != null)
            {

                foreach (var stats in leagueGamesStatistics)
                {
                    var value = stats.ThreePA > 0 ? (Convert.ToDouble(stats.ThreePT) / Convert.ToDouble(stats.ThreePA)) * 100 : 0D;
                    yield return new LeagueStatisticEntity
                    {
                        PlayerId = stats.PlayersId ?? 0,
                        PlayerFullName = stats.PlayersName,
                        Percentage = stats.ThreePA > 0 ? (Convert.ToDouble(stats.ThreePT) / Convert.ToDouble(stats.ThreePA)) * 100 : 0D,
                        PlayersImage = stats.PlayersImage
                    };
                }
            }
        }

        private IEnumerable<LeagueStatisticEntity> CalculateFTPercent(IEnumerable<PlayersStatisticsDTO> leagueGamesStatistics)
        {
            if (leagueGamesStatistics != null)
            {
                foreach (var stats in leagueGamesStatistics)
                {
                    yield return new LeagueStatisticEntity
                    {
                        PlayerId = stats.PlayersId ?? 0,
                        PlayerFullName = stats.PlayersName,
                        Percentage = stats.FTA > 0 ? (Convert.ToDouble(stats.FT) / Convert.ToDouble(stats.FTA)) * 100 : 0D,
                        PlayersImage = stats.PlayersImage
                    };
                }
            }
        }

        private IEnumerable<LeagueStatisticEntity> CalculateFGPercent(IEnumerable<PlayersStatisticsDTO> leagueGamesStatistics)
        {
            if (leagueGamesStatistics != null)
            {
                foreach (var stats in leagueGamesStatistics)
                {
                    yield return new LeagueStatisticEntity
                    {
                        PlayerId = stats.PlayersId ?? 0,
                        PlayerFullName = stats.PlayersName,
                        Percentage = stats.FGA > 0 ? (Convert.ToDouble(stats.FG) / Convert.ToDouble(stats.FGA)) * 100 : 0D,
                        PlayersImage = stats.PlayersImage
                    };
                }
            }
        }

        private IEnumerable<LeagueStatisticEntity> CalculateTurnovers(IEnumerable<PlayersStatisticsDTO> leagueGamesStatistics)
        {
            if (leagueGamesStatistics != null)
            {
                foreach (var stats in leagueGamesStatistics)
                {
                    yield return new LeagueStatisticEntity
                    {
                        PlayerId = stats.PlayersId ?? 0,
                        PlayerFullName = stats.PlayersName,
                        CategoryPoints = stats.TO,
                        Percentage = (double)stats.TO / (double)stats.GamesCount,
                        PlayersImage = stats.PlayersImage
                    };
                }
            }
        }

        private IEnumerable<LeagueStatisticEntity> CalculateSteals(IEnumerable<PlayersStatisticsDTO> leagueGamesStatistics)
        {
            if (leagueGamesStatistics != null)
            {
                foreach (var stats in leagueGamesStatistics)
                {
                    yield return new LeagueStatisticEntity
                    {
                        PlayerId = stats.PlayersId ?? 0,
                        PlayerFullName = stats.PlayersName,
                        CategoryPoints = stats.STL,
                        Percentage = (double)stats.STL / (double)stats.GamesCount,
                        PlayersImage = stats.PlayersImage
                    };
                }
            }
        }

        private IEnumerable<LeagueStatisticEntity> CalculateBlocks(IEnumerable<PlayersStatisticsDTO> leagueGamesStatistics)
        {
            if (leagueGamesStatistics != null)
            {
                foreach (var stats in leagueGamesStatistics)
                {
                    yield return new LeagueStatisticEntity
                    {
                        PlayerId = stats.PlayersId ?? 0,
                        PlayerFullName = stats.PlayersName,
                        CategoryPoints = stats.BLK,
                        Percentage = (double)stats.BLK / (double)stats.GamesCount,
                        PlayersImage = stats.PlayersImage
                    };
                }
            }
        }

        private IEnumerable<LeagueStatisticEntity> CalculateAssists(IEnumerable<PlayersStatisticsDTO> leagueGamesStatistics)
        {
            if (leagueGamesStatistics != null)
            {
                foreach (var stats in leagueGamesStatistics)
                {
                    yield return new LeagueStatisticEntity
                    {
                        PlayerId = stats.PlayersId ?? 0,
                        PlayerFullName = stats.PlayersName,
                        CategoryPoints = stats.AST,
                        Percentage = (double)stats.AST / (double)stats.GamesCount,
                        PlayersImage = stats.PlayersImage
                    };
                }
            }
        }

        private IEnumerable<LeagueStatisticEntity> CalculateRebounds(IEnumerable<PlayersStatisticsDTO> leagueGamesStatistics)
        {
            if (leagueGamesStatistics != null)
            {
                foreach (var stats in leagueGamesStatistics)
                {
                    yield return new LeagueStatisticEntity
                    {
                        PlayerId = stats.PlayersId ?? 0,
                        PlayerFullName = stats.PlayersName,
                        CategoryPoints = stats.REB,
                        Percentage = (double)stats.REB / (double)stats.GamesCount,
                        PlayersImage = stats.PlayersImage
                    };
                }
            }
        }

        private IEnumerable<LeagueStatisticEntity> CalculatePoints(IEnumerable<PlayersStatisticsDTO> leagueGamesStatistics)
        {
            if (leagueGamesStatistics != null)
            {
                foreach (var stats in leagueGamesStatistics)
                {
                    yield return new LeagueStatisticEntity
                    {
                        PlayerId = stats.PlayersId ?? 0,
                        PlayerFullName = stats.PlayersName,
                        CategoryPoints = stats.PTS,
                        Percentage = (double)stats.PTS / (double)stats.GamesCount,
                        PlayersImage = stats.PlayersImage
                    };
                }
            }
        }
    }

    public class LeagueStatisticsViewModel
    {
        /// <summary>
        /// PTS
        /// </summary>
        public LeagueStatisticEntity[] Points { get; set; }
        /// <summary>
        /// REB
        /// </summary>
        public LeagueStatisticEntity[] Rebounds { get; set; }
        /// <summary>
        /// AST
        /// </summary>
        public LeagueStatisticEntity[] Assists { get; set; }
        /// <summary>
        /// BLK
        /// </summary>
        public LeagueStatisticEntity[] Blocks { get; set; }
        /// <summary>
        /// STL
        /// </summary>
        public LeagueStatisticEntity[] Steals { get; set; }
        /// <summary>
        /// TO
        /// </summary>
        public LeagueStatisticEntity[] Turnovers { get; set; }

        public LeagueStatisticEntity[] FGPercent { get; set; }
        public LeagueStatisticEntity[] FTPercent { get; set; }
        public LeagueStatisticEntity[] ThreePTPercent { get; set; }
        public LeagueStatisticEntity[] EFF { get; set; }


    }

    public class LeagueStatisticEntity
    {
        public int PlayerId { get; set; }
        public string PlayerFullName { get; set; }
        public string PlayersImage { get; set; }
        public int CategoryPoints { get; set; }
        public double Percentage { get; set; }
        public double PercentageValue => Math.Round(this.Percentage, 1);
    }
}