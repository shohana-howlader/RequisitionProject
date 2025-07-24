using Microsoft.AspNetCore.Mvc.Rendering;

namespace RequisitionProject.Models
{
    public class Requisition
    {
        public int RequisitionId { get; set; }      
        public string RequisitionNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestedBy { get; set; }
        public string Department { get; set; }
        public string Purpose { get; set; }
        public RequisitionType Type { get; set; } // Regular or Urgent
        public RequisitionStatus Status { get; set; }
        public string Remarks { get; set; }

        // Navigation Properties
        public List<RequisitionItem> Items { get; set; } = new List<RequisitionItem>();
        public List<RequisitionApproval> Approvals { get; set; } = new List<RequisitionApproval>();
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
