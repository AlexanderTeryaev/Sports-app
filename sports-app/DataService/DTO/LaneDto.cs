using System;
using System.Collections.Generic;
using System.Security.AccessControl;

namespace DataService.DTO
{
    public class LaneDto
    {
        public LaneDto()
        {
            PlayersList = new List<PlayerShortDTO>();
        }
        public int Id { get; set; }
        public int Number { get; set; }
        public string SwimmerName { get; set; }
        public string ClubName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string EntryTime { get; set; }
        public string Result { get; set; }
        public int UserId { get; set; }
        public int? ClubId { get; set; }
        public IEnumerable<PlayerShortDTO> PlayersList { get; set; }
    }
}