using System.Runtime.Serialization;

namespace WebApi.Models
{
    [DataContract]
    public class ClubListViewModel
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "logo")]
        public string Logo { get; set; }
        [DataMember(Name = "totalFans")]
        public int TotalFans { get; set; }
        [DataMember(Name = "totalTeams")]
        public int TotalTeams { get; set; }
        [DataMember(Name = "totalPlayers")]
        public int TotalPlayers { get; set; }
        public int? seasonid { get; set; }
    }
    public class EventListViewModel
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "logo")]
        public string Logo { get; set; }
        [DataMember(Name = "totalFans")]
        public int TotalFans { get; set; }
        [DataMember(Name = "totalTeams")]
        public int TotalTeams { get; set; }
        [DataMember(Name = "totalPlayers")]
        public int TotalPlayers { get; set; }
        public int? seasonid { get; set; }
        public string Place { get; set; }
        public string Description { get; set; }
        public string Date { get; set; }

    }

    public class BenefitListViewModel
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "logo")]
        public string Logo { get; set; }

        public int? SeasonId { get; set; }
        public string Company { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

    }
}