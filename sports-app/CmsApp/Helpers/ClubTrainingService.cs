using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;
using CmsApp.Models;

namespace CmsApp.Helpers
{
    public class ClubTrainingService
    {
        //Dic<day, Dic<arena, List<TimeFrame>>>
        private Dictionary<string, Dictionary<int, List<TimeFrame>>> _timeMatrix =
            new Dictionary<string, Dictionary<int, List<TimeFrame>>>();

        public ClubTrainingService(List<ClubTrainingDay> trainingDaySettings)
        {
            foreach (var day in trainingDaySettings.GroupBy(s => s.TrainingDay))
            {
                var trainingsPerDay = new Dictionary<int, List<TimeFrame>>();
                _timeMatrix.Add(day.Key, trainingsPerDay);
                foreach (var arena in trainingDaySettings.GroupBy(s => s.AuditoriumId))
                {
                    var dayArenaSettings = trainingDaySettings
                        .Where(s => s.TrainingDay == day.Key && s.AuditoriumId == arena.Key)
                        .Select(s => new TimeFrame
                        {
                            StartTime = TimeSpan.Parse(s.TrainingStartTime),
                            EndTime = TimeSpan.Parse(s.TrainingEndTime)
                        })
                        .ToList();
                    trainingsPerDay.Add(arena.Key, dayArenaSettings);
                }
            }
            // all time training settings should be added to timeMatrix
        }

        public TeamTrainingViewModel GetTraining(string trainingDay, ClubTeam clubTeam, TrainingSetting trainingSettings, List<TeamsAuditorium> teamArenas)
        {
            var arenaTimeFrame = GetTimeFrame(trainingDay, teamArenas.Select(a => a.AuditoriumId), trainingSettings.DurationTraining);

            if (arenaTimeFrame == null) return null;

            var teamTrainingModel = new TeamTrainingViewModel()
            {
                TeamId = clubTeam.TeamId,
                AuditoriumId = arenaTimeFrame.AuditoriumId,
                TrainingDay = trainingDay.ConvertStringDaysToDate(),
                TrainingStartTime = arenaTimeFrame.TrainingStartTime,
                TrainingEndTime = arenaTimeFrame.TrainingEndTime
            };

            return teamTrainingModel;
        }

        private ArenaTimeFrame GetTimeFrame(string trainingDay, IEnumerable<int> arenaIds, string trainingSettingsDurationTraining)
        {
            var trainingDurationInMinutes = TimeSpan.FromMinutes(Convert.ToDouble(trainingSettingsDurationTraining));
            foreach (var arenaId in arenaIds)
            {
                var timeFrames = _timeMatrix[trainingDay][arenaId];
                foreach (var timeFrame in timeFrames)
                {
                    if (timeFrame.EndTime - timeFrame.StartTime > trainingDurationInMinutes)
                    {
                        var trainingEndTime = timeFrame.StartTime + trainingDurationInMinutes;
                        timeFrame.StartTime = trainingEndTime;
                        return new ArenaTimeFrame
                        {
                            TrainingStartTime = timeFrame.StartTime,
                            TrainingEndTime = trainingEndTime,
                            AuditoriumId = arenaId
                        };
                    }
                }
            }
            
            return null;
        }

        private class ArenaTimeFrame
        {
            public int AuditoriumId { get; set; }
            public TimeSpan TrainingStartTime { get; set; }
            public TimeSpan TrainingEndTime { get; set; }
        }

        private class TimeFrame
        {
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
        }
    }
}