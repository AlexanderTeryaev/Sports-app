using System;

namespace CmsApp.Models
{
    public class FriendshipPaymentDialogModel
    {
        public int[] TeamsPlayersIds { get; set; }
        public string Comment { get; set; }
        public DateTime DatePaid { get; set; }

        public bool CanSubmitManualPayment { get; set; }

        public bool Completed { get; set; }
        public bool Error { get; set; }
    }
}