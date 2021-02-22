
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AppModel;

namespace CmsApp.Models.Mappers
{
    public static class TeamScheduleScrapperMapper
    {

        public static TeamScheduleForm ToViewModel(this TeamScheduleScrapper model)
        {
            var vm = new TeamScheduleForm
            {
                Id = model.Id,
                Auditorium = model.Auditorium,
                GuestTeam = model.GuestTeam,
                HomeTeam = model.HomeTeam,
                Score = model.Score,
                StartDate = model.StartDate
            };

            if (Thread.CurrentThread.CurrentUICulture.Name == "he-IL")
            {
                ReverseScoreValues(vm, model);
            }

            return vm;
        }


        public static List<TeamScheduleForm> ToViewModelSorted(this List<TeamScheduleScrapper> model)
        {
            return model.Select(x => x.ToViewModel()).OrderBy(x => x.StartDate).ToList();
        }

        #region Helper

        private static void ReverseScoreValues(TeamScheduleForm viewModel, TeamScheduleScrapper model)
        {
            viewModel.Score = model.Score.Reverse();
        }
        private static string Reverse(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (input.Contains(':'))
                {
                    var splitted = input.Split(':');
                    return string.Format("{0}:{1}", splitted[1], splitted[0]);
                }
                return input;
            }
            return string.Empty;
        }
        #endregion
    }
}