using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsApp.Models
{
    public class GameCycleForm
    {
        public int CycleId { get; set; }
        public int? AuditoriumId { get; set; }
        public string RefereeIds { get; set; }
        public List<string> SpectatorIds { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsNotSetYet { get; set; }
        public int? HomeTeamId { get; set; }
        public int? GuestTeamId { get; set; }
        public int? HomeAthleteId { get; set; }
        public int? GuestAthleteId { get; set; }
        public bool IsCathcball { get; set; }
        public string Remark { get; set; }
    }

    public class GameCycleFormFull
    {
        public int CycleId { get; set; }
        public int StageId { get; set; }
        public int CycleNum { get; set; }
        public int LeagueId { get; set; }
        public int StageNum { get; set; }
        public int RoundNum { get; set; }
        public DateTime StartDate { get; set; }
        public int? AuditoriumId { get; set; }
        public int? GuestTeamId { get; set; }
        public int? HomeTeamId { get; set; }
        public int? GuestAthleteId { get; set; }
        public int? HomeAthleteId { get; set; }
        public string RefereeIds { get; set; }
        public string GameStatus { get; set; }
        public int? GroupId { get; set; }
        public IEnumerable<SelectListItem> Groups { get; set; }
        public IEnumerable<SelectListItem> Teams { get; set; }
        public IEnumerable<SelectListItem> Auditoriums { get; set; }
        public IEnumerable<SelectListItem> Referees { get; set; }
    }
}