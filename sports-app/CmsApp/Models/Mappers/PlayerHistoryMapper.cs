using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;

namespace CmsApp.Models.Mappers
{
    public static class PlayerHistoryMapper
    {
        private static PlayerHistoryFormView ToViewModel(this PlayerHistory model)
        {
            var vm = new PlayerHistoryFormView
            {
                Player = model.User.FullName,
                SeasonId = model.SeasonId,
                SeasonName = model.Seasons.Name,
                Team = model.Team.TeamsDetails.FirstOrDefault(x => x.SeasonId == model.SeasonId)?.TeamName ??
                       model.Team.Title,
                OldTeam = model.OldTeam?.TeamsDetails.FirstOrDefault(x => x.SeasonId == model.SeasonId)?.TeamName ??
                          model.OldTeam?.Title,
                Date = new DateTime(model.TimeStamp),
                UserActionName = model.User1?.FullName,
            };

            return vm;
        }

        public static List<PlayerHistoryFormView> ToViewModel(this List<PlayerHistory> model)
        {
            return model.Select(x => x.ToViewModel()).OrderBy(x => x.SeasonId).ToList();
        }
    }
}