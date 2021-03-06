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
    
    public partial class PlayersBenefactorPrice
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int TeamId { get; set; }
        public int LeagueId { get; set; }
        public int SeasonId { get; set; }
        public int BenefactorId { get; set; }
        public decimal RegistrationPrice { get; set; }
        public decimal InsurancePrice { get; set; }
    
        public virtual League League { get; set; }
        public virtual Season Season { get; set; }
        public virtual TeamBenefactor TeamBenefactor { get; set; }
        public virtual Team Team { get; set; }
        public virtual User User { get; set; }
    }
}
