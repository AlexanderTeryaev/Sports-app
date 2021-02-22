
namespace CmsApp.Models
{
    public class TeamStandingsForm
    {
        public int Id { get; set; }
        public int Rank { get; set; }
        public string Team { get; set; }
        public byte Games { get; set; }
        public byte Wins { get; set; }
        public byte Lost { get; set; }
        public int Pts { get; set; }
        public string Papf { get; set; }
        public string Home { get; set; }
        public string Road { get; set; }
        public string ScoreHome { get; set; }
        public string ScoreRoad { get; set; }
        public string Last5 { get; set; }
        public int ClubId { get; set; }
        public string GamesUrl { get; set; }
        public string PlusMinusField { get; set; }
    }
}