using System.ComponentModel.DataAnnotations;

namespace RequisitionProject.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Unit { get; set; }

        public decimal UnitPrice { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<RequisitionItem> RequisitionItems { get; set; } = new List<RequisitionItem>();
    }
}
