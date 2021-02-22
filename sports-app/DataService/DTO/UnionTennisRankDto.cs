using System;
using System.Collections.Generic;

namespace DataService.DTO
{
    public class UnionTennisRankDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string TrainingTeam { get; set; }
        public DateTime? Birthday { get; set; }
        public int AveragePoints { get; set; }
        public int PointsToAverage { get; set; }
        public int TotalPoints { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<int> CompetitionPoints { get; set; }
        public List<UnionTennisCompetitionDto> CompetitionList { get; set; }


    }

    public class UnionTennisCompetitionDto
    {
        public int CompetitionId { get; set; }
        public int CompetitionPoints { get; set; }
        public DateTime? StartDate { get; set; }
        public int MinParticipationReq { get; set; }
    }
}
