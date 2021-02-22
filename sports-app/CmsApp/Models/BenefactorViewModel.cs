using AppModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class BenefactorViewModel
    {
        public BenefactorViewModel() { }

        public BenefactorViewModel(TeamBenefactor benefactor)
            : this()
        {
            if (benefactor != null)
            {
                this.TeamId = benefactor.TeamId;
                this.BenefactorId = benefactor.BenefactorId;
                this.Name = benefactor.Name;
                this.PlayerCreditAmount = benefactor.PlayerCreditAmount;
                this.MaximumPlayersFunded = benefactor.MaximumPlayersFunded;
                this.FinancingInsurance = benefactor.FinancingInsurance;
                this.IsApproved = benefactor.IsApproved;
                this.Comment = benefactor.Comment;
            }
        }
        public int TeamId { get; set; }
        public int BenefactorId { get; set; }
        [Required]
        public string Name { get; set; }
        public decimal? PlayerCreditAmount { get; set; }
        public int? MaximumPlayersFunded { get; set; }
        public bool? FinancingInsurance { get; set; }
        public bool? IsApproved { get; set; }

        public int? SeasonId { get; set; }
        public int? LeagueId { get; set; }
        public int? UnionId { get; set; }
        public bool IsEilatTournament { get; set; }
        public string Comment { get; set; }
    }
}