using AppModel;
using DataService.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DataService
{
    public class ClubTeamTrainingsRepo : BaseRepo
    {
        private DataEntities _db;

        public ClubTeamTrainingsRepo()
        {
            _db = new DataEntities();
        }

        public List<ClubTeam> GetAllClubTeams(int clubId, int seasonId)
        {
            return _db.ClubTeams.Include(x => x.Club)
				.Where(c => c.ClubId == clubId && c.SeasonId == seasonId)
				.ToList();
        }

        public List<int> GetAllClubTeamsIds(int clubId, int seasonId, bool shouldLoadSchoolTeams = false)
        {
            var teamIds = _db.ClubTeams.Where(c => c.ClubId == clubId && c.SeasonId == seasonId)
                .Select(x=>x.TeamId)
                .ToList();
            if (shouldLoadSchoolTeams)
            {
                List<int> schoolTeamIds = new List<int>();
                var club = db.Clubs.Find(clubId);
                var schools = club.Schools.Where(x => x.SeasonId == seasonId);
                foreach (var school in schools)
                {
                    schoolTeamIds.AddRange(school.SchoolTeams.Select(y => y.TeamId).ToList());
                }
                teamIds.AddRange(schoolTeamIds);
            }

            return teamIds;
        }

        public string GetClubName(int clubId)
        {
            return _db.Clubs.Where(c => c.ClubId == clubId)?.FirstOrDefault()?.Name;
        }


        public List<Auditorium> GetAllClubNonArchiveAuditoriums(int clubId)
        {
            return _db.Auditoriums.Where(a => a.ClubId == clubId && a.IsArchive == false).ToList();
        }

        public void UpdateBlockedItem(int clubId, int teamId, int seasonId, bool isBlockedValue)
        {
            try
            {
                var itemToUpdate = _db.ClubTeams.Where(c => c.ClubId == clubId && c.SeasonId == seasonId && c.TeamId == teamId).FirstOrDefault();
                itemToUpdate.IsBlocked = isBlockedValue;
                _db.SaveChanges();
            }
            catch (Exception e)
            {

            }
        }
        public void UpdateAllBlockedItems(int clubId, int seasonId, bool blockValue)
        {
            var clubsToUpdate = GetAllClubTeams(clubId, seasonId);
            foreach (var club in clubsToUpdate)
            {
                club.IsBlocked = blockValue;
            }
            _db.SaveChanges();
        }

        public List<ClubTrainingDay> GetClubTrainingDays(int clubId)
        {
            return _db.ClubTrainingDays.Where(c => c.ClubId == clubId && !c.IsArchive).ToList();
        }

        public void AddClubTrainingDays(int clubId, int auditoriumId, string trainingDay, string trainingStartTime, string trainingEndTime)
        {
            try
            {
                var teamTrainingDay = new ClubTrainingDay
                {
                    ClubId = clubId,
                    AuditoriumId = auditoriumId,
                    TrainingDay = trainingDay,
                    TrainingStartTime = trainingStartTime,
                    TrainingEndTime = trainingEndTime
                };
                _db.ClubTrainingDays.Add(teamTrainingDay);
                _db.SaveChanges();
            }
            catch (Exception e)
            {

            }
        }

        public void SetTrainingDayInArchieve(int trainingDayId)
        {
            var trainingDay = _db.ClubTrainingDays.Where(t => t.Id == trainingDayId).FirstOrDefault();
            trainingDay.IsArchive = true;
            _db.SaveChanges();
        }

        public ClubTrainingDay GetLastTrainingDayAdded()
        {
            return _db.ClubTrainingDays.OrderByDescending(c => c.Id).FirstOrDefault();
        }
        public List<ClubTrainingDay> GetAllDaysTrainings(int clubId)
        {
            return _db.ClubTrainingDays.Where(c => c.ClubId == clubId && !c.IsArchive).ToList();
        }

        public void AddPositlionsToTeam(int clubId, int seasonId)
        {
            var teams = GetAllClubTeams(clubId, seasonId).ToList();
            for (int i = 0; i < teams.Count; i++)
            {
                teams[i].TeamPosition = i + 1;
            }
            _db.SaveChanges();
        }

        public void UpdateTeamPositions(int clubId, int seasonId, List<ClubTeamsDTO> positions)
        {
            var clubTeamsPositionsNow = GetAllClubTeams(clubId, seasonId);

            foreach (var cp in clubTeamsPositionsNow)
            {
                foreach (var position in positions)
                {
                    if (cp.TeamId == position.TeamId)
                    {
                        cp.TeamPosition = position.TeamPosition;
                    }
                }
            }
            _db.SaveChanges();
        }

        public IEnumerable<string> GetTrainingDays(int clubId)
        {
            return _db.ClubTrainingDays.Where(c => c.ClubId == 20 && !c.IsArchive).Select(c => c.TrainingDay).AsEnumerable();
        }

        public TrainingSetting GetTrainingSettingsOfTheTeam(int teamId)
        {
            return _db.TrainingSettings.Where(s => s.TeamID == teamId).FirstOrDefault();
        }

        public List<TeamTraining> GetAllClubTrainings(int clubId, int seasonId, bool shouldLoadSchoolTeams = false, bool onlyPublished = false)
        {
            var clubTeamsIds = _db.ClubTeams.Where(c => c.ClubId == clubId && c.SeasonId == seasonId).AsNoTracking().Select(c => c.TeamId).ToList();
            if (shouldLoadSchoolTeams)
            {
                List<int> schoolTeamIds = new List<int>();
                var club = db.Clubs.Find(clubId);
                var schools = club.Schools.Where(x => x.SeasonId == seasonId);
                foreach (var school in schools)
                {
                    schoolTeamIds.AddRange(school.SchoolTeams.Select(y => y.TeamId).ToList());
                }
                clubTeamsIds.AddRange(schoolTeamIds);
            }
            var listOfTrainings = _db.TeamTrainings.Include(x=>x.TrainingAttendances).Where(x => clubTeamsIds.Contains(x.TeamId) && x.SeasonId == seasonId && (!onlyPublished || x.isPublished)
                                                               ).ToList();

            return listOfTrainings;
        }

        public TrainingDaysSetting GetTrainingDaySettingsByTeamIdAndDay(int teamId, string day)
        {
            return _db.TrainingDaysSettings.Where(t => t.TeamId == teamId && t.TrainingDay == day).FirstOrDefault();
        }

        public List<Auditorium> GetAllClubAuditoriums(int? seasonId, int clubId)
        {
            var club = GetClubById(clubId);
            var audQ = db.Auditoriums.Where(a => !a.IsArchive && a.ClubId == clubId);
            if (!(club.IsUnionClub.Value))
            {
                audQ = audQ.Where(a => !seasonId.HasValue || a.SeasonId == seasonId.Value);
            }
            return audQ.OrderBy(a => a.Name).ToList();
        }

        public List<TeamTraining> GetTrainingsInDateRange(int clubId, int seasonId, DateTime? startDate, DateTime? endDate, bool shouldLoadSchoolTeams = false)
        {
            var clubTeamsIds = GetAllClubTeamsIds(clubId, seasonId, shouldLoadSchoolTeams);
                return _db.TeamTrainings
                    .Where(l => clubTeamsIds.Contains(l.TeamId) && l.TrainingDate >= startDate && l.TrainingDate <= endDate && l.SeasonId == seasonId).ToList();
        }

        public List<TeamTraining> GetTrainingsOfFirstDaysOfMonth(int clubId, int seasonId, bool shouldLoadSchoolTeams = false)
        {
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endDate = DateTime.MaxValue;
            var firstWeekList = GetTrainingsInDateRange(clubId, seasonId, startDate, endDate, shouldLoadSchoolTeams);
            return firstWeekList;
        }

        public void PublishToApp(int clubId, int seasonId, bool publishValue)
        {
            var clubTeams = GetAllClubTrainings(clubId, seasonId);
            if (publishValue)//Unpublish
            {
                foreach (var club in clubTeams)
                {
                    club.isPublished = false;
                }
            }
            else
            {
                foreach (var club in clubTeams)
                {
                    club.isPublished = true;
                }
            }
            _db.SaveChanges();
        }

        public void UpdateDatesInTable(List<ExcelTeamTrainingDto> excelTeamTrainings, int clubId, int seasonId)
        {
            var trainingsToUpdate = GetAllClubTrainings(clubId, seasonId);
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
                _db.SaveChanges();
            }
        }

        public void DeleteTrainings(List<int> trainingIds)
        {
            var teamTrainingsToRemove = db.TeamTrainings.Where(x => trainingIds.Contains(x.Id)).ToList();
            if (teamTrainingsToRemove.Any())
            {
                db.TeamTrainings.RemoveRange(teamTrainingsToRemove);
                db.SaveChanges();
            }
        }

        public void SaveTeamTraining(TeamTraining teamTraining)
        {
            db.TeamTrainings.Add(teamTraining);
            db.SaveChanges();
        }

        public void UpdateTeamTrainingFileName(int teamTrainingId, string fileName)
        {
            var teamTraining = db.TeamTrainings.FirstOrDefault(x => x.Id == teamTrainingId);
            if (teamTraining != null)
            {
                teamTraining.TrainingReport = fileName;
            }
            db.SaveChanges();
        }
    }
}
