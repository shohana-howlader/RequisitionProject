namespace RequisitionProject.Models
{
    public class RequisitionItem
    {
        public int RequisitionItemId { get; set; }
        public int RequisitionId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Purpose { get; set; }
        public string Remarks { get; set; }

        // Navigation Properties
        public Requisition Requisition { get; set; }
        public Product Product { get; set; }
    }

}