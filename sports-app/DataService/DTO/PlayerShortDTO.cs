using AppModel;
using System;

namespace DataService.DTO
{
    public class PlayerShortDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string TeamTitle { get; set; }
        public string IdentNum { get; set; }
        public string IdGender { get; set; }
        public string UserNameAge { get; set; }
    }

    public class CompDiscRegDTO : PlayerShortDTO
    {
        public int RegistrationId { get; set; }
        public int? WeightDeclaration { get; set; }
        public int? Lifting { get; set; }
        public int? Push { get; set; }
        public int? GenterId { get; set; }
        public int? SessionId { get; set; }
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string ClubName { get; set; }
        public decimal? Weight { get; set; }
        public bool IsWeightOk { get; internal set; }
        public string Name { get; set; }
        public int? Score { get; set; }
        public int? TeamId { get; set; }
        public bool IsCoxwain { get; set; }
    }

    public class CompDiscTeamDTO
    {
        public int TeamId { get; set; }
        public int ClubId { get; set; }
        public int CompetitionDisciplineId { get; set; }
        public int? TeamNumber { get; set; }
    }


    public class CompDiscRegRankDTO
    {
        public int RegistrationId { get; set; }
        public int? ClubId { get; set; }
        public int? GenderId { get; set; }
        public int? SessionId { get; set; }
        public string ClubName { get; set; }
        public decimal? Score { get; set; }
        public decimal? Correction { get; set; }
        public long? SortedValue { get; set; }
        public bool isAlternative { get; set; }
    }

    public class ComparableCompDiscRegDTO : CompDiscRegDTO
    {
        public string IdentNum { get; set; }
        public string PassportNum { get; set; }
        public int? ClubId { get; set; }
    }

    public class PlayersBlockadeShortDTO : PlayerShortDTO
    {
        public int BlockadeId { get; set; }
        public int BType { get; set; }
        public DateTime? EndBlockadeDate { get; set; }
        public DateTime? StartDate { get; set; }
    }

    public class TennisPlayerWithPair
    {
        public string Value { get; set; }
        public string Name { get; set; }
    }

    public class ActionUser
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
    }
}
