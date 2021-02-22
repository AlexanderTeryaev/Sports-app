using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataService;
using DataService.DTO;

namespace WebApi.Models
{

    // Models returned by AccountController actions.

    public class ExternalLoginViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string State { get; set; }
    }

    public class ManageInfoViewModel
    {
        public string LocalLoginProvider { get; set; }

        public string Email { get; set; }

        public IEnumerable<UserLoginInfoViewModel> Logins { get; set; }

        public IEnumerable<ExternalLoginViewModel> ExternalLoginProviders { get; set; }
    }

    public class UserInfoViewModel
    {
        public int Id { get; set; }
        public int? seasonId { get; set; }
        

        public string UserName { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public string Role { get; set; }

        public string Image { get; set; }

        //public List<int> UserJobs { get; set; }
        public List<UserJobDetail> UserJobs { get; set; }

        public IList<TeamDto> Teams { get; set; }
        public bool MCertification { get; set; }
        public bool IsApproved { get; set; }
        public string MCertificationFile { get; set; }
        public bool TenicardValidity { get; set; }
    }

    public class UserJobDetail
    {
        public int UserId { get; set; }
        public int JobId { get; set; }
        public string JobName { get; set; }
        public int JobRoleId { get; set; }
        public string JobRoleName { get; set; }
        public int JobRolePriority { get; set; }
        public int? LeagueId { get; set; }
        public string LeagueName { get; set; }
        public int? TeamId { get; set; }
        public int? ClubId { get; set; }
        public string TeamName { get; set; }
    }

    public class UserLoginInfoViewModel
    {
        public string LoginProvider { get; set; }

        public string ProviderKey { get; set; }
    }

    public class UserDetails
    {
        public string Email { get; set; }

        public string UserName { get; set; }

        //[Required]
        public string FullName { get; set; }
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //public string MiddleName { get; set; }


        public string NewPassword { get; set; }

        [Compare("NewPassword")]
        public string RepeatPassword { get; set; }

        public string OldPassword { get; set; }

        public bool IsStartAlert { get; set; }

        public bool IsTimeChange { get; set; }

        public bool IsGameRecords { get; set; }

        public bool IsGameScores { get; set; }

        public string Language { get; set; }

        public List<TeamCompactViewModel> Teams { get; set; }
    }

}
