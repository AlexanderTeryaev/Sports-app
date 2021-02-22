using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class LeaguePlayerPrice
    {
        public decimal RegistrationPrice { get; set; }
        public string RegistrationPriceCardComProductId { get; set; }

        public decimal InsurancePrice { get; set; }
        public string InsuranceCardComProductId { get; set; }

        public decimal MembersFee { get; set; }
        public string MembersFeeCardComProductId { get; set; }

        public decimal HandlingFee { get; set; }
        public string HandlingFeeCardComProductId { get; set; }

        public int LeagueId { get; set; }
    }
    public class TeamPlayerPrice
    {
        public int? TeamId { get; set; }

        public decimal PlayerRegistrationAndEquipmentPrice { get; set; }
        public string PlayerRegistrationAndEquipmentCardComProductId { get; set; }

        public decimal ParticipationPrice { get; set; }
        public string ParticipationCardComProductId { get; set; }

        public decimal PlayerInsurancePrice { get; set; }
        public string PlayerInsuranceCardComProductId { get; set; }
    }
}