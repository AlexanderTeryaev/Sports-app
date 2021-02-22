using System.Collections;
using DataService;
using Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class PointEdit
    {
        public string Name { get; set; }
        public int? Points { get; set; }
    }

    public class GroupsForm
    {
        public GroupsForm()
        {
            var numberOfTeamsRange = Enumerable.Range(1, 64).ToList();
            AllowIncomplete = false;
            ScheduleCreated = false;
            GamesTypes = new List<SelectListItem>();
            TeamsList = new List<SelectListItem>();
            SelectedTeamsList = new List<SelectListItem>();
            Types = new List<SelectListItem>();
            GroupsTeams = new List<GroupTeam>();
            PossibleNumberOfCycles = new SelectList(Enumerable.Range(1, 7).ToList());
            PossibleNumberOfTeams = new SelectList(numberOfTeamsRange);
            PointsTypes = new Dictionary<int, string> { { 1, Messages.WithTheirRecords }, { 2, Messages.ResetScores }, { 3, Messages.SetTheScoresManualy }, {4, Messages.SameRecords } };
            AthletesList = new List<SelectListItem>();
            SelectedAthletesList = new List<SelectListItem>();
        }

        public int GroupId { get; set; }
        public int StageId { get; set; }
        public int TypeId { get; set; }
        public int PointId { get; set; }
        public int LeagueId { get; set; }
        public bool FirstStage { get; set; }
        public int? SeasonId { get; set; }
        public bool IsIndividual { get; set; }
        public string Type { get; set; }
        public int? CategoryId { get; set; }

        public bool AllowIncomplete { get; set; }
        public bool JustCreated { get; set; }
        public bool ScheduleCreated { get; set; }
        [Required]
        [Range(1, 64, ErrorMessage = "עליך לבחור מספר מתוך התפריט הנפתח")]
        public int? NumberOfTeams { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        [Range(1, 7, ErrorMessage = "נא להכניס מספר בין 1 ל 7")]
        public int NumberOfCycles { get; set; }
        public IEnumerable<SelectListItem> PossibleNumberOfCycles { get; set; }
        public IEnumerable<SelectListItem> PossibleNumberOfTeams { get; set; }
        public List<SelectListItem> Types { get; set; }
        public int[] Points { get; set; }
        public int?[] IdTeams { get; set; }
        public string[] Names { get; set; }
        public int[] TeamsArr { get; set; }
        public int[] AthletesArr { get; set; }
        public int?[] SelectedTeamsArr { get; set; }
        public string[] SelectedTennisTeamsArr { get; set; }
        public IEnumerable<SelectListItem> GamesTypes { get; set; }
        public Dictionary<int, string> PointsTypes { get; set; }
        public IEnumerable<SelectListItem> TeamsList { get; set; }
        public IEnumerable<SelectListItem> SelectedTeamsList { get; set; }
        public IEnumerable<GroupTeam> GroupsTeams { get; set; }

        public AthletesModel Athtletes { get; set; }
        public string SectionAlias { get; set; }
        public IEnumerable<SelectListItem> AthletesList { get; set; }
        public IEnumerable<SelectListItem> SelectedAthletesList { get; set; }
        public int?[] SelectedAthletesArr { get; set; }

    }
}