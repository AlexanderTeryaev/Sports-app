using System.Collections.Generic;

namespace WebApi.Models
{
    public class RefereeViewModel
    {
        public IList<GameViewModel> ClosedGames { get; set; }
        public GameViewModel CloseGame { get; set; }

        public GameViewModel LiveGame { get; set; }

        public IList<GameViewModel> NextGames { get; set; }
    }
}