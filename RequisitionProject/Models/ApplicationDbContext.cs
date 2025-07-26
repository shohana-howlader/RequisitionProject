using Microsoft.EntityFrameworkCore;
using RequisitionProject.Models.ViewModel;

namespace RequisitionProject.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Requisition> Requisitions { get; set; }
        public DbSet<RequisitionItem> RequisitionItems { get; set; }
        public DbSet<RequisitionApproval> RequisitionApprovals { get; set; }



        public DbSet<RequisitionDetailsViewModel> RequisitionDetailsViewModels { get; set; }
        public DbSet<RequisitionItemDetailsViewModel> RequisitionItemDetailsViewModels { get; set; }
        public DbSet<ApprovalDetailsViewModel> ApprovalDetailsViewModels { get; set; }
        public DbSet<RequisitionListViewModel> RequisitionListViewModels { get; set; }
        public DbSet<PendingApprovalViewModel> PendingApprovalViewModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure main entities
            modelBuilder.Entity<Requisition>(entity =>
            {
                entity.HasKey(e => e.RequisitionId);
                entity.Property(e => e.RequisitionNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RequestedBy).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Purpose).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Remarks).HasMaxLength(1000);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Unit).HasMaxLength(50);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<RequisitionItem>(entity =>
            {
                entity.HasKey(e => e.RequisitionItemId);
                entity.Property(e => e.Purpose).HasMaxLength(255);
                entity.Property(e => e.Remarks).HasMaxLength(255);

                //entity.HasOne(d => d.Requisition)
                //    .WithMany(p => p.RequisitionItems)
                //    .HasForeignKey(d => d.RequisitionId);

                entity.HasOne(d => d.Product)
                    .WithMany()
                    .HasForeignKey(d => d.ProductId);
            });

            modelBuilder.Entity<RequisitionApproval>(entity =>
            {
                entity.HasKey(e => e.RequisitionApprovalId);
                entity.Property(e => e.ApproverRole).HasMaxLength(100);
                entity.Property(e => e.ApproverName).HasMaxLength(255);
                entity.Property(e => e.Remarks).HasMaxLength(500);

                //entity.HasOne(d => d.Requisition)
                //    .WithMany(p => p.RequisitionApprovals)
                //    .HasForeignKey(d => d.RequisitionId);
            });

            // Configure ViewModels (these are keyless for stored procedure results)
            modelBuilder.Entity<RequisitionDetailsViewModel>().HasNoKey();
            modelBuilder.Entity<RequisitionItemDetailsViewModel>().HasNoKey();
            modelBuilder.Entity<ApprovalDetailsViewModel>().HasNoKey();
            modelBuilder.Entity<RequisitionListViewModel>().HasNoKey();
            modelBuilder.Entity<PendingApprovalViewModel>().HasNoKey();

            // Seed some sample products
            modelBuilder.Entity<Product>().HasData(
    new Product { ProductId = 1, Name = "Laptop", Description = "High-performance laptop", Unit = "Piece", UnitPrice = 1000.00m, IsActive = true },
    new Product { ProductId = 2, Name = "Mouse", Description = "Wireless optical mouse", Unit = "Piece", UnitPrice = 25.00m, IsActive = true },
    new Product { ProductId = 3, Name = "Keyboard", Description = "Mechanical keyboard", Unit = "Piece", UnitPrice = 75.00m, IsActive = true },
    new Product { ProductId = 4, Name = "Monitor", Description = "24-inch LED monitor", Unit = "Piece", UnitPrice = 300.00m, IsActive = true },
    new Product { ProductId = 5, Name = "Printer Paper", Description = "A4 size paper ream", Unit = "Ream", UnitPrice = 10.00m, IsActive = true }
);

        }
    }
}
