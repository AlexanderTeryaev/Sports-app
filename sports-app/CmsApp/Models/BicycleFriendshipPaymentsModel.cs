using System;
using System.Collections.Generic;
using DataService.Utils;

namespace CmsApp.Models
{
    public class BicycleFriendshipPaymentsModel
    {
        public List<BicycleFriendshipPaymentItem> Payments { get; set; }
    }

    public class BicycleFriendshipPaymentItem
    {
        public string ClubName { get; set; }
        public string TeamName { get; set; }
        public string SeasonName { get; set; }

        public BicycleFriendshipPriceHelper.BicycleFriendshipPrice Prices { get; set; }

        public Guid LogLigPaymentId { get; set; }
        public long? OfficeGuyCustomerId { get; set; }
        public long? OfficeGuyPaymentId { get; set; }
        public int? OfficeGuyDocumentNumber { get; set; }

        public string CreatedByName { get; set; }
        public DateTime DateCreated { get; set; }

        public DateTime? DatePaid { get; set; }

        public bool Discarded { get; set; }
        public string DiscardedByName { get; set; }
        public DateTime? DiscardDate { get; set; }

        public string Comment { get; set; }

        public bool IsManual { get; set; }
    }
}