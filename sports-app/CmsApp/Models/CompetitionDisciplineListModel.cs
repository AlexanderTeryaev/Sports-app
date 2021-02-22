using System.Collections.Generic;
using AppModel;
using DataService.DTO;

namespace CmsApp.Models
{
    public class CompetitionDisciplineListModel
    {
        public string SectionName { get; set; }
        public List<CompetitionDisciplineDto> Disciplines { get; set; }
        public List<WeightLiftingSession> Sessions { get; internal set; }
        public int CompetitionId { get; set; }
        public List<League> ListLeagues { get; internal set; }
        public List<CompetitionExperty> CompetitionExperties { get; set; }

    }
}