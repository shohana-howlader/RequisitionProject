namespace RequisitionProject.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; } // kg, pcs, ltr etc
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
    }
}
