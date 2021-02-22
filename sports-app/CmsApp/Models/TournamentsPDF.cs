using AppModel;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class TournamentsPDF
    {
        public enum EditType
        {
            LgUnion = 0,
            TmntUnion = 1,
            TmntSectionClub = 2,
            TmntUnionClub = 3
        }

        private const int pdfCount = 4;
        public TournamentsPDF()
        {
            pdfArr = new string[4];
            listLeagues = new List<League>();
            listLevels = new List<CompetitionLevel>();
            listAges = new List<CompetitionAge>();
            listRegions = new List<CompetitionRegion>();
            listAthleticLeagues = new List<AthleticLeague>();
            listDisciplines = new List<SelectListItem>();
            listFriendshipsTypes = new List<SelectListItem>();
            listAgeDisciplines = new Dictionary<int, List<SelectListItem>>();
            listAgeFriendshipsTypes = new Dictionary<int, List<SelectListItem>>();
            listBicycleDisciplinesForSelection = new List<SelectListItem>();
            listExistingBicycleDisciplinesForExpertiseSelection = new Dictionary<int, List<SelectListItem>>();
        }
        public int? UnionId { get; set; }
        public int? DisciplineId { get; set; }
        public int SeasonId { get; set; }
        public int? ClubId { get; set; }
        public EditType Et { get; set; }
        public List<League> listLeagues { get; set; }
        public List<CompetitionLevel> listLevels { get; set; }
        public List<CompetitionAge> listAges { get; set; }
        public List<CompetitionRegion> listRegions { get; set; }
        public List<AthleticLeague> listAthleticLeagues { get; set; }
        public List<SelectListItem> listDisciplines { get; set; }
        public List<SelectListItem> listFriendshipsTypes { get; set; }
        public int SelectedDisciplineId { get; set; }
        public int SelectedFriendshipsTypesId { get; set; }
        public Dictionary<int, List<SelectListItem>> listAgeDisciplines { get; set; }
        public Dictionary<int, List<SelectListItem>> listAgeFriendshipsTypes { get; set; }
        public List<BicycleCompetitionDiscipline> listBicycleDisciplines { get; set; }
        public List<BicycleCompetitionHeat> listBicycleCompetitionHeats { get; set; }
        public List<DisciplineExpertise> listDisciplineExpertise { get; set; }
        public List<SelectListItem> listBicycleDisciplinesForSelection { get; set; }
        public Dictionary<int, List<SelectListItem>> listExistingBicycleDisciplinesForExpertiseSelection { get; set; }

        private string[] pdfArr;
        public string this[int index]
        {
            get
            {
                if (index < 0 && index >= pdfCount)
                {
                    throw new IndexOutOfRangeException();
                }
                return pdfArr[index];
            }
            set
            {
                if (index < 0 || index >= pdfCount)
                {
                    throw new IndexOutOfRangeException();
                }
                pdfArr[index] = value;
            }
        }
        public int Count { get { return pdfCount; } }
        public string Pdf1 { get { return pdfArr[0]; } set { pdfArr[0] = value; } }
        public string Pdf2 { get { return pdfArr[1]; } set { pdfArr[1] = value; } }
        public string Pdf3 { get { return pdfArr[2]; } set { pdfArr[2] = value; } }
        public string Pdf4 { get { return pdfArr[3]; } set { pdfArr[3] = value; } }

        public bool IsDepartment { get; set; }
        public string Section { get; set; }
        public bool IsIndividual { get; set; }
        public IEnumerable<SelectListItem> listClubs { get; set; }
    }
}