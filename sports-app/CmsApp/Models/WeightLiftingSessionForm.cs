using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class WeightLiftingSessionForm
    {
        public int Id { get; set; }
        public int CompetitionId { get; set; }
        public int SessionNum { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime WeightStartTime { get; set; }
        public DateTime WeightFinishTime { get; set; }
    }
}