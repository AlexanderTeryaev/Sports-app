using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class UnionClubPlayerPrice
    {
        public decimal CompetingRegistrationPrice { get; set; }
        public string CompetingRegistrationCardComProductId { get; set; }

        public decimal RegularRegistrationPrice { get; set; }
        public string RegularRegistrationCardComProductId { get; set; }

        public decimal InsurancePrice { get; set; }
        public string InsuranceCardComProductId { get; set; }

        public decimal TenicardPrice { get; set; }
        public string TenicardCardComProductId { get; set; }
    }
}