using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogLigFront.Models
{
    public class TennisGameViewModel
    {
        public int GameId { get; set; }
        public int GameNumber { get; set; }

        public string HomePlayerName { get; set; }
        public string GuestPlayerName { get; set; }
        public string HomePlayerPairName { get; set; }
        public string GuestPlayerPairName { get; set; }

        public string Player1Name => string.IsNullOrEmpty(HomePlayerPairName) ? HomePlayerName : $"{HomePlayerName}/{HomePlayerPairName}";
        public string Player2Name => string.IsNullOrEmpty(GuestPlayerPairName) ? GuestPlayerName : $"{GuestPlayerName}/{GuestPlayerPairName}";

        public IEnumerable<TennisLeagueGameScore> Sets { get; set; }
        public int? TechnicalWinnerId { get; internal set; }
        public int? HomePlayerId { get; internal set; }
        public int? GuestPlayerId { get; internal set; }
    }
}