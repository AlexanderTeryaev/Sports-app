using System;
using System.ComponentModel.DataAnnotations;

namespace CmsApp.Models
{
    public class ClubPaymentViewModel
    {
        public int? Id { get; set; }
        public int ClubId { get; set; }
        public decimal Paid { get; set; }
        public DateTime DateOfPayment { get; set; }
    }
}