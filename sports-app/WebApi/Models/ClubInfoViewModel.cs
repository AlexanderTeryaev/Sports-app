using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Web;

namespace WebApi.Models
{
    [DataContract (Name = "club")]
    public class ClubInfoViewModel
    {
        [DataMember(Name = "main")]
        public Main Main { get; set; }
        [DataMember(Name = "info")]
        public Info Info { get; set; }
        [DataMember(Name = "officials")]
        public Officials [] Officials { get; set; }
        [DataMember(Name = "teams")]
        public Teams[] Teams { get; set; }
        [DataMember(Name = "tournaments")]
        public Tournaments[] Tournaments { get; set; }
    }

    public class RosterInfoViewModel
    {
        public List<CompactPlayerViewModel> players { get; set; }
        public List<TeamJobsViewModel> teamJobs { get; set; }
    }

    [DataContract]
    public class Main
    {
        [DataMember(Name = "players")]
        public int Players { get; set; }
        [DataMember(Name = "officials")]
        public int Officials { get; set; }
        [DataMember(Name = "fans")]
        public int Fans { get; set; }
    }
    [DataContract]
    public class Info
    {
        [DataMember(Name = "clubname")]
        public string ClubName { get; set; }
        [DataMember(Name = "logo")]
        public string Logo { get; set; }
        [DataMember(Name = "image")]
        public string Image { get; set; }
        [DataMember(Name = "index")]
        public string Index { get; set; }
        [DataMember(Name = "address")]
        public string Address { get; set; }
        [DataMember(Name = "phone")]
        public string Phone { get; set; }
        [DataMember(Name = "email")]
        public string Email { get; set; }
        [DataMember(Name = "termscondition")]
        public string TermsCondition { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "aboutclub")]
        public string AboutClub { get; set; }
    }

    [DataContract]
    public class Officials
    {
        [DataMember(Name = "fullname")]
        public string UserName { get; set; }
        [DataMember(Name = "jobname")]
        public string JobName { get; set; }
    }

    [DataContract]
    public class Teams
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }
        [DataMember(Name = "team")]
        public string Team { get; set; }
        [DataMember(Name = "logo")]
        public string Logo { get; set; }
        [DataMember(Name = "parentid")]
        public int ParentId { get; set; }
        [DataMember(Name = "isschoolteam")]
        public bool IsSchoolTeam { get; set; }
        [DataMember(Name = "players")]
        public int Players { get; set; }
        [DataMember(Name = "fans")]
        public int Fans { get; set; }
        [DataMember(Name = "istrainingteam")]
        public bool IsTrainingTeam { get; set; }
        [DataMember(Name = "isleagueteam")]
        public bool IsLeagueTeam { get; set; }

    }

    [DataContract]
    public class Tournaments
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "ages")]
        public string Ages { get; set; }
        [DataMember(Name = "gender")]
        public string Gender { get; set; }
    }
}