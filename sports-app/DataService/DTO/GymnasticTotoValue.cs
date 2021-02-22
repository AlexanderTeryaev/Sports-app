using System;
using System.Collections.Generic;

namespace DataService.DTO
{
    public class GymnasticTotoValue
    {
        public UsersInformation UsersInformation { get; set; }
        public MainInformation MainInformation { get; set; }
        public List<TotoCompetitionUsers> Competitions { get; set; }
        public int CompetitionParticipationCount { get; internal set; }
        public List<int> WeightLiftingCompetitionsIds { get; set; }
        public List<int> AthleticsCompetitionsIds { get; set; }
        
    }

    public class UsersInformation
    {
        public string IdentNum { get; set; }
        public string PassportNum { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public bool Insurance { get; set; }
        public bool MedicalCertificate { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? YearOfBirth => BirthDate?.Year;
        public string Category { get; set; }
        public int? GenderId { get; set; }
        public int UserId { get; set; }
    }

    public class MainInformation
    {
        public string ClubName { get; set; }
        public string ClubNumber { get; set; }
        public string SportCenterNameEng { get; set; }
        public string SportCenterNameHeb { get; set; }
        public string TeamName { get; set; }
        public string ClubDisciplineName { get; set; }
    }

    public class TotoCompetitionShort
    {
        public int? Id { get; set; }
    }

    public class TotoCompetitionUsers : TotoCompetitionShort
    {
        public int? Position { get; set; }
        public int CycleNum { get; set; }
        public int count { get; set; }
        public int GroupId { get; set; }
    }

    public class TotoCompetition : TotoCompetitionShort
    {
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public int ExcelPosition { get; internal set; }
        public bool IsCompetitionNotLeague { get; set; }
        public bool IsDailyCompetition { get; set; }
        public List<int> CategoryIds { get; set; }
    }
}
