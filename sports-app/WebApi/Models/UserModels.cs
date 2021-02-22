using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public static class FriendshipStatus
    {

        public const string Yes = "Yes";

        public const string No = "No";

        public const string Pending = "Pending";
    }


    public class UserBaseViewModel
    {
        private string userName { get; set; }
        public int Id { get; set; }

        public string UserName
        {
            get { return UserRole == "players" ? FullName : this.userName; }
            set { this.userName = value; }
        }

        public string Image { get; set; }

        public bool IsFriend { get; set; }

        public string FriendshipStatus { get; set; }

        public bool CanRcvMsg { get; set; }

        public string FullName { get; set; }

        public string UserRole { get; set; }

        public DateTime Timestamp1 { get; set; }

    }
}