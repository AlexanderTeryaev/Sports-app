using System;
using System.Collections.Generic;
using DataService.DTO;

namespace CmsApp.Models
{
    public class MovePlayerForm
    {
        #region Constructor

        public MovePlayerForm()
        {
            Teams = new List<TeamDto>();
        }
        #endregion

        public bool CopyPlayers { get; set; }
        public string TeamLeagueClub { get; set; }
        public List<TeamDto> Teams { get; set; }
        public int TeamId { get; set; }
        public int CurrentTeamId { get; set; }
        public int? CurrentLeagueId { get; set; }
        public int[] Players { get; set; }
        public int SeasonId { get; set; }
        public int? ClubId { get; set; }
        public int? UnionId { get; set; }
        public bool HasAccess { get; set; }
        public bool IsBlockade { get; set; }
        public bool IsExeptional { get; set; }
        public DateTime? BlockadeEndDate { get; set; }
        public bool? IsIndividual { get; set; }
        
        public bool IgnoreDifferentClubs { get; set; }
    }
}