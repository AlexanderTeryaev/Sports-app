using AppModel;
using CmsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CmsApp
{
    public static class DateTimeHelper
    {

        /// <summary>
        /// Returns days interval between start day date and end day date
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns>Collection of days interval</returns>
        public static List<DateTime> GetDaysInterval(DateTime startDate, DateTime endDate)
        {
            var datesInterval = new List<DateTime>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                datesInterval.Add(date);
            }
            return datesInterval;
        }

        /// <summary>
        /// Removes holidays from list of dates
        /// </summary>
        /// <param name="daysList"> list of days</param>
        /// <param name="holidays">list of holidays</param>
        /// <returns>list of dates without holidays</returns>
        public static List<TeamTrainingViewModel> RemoveHolidays(this List<TeamTrainingViewModel> teamTrainings, List<DateTime> holidays)
        {
            foreach (var teamTraining in teamTrainings.ToList())
            {
                foreach (var holiday in holidays.ToList())
                {
                    if (teamTraining.TrainingDate.Date == holiday.Date)
                    {
                        teamTrainings.Remove(teamTraining);
                    }
                }
            }
            return teamTrainings;
        }

        public static DayOfWeek ConvertStringDaysToDate(this string date)
        {
            DayOfWeek day = 0;
            switch (date)
            {
                case "Monday":
                    day = DayOfWeek.Monday;
                    break;
                case "Tuesday":
                    day = DayOfWeek.Tuesday;
                    break;
                case "Wednesday":
                    day = DayOfWeek.Wednesday;
                    break;
                case "Thursday":
                    day = DayOfWeek.Thursday;
                    break;
                case "Friday":
                    day = DayOfWeek.Friday;
                    break;
                case "Saturday":
                    day = DayOfWeek.Saturday;
                    break;
                case "Sunday":
                    day = DayOfWeek.Sunday;
                    break;
            }
            return day;
        }

        /// <summary>
        /// Sort datetime list in asc
        /// </summary>
        /// <param name="list"> Date time list</param>
        /// <returns></returns>
        public static List<DateTime> SortAscending(this List<DateTime> list)
        {
            list.Sort((a, b) => a.CompareTo(b));
            return list;
        }

        /// <summary>
        /// <para>Truncates a DateTime to a specified resolution.</para>
        /// <para>A convenient source for resolution is TimeSpan.TicksPerXXXX constants.</para>
        /// </summary>
        /// <param name="date">The DateTime object to truncate</param>
        /// <param name="resolution">e.g. to round to nearest second, TimeSpan.TicksPerSecond</param>
        /// <returns>Truncated DateTime</returns>
        public static DateTime Truncate(this DateTime date, long resolution)
        {
            return new DateTime(date.Ticks - (date.Ticks % resolution), date.Kind);
        }

        public static DateTime TrimSeconds(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0, dt.Kind);
        }
    }
}