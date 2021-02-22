using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DTO
{
    public class CombinedPlayerRankDto
    {
        public User User { get; set; }
        public string ClubName { get; set; }
        public List<string> Results { get; set; }
        public List<int> Points { get; set; }
        public List<int?> Formats { get; set; }
        public int GenderId { get; set; }
        public List<double?> Winds { get; set; }
        public int SumPoints { get; set; }
    }
}
