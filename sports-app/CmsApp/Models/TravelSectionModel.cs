using DataService;

namespace CmsApp.Models
{
    public class TravelSectionModel
    {
        public int JobId { get; set; }
        public string SelectedDate { get; set; }
        public bool IsAthleticsLeague { get; set; }
        public bool IsIndividualSection { get; set; }
        public TravelInformationDto TravelInformationDto { get; set; }
    }
}