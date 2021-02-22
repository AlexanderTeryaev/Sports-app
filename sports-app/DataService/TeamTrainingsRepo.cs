using System;
using System.Linq;
using AppModel;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using DataService.DTO;

namespace DataService
{
    public class TeamTrainingsRepo : BaseRepo
    {
        private DataEntities _db;

        public TeamTrainingsRepo()
        {
            _db = new DataEntities();
        }

        public string GetTeamsNameById(int teamId)
        {
            return _db.Teams.FirstOrDefault(t => t.TeamId == teamId)?.Title;
        }
        public List<TrainingDaysSetting> GetTrainingDaysSettingsForTeam(int teamId)
        {
            var trainingDaysSettings = _db.TrainingDaysSettings.Where(m => m.TeamId == teamId)
                .ToList();
            return trainingDaysSettings;
        }

        /// <summary>
        /// Insert values into TeamTraining table
        /// </summary>
        /// <param name="obj"></param>
        public void InsertTeamTrainings(TeamTraining obj)
        {
            try
            {
                var selectedTeamTrainingData = _db.TeamTrainings.FirstOrDefault(x => x.Id == obj.Id);
                if (selectedTeamTrainingData != null)
                {
                    _db.TeamTrainings.Remove(selectedTeamTrainingData);
                }
                _db.TeamTrainings.Add(obj);
                _db.SaveChanges();
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Get all team settings of team
        /// </summary>
        /// <param name="teamId">team id</param>
        /// <returns></returns>
        public List<TeamTraining> GetAllTeamTrainingsByTeamId(int teamId, int? seasonId)
        {
            var listOfTrainings = _db.TeamTrainings.Where(l => l.TeamId == teamId && l.SeasonId == seasonId).ToList();
            return listOfTrainings;
        }

        public void GetAllTeamPlayers(int teamId)
        {
            var list = _db.TeamsPlayers.Where(p => p.TeamId == teamId).AsEnumerable();
            var list2 = _db.Users.FirstOrDefault()?.UserId;
        }

        /// <summary>
        /// Remove teams by team id
        /// </summary>
        /// <param name="teamId"></param>
        public void RemoveTeamTrainingByTeamId(int teamId)
        {
            try
            {
                var trainingsIds = _db.TeamTrainings.Where(l => l.TeamId == teamId).Select(t => t.Id);

                foreach (var trainingId in trainingsIds)
                {
                    var attendanceToDelete = _db.TrainingAttendances.Where(a => a.TrainingId == trainingId);
                    _db.TrainingAttendances.RemoveRange(attendanceToDelete);
                }
                var listToDelete = _db.TeamTrainings.Where(l => l.TeamId == teamId);
                _db.TeamTrainings.RemoveRange(listToDelete);
                _db.SaveChanges();
            }
            catch (Exception e)
            {

            }

        }


        /// <summary>
        /// Remove team training by its id
        /// </summary>
        public void RemoveTeamTrainingById(int id)
        {
            var attendanceToDelete = _db.TrainingAttendances.Where(a => a.TrainingId == id);
            _db.TrainingAttendances.RemoveRange(attendanceToDelete);

            var itemToDelete = _db.TeamTrainings.FirstOrDefault(t => t.Id == id);
            if (itemToDelete != null)
            {
                _db.TeamTrainings.Remove(itemToDelete);
            }
            _db.SaveChanges();
        }

        /// <summary>
        /// Gets all arenas for certain team
        /// </summary>
        /// <param name="teamsIds"></param>
        /// <returns></returns>
        public List<TeamsAuditorium> GetTeamArenas(params int[] teamsIds)
        {
            return _db.TeamsAuditoriums.Where(a => teamsIds.Contains(a.TeamId)).ToList();
        }

        public List<TeamsAuditorium> GetActiveTeamArenas(int seasonId, params int[] teamsIds)
        {
            return _db.TeamsAuditoriums.Where(a => teamsIds.Contains(a.TeamId) && !a.Auditorium.IsArchive && a.Auditorium.SeasonId == seasonId).ToList();
        }

        public List<Auditorium> GetActiveClubArenas(int seasonId, int clubId)
        {
            return _db.Auditoriums.Where(a => a.ClubId == clubId && !a.IsArchive && a.SeasonId == seasonId).ToList();
        }

        #region Attendance settings
        public Dictionary<int, List<int>> GetAttendancesByTeamId(int teamId)
        {
            var attendanceDictionary = new Dictionary<int, List<int>>();
            var trainingsId = _db.TeamTrainings.Where(t => t.TeamId == teamId).AsNoTracking().ToList().Select(t => t.Id);
            //var allAttendancies = _db.TrainingAttendances.Where(x => trainingsId.Contains(x.TrainingId)).AsNoTracking().ToList();

            foreach (var trainingId in trainingsId)
            {
                var selectedPlayers = _db.TrainingAttendances.Where(t => t.TrainingId == trainingId).Select(t => t.PlayerId).ToList();
                attendanceDictionary.Add(trainingId, selectedPlayers);
            }

            return attendanceDictionary;
        }
        public Dictionary<int, Dictionary<int, List<int>>> GetAttendances(int[] teamIds)
        {
            List<int> teamIdsList = teamIds.Distinct().ToList();
            // structure is teamId -> trainingId-> playerIds
            var teamTrainingPairs = _db.TeamTrainings.Where(t => teamIdsList.Contains(t.TeamId)).AsNoTracking().Select(t => new
            {
                TeamId = t.TeamId,
                TrainingId = t.Id
            }).ToList();

            var trainingIds = teamTrainingPairs.Select(x => x.TrainingId).ToList();
            var trainingPlayerPairs = _db.TrainingAttendances.Where(t => trainingIds.Contains(t.TrainingId))
                .Select(x => new
                {
                    TrainingId = x.TrainingId,
                    PlayerId = x.PlayerId
                })
                .ToList();
            Dictionary<int, List<int>> trainingPlayerDictionary = new Dictionary<int, List<int>>();
            foreach (var trainingId in trainingIds)
            {
                trainingPlayerDictionary.Add(trainingId, trainingPlayerPairs.Where(x => x.TrainingId == trainingId).Select(x => x.PlayerId).ToList());
            }
            var teamTrainingDictionary = new Dictionary<int, Dictionary<int, List<int>>>();
            foreach (var teamId in teamIdsList)
            {
                var trainingIdsForCurrentTeam = teamTrainingPairs.Where(x => x.TeamId == teamId).Select(x => x.TrainingId);
                teamTrainingDictionary.Add(teamId, trainingPlayerDictionary.Where(x => trainingIdsForCurrentTeam.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value));
            }

            return teamTrainingDictionary;
        }


        public IEnumerable<TrainingAttendance> GetAttendancesById(int id)
        {
            var attendances = _db.TrainingAttendances.Where(a => a.TrainingId == id).AsEnumerable();
            return attendances;
        }

        private void UpdateAttendance(int id, IEnumerable<string> playersIdString)
        {
            try
            {
                var playersDb = _db.TrainingAttendances.Where(t => t.TrainingId == id);
                if (playersDb.Count() > 0)
                {
                    _db.TrainingAttendances.RemoveRange(playersDb);
                    _db.SaveChanges();
                }

                foreach (var playerIdComma in playersIdString)
                {
                    foreach (var playerId in playerIdComma.Split(','))
                    {
                        var attendance = new TrainingAttendance
                        {
                            TrainingId = id,
                            PlayerId = Convert.ToInt32(playerId)
                        };
                        _db.TrainingAttendances.Add(attendance);
                    }
                }
                _db.SaveChanges();
            }
            catch (Exception e)
            {

            }

        }
        #endregion

        /// <summary>
        /// Update team trainings
        /// </summary>
        public void UpdateTeamTrainingById(int id, string title, int? auditoriumId, DateTime date, string content, IEnumerable<string> playersId, string trainingReportName)
        {
            try
            {
                var itemToUpdate = _db.TeamTrainings.FirstOrDefault(t => t.Id == id);
                if (itemToUpdate != null)
                {
                    itemToUpdate.Title = title;
                    itemToUpdate.TrainingDate = date;
                    itemToUpdate.Content = content;
                    itemToUpdate.AuditoriumId = auditoriumId;
                    if (trainingReportName != null)
                    {
                        itemToUpdate.TrainingReport = trainingReportName;
                    }
                }
                _db.SaveChanges();
                UpdateAttendance(id, playersId);
            }
            catch (Exception e)
            {

            }
        }

        public void CreateTrialPlayer(int teamId, int clubId, int unionId,
            string firstName,
            string lastName,
            string middleName,
            string phone, int userid)
        {
            try
            {
                var temp = from t in _db.TeamsPlayers
                           where (t.TeamId == teamId)
                           select new
                           {
                               id = t.ClubId
                           };

                clubId = (int)temp.FirstOrDefault().id;
                temp = from c in _db.TeamsPlayers
                       where (c.TeamId == teamId)
                       select new
                       {
                           id = c.LeagueId
                       };
                var leagueId = temp.FirstOrDefault().id;
                var seasonId =
                    from userjobs in _db.UsersJobs
                    from s in _db.Seasons
                    where (userjobs.TeamId == teamId && userjobs.SeasonId == s.Id && s.StartDate < DateTime.Now &&
                           s.EndDate > DateTime.Now)
                    select new
                    {
                        id = userjobs.SeasonId
                    };
                var athleteNumbers = new List<AthleteNumber>();
                athleteNumbers.Add(new AthleteNumber
                {
                    SeasonId = seasonId.FirstOrDefault()?.id ?? 0
                });
                var newUser = new User
                {
                    //UserName = name,
                    FirstName = firstName,
                    LastName = lastName,
                    MiddleName = middleName,
                    Telephone = phone,
                    TypeId = 6,
                    GenderId = 3,
                    IsActive = true,
                    TriesNum = 0,
                    IsBlocked = false,
                    IsArchive = false,
                    TestResults = 0,
                    NoInsurancePayment = false,
                    IsCompetitiveMember = false,
                    AthleteNumbers = athleteNumbers
                };

                var player = new TeamsPlayer
                {
                    UserId = newUser.UserId,
                    TeamId = teamId,
                    ClubId = clubId,
                    LeagueId = leagueId,
                    SeasonId = seasonId.FirstOrDefault()?.id
                };
                _db.TeamsPlayers.Add(player);
                _db.SaveChanges();
            }
            catch (Exception e)
            {

            }
        }

        public void UpdateTrainingDate(int trainingId, DateTime date)
        {
            var trainingToUpdate = _db.TeamTrainings.FirstOrDefault(t => t.Id == trainingId);
            if (trainingToUpdate != null)
            {
                trainingToUpdate.TrainingDate = date;
                _db.SaveChanges();
            }
        }

        /// <summary>
        /// Get all players of team 
        /// </summary>
        /// <param name="teamsIds"></param>
        /// <returns></returns>
        public List<TeamsPlayer> GetPlayersByTeamId(int seasonId, params int[] teamsIds)
        {
            var teamPlayersList = _db.TeamsPlayers
                .Where(v => teamsIds.Contains(v.TeamId) && v.SeasonId == seasonId)
                .GroupBy(t => t.UserId)
                .Select(grp => grp.FirstOrDefault())
                .ToList();

            return teamPlayersList;
        }

        public List<TeamsPlayer> GetPlayersByTeamId(int teamId, int? seasonId)
        {
            var teamPlayersList = _db.TeamsPlayers
                .Where(v => v.TeamId == teamId && v.SeasonId == seasonId)
                .GroupBy(t => t.UserId)
                .Select(grp => grp.FirstOrDefault())
                .ToList();

            return teamPlayersList;
        }

        #region Date filters
        /// <summary>
        /// Returns list of trainings in date range
        /// </summary>
        /// <param name="teamId">Id of team</param>
        /// <param name="startDate">Start range day</param>
        /// <param name="endDate">Enda range day</param>
        /// <returns></returns>
        public List<TeamTraining> GetTrainingsInDateRange(int teamId, DateTime startDate, DateTime endDate, int seasonId)
        {
            var listOfTrainings = _db.TeamTrainings
                .Where(l => l.TeamId == teamId && l.TrainingDate >= startDate && l.TrainingDate <= endDate && l.SeasonId == seasonId)
                .ToList();
            return listOfTrainings;
        }

        /// <summary>
        /// Returns trainings in range of first week of current month
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public List<TeamTraining> GetTrainingsOfFirstDaysOfMonth(int teamId, int seasonId)
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = DateTime.MaxValue;
            var firstWeekList = GetTrainingsInDateRange(teamId, startDate, endDate, seasonId);
            return firstWeekList;
        }
        #endregion

        public List<DateTime> GetDatesOfGames(int teamId, DateTime startDate, DateTime endDate)
        {
            var teamName = _db.Teams.FirstOrDefault(t => t.TeamId == teamId)?.Title;

            var homeGamesCycles = _db.GamesCycles.Where(t => t.HomeTeamId == teamId).Select(t => t.StartDate).ToList();
            var guestGamesCycles = _db.GamesCycles.Where(t => t.GuestTeamId == teamId).Select(t => t.StartDate).ToList();

            var homeTeamScheduleScrapper = _db.TeamScheduleScrappers
                .Where(t => t.HomeTeam == teamName).Select(t => t.StartDate).ToList();
            var guestTeamScheduleScrapper = _db.TeamScheduleScrappers
                .Where(t => t.GuestTeam == teamName).Select(t => t.StartDate).ToList();

            var gamesList = new List<DateTime>();
            gamesList.AddRange(homeGamesCycles);
            gamesList.AddRange(guestGamesCycles);
            gamesList.AddRange(homeTeamScheduleScrapper);
            gamesList.AddRange(guestTeamScheduleScrapper);

            return gamesList.Where(d => d >= startDate && d <= endDate).ToList();
        }
        public List<DateTime> GetDatesOfGames(int teamId, DateTime dateOfTraining)
        {
            var teamName = _db.Teams.FirstOrDefault(t => t.TeamId == teamId)?.Title;

            var homeGamesCycles = _db.GamesCycles.Where(t => t.HomeTeamId == teamId).Select(t => t.StartDate).ToList();
            var guestGamesCycles = _db.GamesCycles.Where(t => t.GuestTeamId == teamId).Select(t => t.StartDate).ToList();

            var homeTeamScheduleScrapper = _db.TeamScheduleScrappers
                .Where(t => t.HomeTeam == teamName).Select(t => t.StartDate).ToList();
            var guestTeamScheduleScrapper = _db.TeamScheduleScrappers
                .Where(t => t.GuestTeam == teamName).Select(t => t.StartDate).ToList();

            var gamesList = new List<DateTime>();
            gamesList.AddRange(homeGamesCycles);
            gamesList.AddRange(guestGamesCycles);
            gamesList.AddRange(homeTeamScheduleScrapper);
            gamesList.AddRange(guestTeamScheduleScrapper);

            return gamesList.Where(date => date.ToShortDateString() == dateOfTraining.ToShortDateString()).ToList();
        }

        /// <summary>
        /// Get coach teams
        /// </summary>
        public List<Team> GetCoachTeams(int teamId)
        {
            var teamsCoachId = db.UsersJobs.Where(j => j.Job.JobName == "מאמן" || j.Job.JobName == "Coach" && j.Job.IsArchive == false)
                .Where(j => j.TeamId == teamId)
                .Select(j => j.Id)
                .FirstOrDefault();
            var coachTeams = db.UsersJobs.Where(j => j.Id == teamsCoachId)
                .Select(j => j.Team)
                .ToList();
            return coachTeams;


        }

        public void UpdateDatesInTable(List<ExcelTeamTrainingDto> excelTeamTrainings, int teamId)
        {
            var trainingsToUpdate = db.TeamTrainings.Where(t => t.TeamId == teamId).ToList();
            if (excelTeamTrainings != null)
            {
                foreach (var training in trainingsToUpdate)
                {
                    foreach (var excelTraining in excelTeamTrainings)
                    {
                        if (training.Id == excelTraining.TrainingId)
                        {
                            training.TrainingDate = excelTraining.TrainingDate;
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        public void PublishTrainingById(int trainingId, bool publishValue)
        {
            var trainingToPublish = _db.TeamTrainings
                .FirstOrDefault(t => t.Id == trainingId);

            if (trainingToPublish != null)
            {
                trainingToPublish.isPublished = publishValue;
                _db.SaveChanges();
            }
        }

        public bool CheckIfTeamIsBlocked(int teamId, int? clubId, int? sesonId)
        {
            return _db.ClubTeams.FirstOrDefault(c => c.ClubId == clubId && c.SeasonId == sesonId && c.TeamId == teamId)?.IsBlocked ?? false;
        }

        public bool IsClubTrainingEnabledForTeam(int teamId)
        {
            var clubTeam = _db.ClubTeams.Include(c => c.Club).FirstOrDefault(c => c.TeamId == teamId);
            var schoolClub = _db.SchoolTeams.FirstOrDefault(s => s.TeamId == teamId)?.School?.Club;

            var isSchoolTrainingEnabled = schoolClub != null && schoolClub.IsTrainingEnabled == true;
            var isClubTrainingEnabled = clubTeam != null && clubTeam.Club.IsTrainingEnabled;

            var result = clubTeam == null ? isSchoolTrainingEnabled : isClubTrainingEnabled;
            return result;
        }

        public TrainingSetting GetTrainingSettingForTeam(int teamId)
        {
            return _db.TrainingSettings.FirstOrDefault(t => t.TeamID == teamId);
        }
    }
}
