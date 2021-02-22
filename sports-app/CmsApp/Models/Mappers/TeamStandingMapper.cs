using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AppModel;

namespace CmsApp.Models.Mappers
{
    public static class TeamStandingMapper
    {
        public static TeamStandingsForm ToViewModel(this TeamStanding model)
        {
            var viewModel = new TeamStandingsForm
            {
                Id = model.Id,
                Team = model.Team,
                Games = model.Games,
               Home = model.Home,
               Last5 = model.Last5,
               Lost = model.Lost,
               Papf = model.Papf,
               Pts = model.Pts,
               Rank = model.Rank,
               Road = model.Road,
               ScoreRoad = model.ScoreRoad,
               ScoreHome = model.ScoreHome,
               Wins = model.Wins,
               PlusMinusField = model.PlusMinusField
              
            };

            if (Thread.CurrentThread.CurrentUICulture.Name == "he-IL")
            {
                ReverseValues(viewModel, model);
            }

            return viewModel;
        }

        public static List<TeamStandingsForm> ToViewModel(this IList<TeamStanding> model)
        {
            return model.Select(x => x.ToViewModel()).ToList();
        }

        #region Helper

        private static void ReverseValues(TeamStandingsForm viewModel, TeamStanding model)
        {
            viewModel.Home = model.Home.Reverse();
            viewModel.Last5 = model.Last5.Reverse();
            viewModel.Papf = model.Papf.Reverse();
            viewModel.Road = model.Road.Reverse();
            viewModel.ScoreRoad = model.ScoreRoad.Reverse();
            viewModel.ScoreHome = model.ScoreHome.Reverse();
            viewModel.PlusMinusField = model.PlusMinusField.Reverse();
        }
        private static string Reverse(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (input.Contains('/'))
                {
                    var splitted = input.Split('/');
                    return string.Format("{0}/{1}", splitted[1], splitted[0]);
                }
                return input;
            }
            return input;
        }
        #endregion
    }


}