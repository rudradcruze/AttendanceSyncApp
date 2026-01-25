using System.Data.Entity;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Models
{
    /// <summary>
    /// Dynamic DbContext for connecting to user-assigned databases
    /// Used to query attendance data from assigned databases
    /// </summary>
    public class DynamicDbContext : DbContext
    {
        public DynamicDbContext(string connectionString)
            : base(connectionString)
        {
            // Disable lazy loading for dynamic connections
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }

        // Define DbSets for the tables you expect in the target database
        // These should match the Smart_v4_DS schema
        public DbSet<Company> Companies { get; set; }
        public DbSet<AttandanceSynchronization> AttandanceSynchronizations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
