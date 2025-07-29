using System.ComponentModel.DataAnnotations;

namespace RequisitionProject.Models
{
    public class RequisitionApproval
    {
        public int RequisitionApprovalId { get; set; }

        public int RequisitionId { get; set; }

        public int Level { get; set; }

        [StringLength(100)]
        public string ApproverRole { get; set; }

        [StringLength(255)]
        public string ApproverName { get; set; }

        public ApprovalStatus Status { get; set; }

        public DateTime? ApprovalDate { get; set; }

        [StringLength(1000)]
        public string Remarks { get; set; }

        // Navigation Properties
        public virtual Requisition Requisition { get; set; }
    }
    public enum ApprovalStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}