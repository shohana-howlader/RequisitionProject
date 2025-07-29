using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RequisitionProject.Models
{
    public class Requisition
    {
        public int RequisitionId { get; set; }

        [Required]
        [StringLength(50)]
        public string RequisitionNumber { get; set; }

        public DateTime RequestDate { get; set; }

        [Required]
        [StringLength(255)]
        public string RequestedBy { get; set; }

        [Required]
        [StringLength(255)]
        public string Department { get; set; }

        [Required]
        [StringLength(500)]
        public string Purpose { get; set; }

        public RequisitionType Type { get; set; }

        public RequisitionStatus Status { get; set; }

        [StringLength(1000)]
        public string Remarks { get; set; }

        // Navigation Properties
        public virtual ICollection<RequisitionItem> Items { get; set; } = new List<RequisitionItem>();
        public virtual ICollection<RequisitionApproval> Approvals { get; set; } = new List<RequisitionApproval>();
    }
    public enum RequisitionType
    {
        Regular = 1,
        Urgent = 2
    }

    public enum RequisitionStatus
    {
        Draft = 1,
        Submitted = 2,
        PartiallyApproved = 3,
        Approved = 4,
        Rejected = 5
    }




}
