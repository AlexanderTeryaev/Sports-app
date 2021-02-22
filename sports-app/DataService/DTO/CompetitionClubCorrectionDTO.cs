using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DTO
{
    public partial class CompetitionClubCorrectionDTO
    {
        public int Id { get; set; }
        public int Rank { get; set; }
        public int LeagueId { get; set; }
        public int SeasonId { get; set; }
        public int ClubId { get; set; }
        public string ClubName { get; set; }
        public int GenderId { get; set; }
        public decimal Correction { get; set; }
        public Nullable<decimal> Points { get; set; }
        public Nullable<int> TypeId { get; set; }
        public Nullable<int> ResultsCounted { get; set; }
    }
}
