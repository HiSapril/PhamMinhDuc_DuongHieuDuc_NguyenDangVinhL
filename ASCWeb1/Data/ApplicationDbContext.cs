using ASC.Model.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASCWeb1.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public virtual DbSet<MasterDataKey> MasterDataKeys { get; set; }
        public virtual DbSet<MasterDataValue> MasterDataValues { get; set; }
        public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<MasterDataKey>()
                .HasKey(c => new { c.PartitionKey, c.RowKey });
            builder.Entity<MasterDataValue>()
                .HasKey(c => new { c.PartitionKey, c.RowKey });
            builder.Entity<ServiceRequest>()
                .HasKey(c => new { c.PartitionKey, c.RowKey });

            // Add global query filter to exclude soft-deleted records
            builder.Entity<MasterDataKey>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<MasterDataValue>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<ServiceRequest>().HasQueryFilter(e => !e.IsDeleted);

            base.OnModelCreating(builder);
        }
    }
}
