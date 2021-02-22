using AppModel;
using System;
using System.Collections.Generic;

namespace DataService
{
    public class ExcelGameDto
    {
        public int GameId { get; set; }
        public string League { get; set; }
        public int LeagueId { get; set; }
        public int Stage { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string HomeTeam { get; set; }
        public string HomeCompetitor { get; set; }
        public int HomeTeamId { get; set; }
        public int HomeCompetitorId { get; set; }
        public int HomeTeamScore { get; set; }
        public string GuestTeam { get; set; }
        public string GuestCompetitor { get; set; }
        public int GuestTeamScore { get; set; }
        public int GuestTeamId { get; set; }
        public int GuestCompetitorId { get; set; }
        public string Auditorium { get; set; }
        public int AuditoriumId { get; set; }
        public string Referees { get; set; }
        public string RefereeIds { get; set; }
        public string SpectatorIds { get; set; }
        public string Spectators { get; set; }
        public string DesksIds { get; set; }
        public string DesksNames { get; set; }
        public int CycleNumber { get; set; }
        public string Groupe { get; set; }
        public string Set1 { get; set; }
        public string Set2 { get; set; }
        public string Set3 { get; set; }
        public string Set4 { get; set; }
        public bool IsIndividual { get; set; }
        public bool HomeTeamTechnicalWinner { get; set; }
        public bool GuestTeamTechnicalWinner { get; set; }
        public List<TennisLeagueGame> TennisLeagueGames { get; set; }
        public bool IsTennisLeagueGame { get; set; }
        public string Section { get; set; }
        public string RoundNum { get; set; }
        public int GroupId { get; set; }
        public int CycleNum { get; set; }

    }

    public class ExcelRefereeDto
    {
        public string League { get; set; }
        public DateTime StartDate { get; set; }
        public string HomeTeam { get; set; }
        public string GuestTeam { get; set; }
        public string Auditorium { get; set; }
        public string AuditoriumAddress { get; set; }
        public string Referees { get; set; }
    }

    public class ExcelTeamTrainingDto
    {
        public int TrainingId { get; set; }
        public DateTime TrainingDate { get; set; }
    }
}
