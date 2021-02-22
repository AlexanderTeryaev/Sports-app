using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApi.Models.Tennis
{
    /// <summary>
    /// Model of player to add to CMS
    /// </summary>
    public class PlayersPostRequest
    {
        public string IdNum { get; set; }
        public string PassNum { get; set; }

        public string FullName { get; set; }
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //public string MiddleName { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime BirthDate { get; set; } // dd/mm/yyyy

        [EmailAddress]
        public string Email { get; set; }

        public string City { get; set; }

        public ApiGender Gender { get; set; }

        public int ClubId { get; set; }
    }

    public enum ApiGender
    {
        Female,
        Male
    }
}