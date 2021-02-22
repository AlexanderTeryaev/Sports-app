using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ActivityFormGetUnionCustomGroupUserModel
    {
        public int Id { get; set; }
        public string IdNum { get; set; }

        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string BirthDate { get; set; }

        public List<ActivityFormUnionCustomGroupLeagueModel> Leagues { get; set; }
    }

    public class ActivityFormUnionCustomGroupLeagueModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal TeamRegistrationPrice { get; set; }
    }
}