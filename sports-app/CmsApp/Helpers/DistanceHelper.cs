using DataService;

namespace CmsApp.Helpers
{
    public class DistanceHelper
    {
        private readonly JobsRepo _jobsRepo;

        public DistanceHelper()
        {
            _jobsRepo = new JobsRepo();
        }

        public string GetDistanceBetweenCities(int id, LogicaName logicalName, int? seasonId, string city1Name, string city2Name)
        {
            int distance = _jobsRepo.GetDistanceBetweenCities(id, logicalName, seasonId, city1Name, city2Name, out string distanceType);
            return distance != 0 ? $"{distance} {distanceType}" : string.Empty;
        }
    }
}