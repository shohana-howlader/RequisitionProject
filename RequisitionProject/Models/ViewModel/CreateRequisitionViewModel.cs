using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RequisitionProject.Models.ViewModel
{
    public class CreateRequisitionViewModel
    {
        public string Department { get; set; }
        public string Purpose { get; set; }
        public RequisitionType Type { get; set; }
        public string Remarks { get; set; }

        public List<RequisitionItemViewModel> Items { get; set; } = new List<RequisitionItemViewModel>();

        // For Dropdowns
        public List<SelectListItem> Products { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
    }

    public class RequisitionItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string Purpose { get; set; }
        public string Remarks { get; set; }
    }

    public class ApprovalViewModel
    {
        public int RequisitionId { get; set; }
        public string RequisitionNumber { get; set; }
        public string RequestedBy { get; set; }
        public string Department { get; set; }
        public DateTime RequestDate { get; set; }
        public RequisitionType Type { get; set; }
        public string Purpose { get; set; }

        public List<RequisitionItemViewModel> Items { get; set; } = new List<RequisitionItemViewModel>();

        public ApprovalStatus Decision { get; set; }
        public string ApprovalRemarks { get; set; }
    }

    public class ApprovalDecisionViewModel
    {
        public int RequisitionId { get; set; }
        public string Remarks { get; set; }
    }

    public class RequisitionDetailsViewModel
    {
        public int RequisitionId { get; set; }
        public string RequisitionNumber { get; set; }
        public string RequestedBy { get; set; }
        public string Department { get; set; }
        public DateTime RequestDate { get; set; }
        public RequisitionType Type { get; set; }
        public string Purpose { get; set; }
        public RequisitionStatus Status { get; set; }
        public string Remarks { get; set; }
        public List<RequisitionItemDetailsViewModel> Items { get; set; } = new List<RequisitionItemDetailsViewModel>();
        public List<ApprovalDetailsViewModel> Approvals { get; set; } = new List<ApprovalDetailsViewModel>();
    }

    public class RequisitionItemDetailsViewModel
    {
        public int RequisitionItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Purpose { get; set; }
        public string Remarks { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class ApprovalDetailsViewModel
    {
        public int Level { get; set; }
        public string ApproverName { get; set; }
        public string ApproverRole { get; set; }
        public ApprovalStatus Status { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string Remarks { get; set; }
    }

    public class RequisitionListViewModel
    {
        public int RequisitionId { get; set; }
        public string RequisitionNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestedBy { get; set; }
        public string Department { get; set; }
        public string Purpose { get; set; }
        public RequisitionType Type { get; set; }
        public RequisitionStatus Status { get; set; }
    }

    public class PendingApprovalViewModel
    {
        public int RequisitionId { get; set; }
        public string RequisitionNumber { get; set; }
        public string RequestedBy { get; set; }
        public string Department { get; set; }
        public DateTime RequestDate { get; set; }
        public RequisitionType Type { get; set; }
        public string Purpose { get; set; }
        public int Level { get; set; }
        public string ApproverRole { get; set; }
    }

    public class RequisitionCreateViewModel
    {
        [Required(ErrorMessage = "Department is required")]
        public string Department { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public RequisitionType Type { get; set; }

        [Required(ErrorMessage = "Purpose is required")]
        [StringLength(500, ErrorMessage = "Purpose cannot exceed 500 characters")]
        public string Purpose { get; set; }

        [StringLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters")]
        public string Remarks { get; set; }

        public List<RequisitionItemViewModel> Items { get; set; } = new List<RequisitionItemViewModel>();
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> TypeList { get; set; } = new List<SelectListItem>();
    }

    // Entity Models
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Requisition
    {
        public int RequisitionId { get; set; }
        public string RequisitionNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestedBy { get; set; }
        public string Department { get; set; }
        public string Purpose { get; set; }
        public RequisitionType Type { get; set; }
        public RequisitionStatus Status { get; set; }
        public string Remarks { get; set; }

        public virtual ICollection<RequisitionItem> RequisitionItems { get; set; }
        public virtual ICollection<RequisitionApproval> RequisitionApprovals { get; set; }
    }

    public class RequisitionItem
    {
        public int RequisitionItemId { get; set; }
        public int RequisitionId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Purpose { get; set; }
        public string Remarks { get; set; }

        public virtual Requisition Requisition { get; set; }
        public virtual Product Product { get; set; }
    }

    public class RequisitionApproval
    {
        public int RequisitionApprovalId { get; set; }
        public int RequisitionId { get; set; }
        public int Level { get; set; }
        public string ApproverRole { get; set; }
        public string ApproverName { get; set; }
        public ApprovalStatus Status { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string Remarks { get; set; }

        public virtual Requisition Requisition { get; set; }
    }
}
