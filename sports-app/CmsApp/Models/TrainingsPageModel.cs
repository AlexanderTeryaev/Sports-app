using System.Collections.Generic;

namespace CmsApp.Models
{
    public class TrainingsPageModel
    {
        public IEnumerable<TeamTrainingViewModel> TeamTrainings { get; set; }
        public Pager Pager { get; set; }
    }
}