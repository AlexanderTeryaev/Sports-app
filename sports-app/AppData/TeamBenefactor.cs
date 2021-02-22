//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AppModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class TeamBenefactor
    {
        public TeamBenefactor()
        {
            this.PlayersBenefactorPrices = new HashSet<PlayersBenefactorPrice>();
        }
    
        public int BenefactorId { get; set; }
        public int TeamId { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> PlayerCreditAmount { get; set; }
        public Nullable<bool> FinancingInsurance { get; set; }
        public Nullable<bool> IsApproved { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> ApprovedUserId { get; set; }
        public Nullable<System.DateTime> ApprovedDate { get; set; }
        public Nullable<int> MaximumPlayersFunded { get; set; }
        public string Comment { get; set; }
    
        public virtual Team Team { get; set; }
        public virtual ICollection<PlayersBenefactorPrice> PlayersBenefactorPrices { get; set; }
    }
}
