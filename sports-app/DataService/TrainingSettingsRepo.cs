using AppModel;
using System;
using System.Linq;

namespace DataService
{
    public class TrainingSettingsRepo : BaseRepo
    {
        public void InsertTrainingSettings(TrainingSetting obj)
        {
            try
            {
                var db = new DataEntities();

                TrainingSetting selectedTeamData = db.TrainingSettings.Where(x => x.TeamID == obj.TeamID).FirstOrDefault();

                if (selectedTeamData != null)
                {
                    db.TrainingSettings.Remove(selectedTeamData);
                }

                db.TrainingSettings.Add(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }

        public void InsertTrainingDaysSettings(TrainingDaysSetting obj)
        {
            try
            {
                var db = new DataEntities();

                TrainingDaysSetting selectedDayData = db.TrainingDaysSettings
                    .Where(x => x.TeamId == obj.TeamId && x.AuditoriumId == obj.AuditoriumId && x.TrainingDay == obj.TrainingDay)
                    .FirstOrDefault();

                if (selectedDayData != null)
                {
                    db.TrainingDaysSettings.Remove(selectedDayData);
                }

                db.TrainingDaysSettings.Add(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

            }

        }

        public TrainingSetting getTrainingSettingsData_byTeamID(int teamid)
        {
            var db = new DataEntities();

            TrainingSetting selectedTeamData = db.TrainingSettings.Where(x => x.TeamID == teamid).FirstOrDefault();

            if (selectedTeamData != null)
            {
                return selectedTeamData;
            }
            else
            {
                return new TrainingSetting();
            }
        }


        public TrainingDaysSetting getTrainingDaysSettingsData_byTeamID(int teamid, int audiID, string day)
        {
            var db = new DataEntities();

            TrainingDaysSetting selectedTeamData = db.TrainingDaysSettings
                .Where(x => x.TeamId == teamid && x.TrainingDay == day && x.AuditoriumId == audiID)
                .FirstOrDefault();

            if (selectedTeamData != null)
            {
                return selectedTeamData;
            }
            else
            {
                return new TrainingDaysSetting();
            }
        }
    }
}
