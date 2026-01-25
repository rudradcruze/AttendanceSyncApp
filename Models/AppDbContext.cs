using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=DefaultConnection")
        {
        }

        /// <summary>
        /// Constructor for dynamic connection strings (external databases)
        /// </summary>
        public AppDbContext(string connectionString) : base(connectionString)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<AttandanceSynchronization> AttandanceSynchronizations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}