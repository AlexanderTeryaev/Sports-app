using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;

namespace CmsApp.Models
{
    public class CompetitionForm
    {
        public CompetitionForm()
        {
            RoutesRanks = Enumerable.Empty<BasicRouteForm>();
        }
        public int CompetitionId { get; set; }
        public string NameOfCompetition { get; set; }
        public string Logo { get; set; }
        public string Image { get; set; }
        public string AboutCompetition { get; set; }
        public string CompetitionStructure { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EndRegistrationDate { get; set; }
        public string DisciplineName { get; set; }
        public IEnumerable<BasicRouteForm> RoutesRanks { get; set; }
        public IEnumerable<BasicRouteForm> RoutesTeamRanks { get; set; }
        
        public int? MaxRegistrations { get; set; }
        public int? MinimumPlayers { get; set; }
        public int? MaximumPlayers { get; set; }
        public DateTime? MaximumAge { get; set; }
        public DateTime? MinimumAge { get; set; }
        public int? TypeId { get; set; }
        public DateTime? StartRegistrationDate { get; internal set; }
        public string Place { get; internal set; }
        public IEnumerable<CompetitionDiscipline> CompetitionDisciplines { get; internal set; }

        public DateTime? StartTeamRegistrationDate { get; set; }
        public DateTime? EndTeamRegistrationDate { get; set; }
        public List<CompetitionExperty> CompetitionExperties { get; set; }
        public string LevelName { get; set; }
        public string TypeName { get; set; }
        public string RegistrationLink { get; set; }
    }
}