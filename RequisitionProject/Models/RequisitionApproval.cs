namespace RequisitionProject.Models
{
    public class RequisitionApproval
    {
        public int RequisitionApprovalId { get; set; }
        public int RequisitionId { get; set; }
        public int Level { get; set; } // 1=Department Head, 2=Procurement Officer
        public string ApproverName { get; set; }
        public string ApproverRole { get; set; }
        public ApprovalStatus Status { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string Remarks { get; set; }

        // Navigation Properties
        public Requisition Requisition { get; set; }
    }
    public enum ApprovalStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}