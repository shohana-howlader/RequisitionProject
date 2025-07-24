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

    // ViewModels/ApprovalViewModel.cs
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
}
