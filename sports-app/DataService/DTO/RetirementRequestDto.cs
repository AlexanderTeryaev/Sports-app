using System;

namespace DataService.DTO
{
    public class RetirementRequestDto
    {
        public DateTime RequestDate { get; set; }
        public string Reason { get; set; }
        public string DocumentFile { get; set; }

        public bool Approved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? DateApproved { get; set; }
        public string ApproveText { get; set; }
        public int RefundAmount { get; set; }
    }
}
