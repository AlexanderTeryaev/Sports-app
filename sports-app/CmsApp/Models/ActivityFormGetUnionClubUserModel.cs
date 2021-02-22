using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ActivityFormGetUnionClubUserModel
    {
        public int Id { get; set; }
        public string IdNum { get; set; }

        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string BirthDate { get; set; }

        public bool DisableRegPaymentForExistingClubs { get; set; }

        public List<ActivityFormUnionClubSportCenterModel> SportCenters { get; set; }
        public List<ActivityFormUnionClubClubModel> Clubs { get; set; }
        public List<ActivityFormUnionClubRegionModel> Regions { get; set; }
    }

    public class ActivityFormUnionClubRegionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ActivityFormUnionClubSportCenterModel
    {
        public int Id { get; set; }
        public string Caption { get; set; }
    }

    public class ActivityFormUnionClubClubModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NumberOfCourts { get; set; }
        public string NGONumber { get; set; }
        public int? SportsCenter { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}