using Microsoft.EntityFrameworkCore;
using RequisitionProject.Models;
using RequisitionProject.Models.ViewModel;

namespace RequisitionProject.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Entity DbSets
        public DbSet<Requisition> Requisitions { get; set; }
        public DbSet<RequisitionItem> RequisitionItems { get; set; }
        public DbSet<RequisitionApproval> RequisitionApprovals { get; set; }
        public DbSet<Product> Products { get; set; }

        // View Model DbSets for stored procedure results
        public DbSet<RequisitionDetailsViewModel> RequisitionDetailsViewModels { get; set; }
        public DbSet<RequisitionItemDetailsViewModel> RequisitionItemDetailsViewModels { get; set; }
        public DbSet<ApprovalDetailsViewModel> ApprovalDetailsViewModels { get; set; }
        public DbSet<RequisitionListViewModel> RequisitionListViewModels { get; set; }
        public DbSet<PendingApprovalViewModel> PendingApprovalViewModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Requisition entity
            modelBuilder.Entity<Requisition>(entity =>
            {
                entity.HasKey(e => e.RequisitionId);
                entity.Property(e => e.RequisitionNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.RequestedBy).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Department).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Purpose).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Remarks).HasMaxLength(1000);
                entity.Property(e => e.Type).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();

                // Configure relationships
                entity.HasMany(r => r.Items)
                      .WithOne(i => i.Requisition)
                      .HasForeignKey(i => i.RequisitionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(r => r.Approvals)
                      .WithOne(a => a.Requisition)
                      .HasForeignKey(a => a.RequisitionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure RequisitionItem entity
            modelBuilder.Entity<RequisitionItem>(entity =>
            {
                entity.HasKey(e => e.RequisitionItemId);
                entity.Property(e => e.Purpose).HasMaxLength(500);
                entity.Property(e => e.Remarks).HasMaxLength(1000);

                entity.HasOne(i => i.Product)
                      .WithMany()
                      .HasForeignKey(i => i.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure RequisitionApproval entity
            modelBuilder.Entity<RequisitionApproval>(entity =>
            {
                entity.HasKey(e => e.RequisitionApprovalId);
                entity.Property(e => e.ApproverRole).HasMaxLength(100);
                entity.Property(e => e.ApproverName).HasMaxLength(255);
                entity.Property(e => e.Remarks).HasMaxLength(1000);
                entity.Property(e => e.Status).HasConversion<int>();
            });

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Unit).HasMaxLength(50);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            });

            // Configure View Models for stored procedure results (these are keyless entities)
            modelBuilder.Entity<RequisitionDetailsViewModel>().HasNoKey().ToView(null);
            modelBuilder.Entity<RequisitionItemDetailsViewModel>().HasNoKey().ToView(null);
            modelBuilder.Entity<ApprovalDetailsViewModel>().HasNoKey().ToView(null);
            modelBuilder.Entity<RequisitionListViewModel>().HasNoKey().ToView(null);
            modelBuilder.Entity<PendingApprovalViewModel>().HasNoKey().ToView(null);
        }
    }
}