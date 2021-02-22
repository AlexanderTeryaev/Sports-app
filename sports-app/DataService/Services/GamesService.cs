using System;
using DataService.DTO;
using AppModel;

namespace DataService.Services
{
    public class GamesService
    {
        readonly GamesRepo _gamesRepo = new GamesRepo();

        public void SetTechnicalWinForGame(int gameId, int teamId)
        {
            var gc = _gamesRepo.GetGameCycleById(gameId);
            var gameAlias = gc.Stage?.League?.Union?.Section?.Alias ?? gc.Stage?.League?.Club?.Section?.Alias;

            switch (gameAlias)
            {
                case GamesAlias.WaterPolo:
                case GamesAlias.Soccer:
                    _gamesRepo.WaterPoloTechnicalWin(gameId, teamId);
                    break;
                case GamesAlias.Softball:
                    _gamesRepo.SoftballTechnicalWin(gameId, teamId);
                    break;
                case GamesAlias.BasketBall:
                case GamesAlias.Rugby:
                    _gamesRepo.BasketBallTechnicalWin(gameId, teamId);
                    break;
                default:
                    _gamesRepo.TechnicalWin(gameId, teamId);
                    break;
            }

            _gamesRepo.UpdateGameScore(gameId);
        }
        /** add cheng for comment of EditGamePartial */
        public void SetGameCycleComments(int gameId, string comments)
        {
            _gamesRepo.GameCycleComments(gameId, comments);
        }

        public void SetTechnicalWinForAthletesGame(int gameId, int athleteId)
        {
            var gc = _gamesRepo.GetGameCycleById(gameId);
            var gameAlias = gc.Stage?.League?.Union?.Section?.Alias;

            _gamesRepo.TechnicalWinForAthlete(gameId, athleteId);

            _gamesRepo.UpdateGameScore(gameId);
        }

        public void AddStatistics(StatisticBindingModel statistic)
        {
            var statisticTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(statistic.Timestamp);

            var modelStatistic = new Statistic
            {
                Abbreviation = statistic.Abbreviation,
                GameTime = statistic.GameTime,
                //Category = statistic.Category,
                GameId = statistic.GameId,
                //Note = statistic.Note,
                PlayerId = statistic.PlayerId,
                Point_x = statistic.Location.X,
                Point_y = statistic.Location.Y,
                //ReporterId = statistic.ReporterId,
                //SegmentTimeStamp = statistic.SegmentTimeStamp,
                //StatisticTypeId = statistic.StatisticTypeId,
                //SyncStatus = statistic.SyncStatus,
                TeamId = statistic.TeamId,
                //TimeSegment = statistic.TimeSegment,
                TimeSegmentName = statistic.TimeSegmentName,
                Timestamp = statisticTimestamp
            };

            _gamesRepo.AddStatistic(modelStatistic);
        }
    }
}
