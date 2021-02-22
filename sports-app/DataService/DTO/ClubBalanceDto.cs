using System;

namespace DataService.DTO
{
    public class ClubBalanceDto
    {
        public ClubBalanceDto()
        {
            ActionUser = new ActionUser();
        }
        /// <summary>
        /// Row id
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        /// The user that update the balance
        /// </summary>
        public ActionUser ActionUser { get; set; }
        /// <summary>
        /// Income
        /// </summary>
        public decimal? Income { get; set; }
        /// <summary>
        /// Expense
        /// </summary>
        public decimal? Expense { get; set; }
        /// <summary>
        /// Expense / Income comment
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// Total - summary
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// ClubId
        /// </summary>
        public int ClubId { get; set; }
        /// <summary>
        /// SeasonId
        /// </summary>
        public int? SeasonId { get; set; }
        /// <summary>
        /// Time of the action
        /// </summary>
        public DateTime? TimeOfAction { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? IsPdfReport { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? IsPaid { get; set; }
        /// <summary>
        /// LeagueId
        /// </summary>
        public int? LeagueId { get; set; }
        /// <summary>
        /// Reference number - unique id of the row
        /// </summary>
        public int? Reference { get; set; }
        /// <summary>
        /// referenceDate
        /// </summary>
        public DateTime? referenceDate { get; set; }
    }
}
