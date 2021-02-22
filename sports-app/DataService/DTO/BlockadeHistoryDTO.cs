using System;

namespace DataService.DTO
{
    public class BlockadeHistoryDTO
    {
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UserActionName { get; internal set; }
    }
}
