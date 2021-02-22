using System;
using System.Collections.Generic;

namespace DataService.DTO
{
    public class HeatDto
    {
        public HeatDto()
        {
            Lanes = new LinkedList<LaneDto>();
        }
        public int Id { get; set; }
        public DateTime? StartTime { get; set; }
        public bool IsFinal { get; set; }
        public string Name { get; set; }
        public int CompetitionDisciplineId { get; set; }
        
        public LinkedList<LaneDto> Lanes { get; set; }
    }
}