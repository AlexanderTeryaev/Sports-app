using AppModel;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class TennisLeagueViewModel
    {
        public int GameCycleId { get; set; }
        public int LeagueId { get; set; }
        public TennisTeam HomeTeam { get; set; }
        public TennisTeam GuestTeam { get; set; }
        public TennisGameSettings GameSettings { get; set; }
        public List<TennisGameViewModel> Games { get; set; }
    }

    public class TennisGameSettings
    {
        public int? BestOfSets { get; set; }
        public int? NumberOfGames { get; set; }
        public bool PairsAsLastGame { get; set; }
        public int? TechWinHomePoints { get; set; }
        public int? TechWinGuestPoints { get; set; }

    }

    public class TennisTeam
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public List<User> RegisteredPlayersList { get; set; }
    }
}