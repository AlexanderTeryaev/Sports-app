using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DTO
{
    public class AthleteCompetitionAchievementViewItem
    {
        public int CompetitionDisciplineId { get; set; }
        public int RegistrationId { get; set; }
        public int UserId { get; set; }
        public int? Format { get; set; }
        public string CompetitionName { get; set; }
        public string DisciplineName { get; set; }
        public DateTime? CompetitionStartDate { get; set; }
        public string Heat { get; set; }
        public int? Lane { get; set; }
        public string Result { get; set; }
        public double? Wind { get; set; }
        public int? Rank { get; set; }
        public int? Points { get; set; }
        public int AlternativeResult { get; set; }
        public long? SortValue { get; set; }
        public List<int> RecordId { get; set; }
        public string DisciplineType { get; set; }
        public int SeasonId { get; set; }
    }

    public class AthleteCompetitionAchievementsBySeason
    {
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }
        public List<AthleteCompetitionAchievementViewItem> Achievements { get; set; }
    }
}
