using System.Data.Entity;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Models.Sync;

namespace AttandanceSyncApp.Models
{
    /// <summary>
    /// DbContext for AttandanceSync database (authentication and sync requests)
    /// </summary>
    public class AuthDbContext : DbContext
    {
        public AuthDbContext() : base("name=AttandanceSyncConnection")
        {
        }

        // Auth entities
        public DbSet<User> Users { get; set; }
        public DbSet<LoginSession> LoginSessions { get; set; }

        // Sync entities
        public DbSet<SyncCompany> Companies { get; set; }
        public DbSet<Tool> Tools { get; set; }
        public DbSet<AttandanceSyncRequest> AttandanceSyncRequests { get; set; }
        public DbSet<DatabaseConfiguration> DatabaseConfigurations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // User - LoginSession relationship (one-to-many)
            modelBuilder.Entity<LoginSession>()
                .HasRequired(ls => ls.User)
                .WithMany(u => u.LoginSessions)
                .HasForeignKey(ls => ls.UserId)
                .WillCascadeOnDelete(true);

            // User - AttandanceSyncRequest relationship (one-to-many)
            modelBuilder.Entity<AttandanceSyncRequest>()
                .HasRequired(sr => sr.User)
                .WithMany(u => u.SyncRequests)
                .HasForeignKey(sr => sr.UserId)
                .WillCascadeOnDelete(false);

            // SyncCompany - AttandanceSyncRequest relationship (one-to-many)
            modelBuilder.Entity<AttandanceSyncRequest>()
                .HasRequired(sr => sr.Company)
                .WithMany(c => c.SyncRequests)
                .HasForeignKey(sr => sr.CompanyId)
                .WillCascadeOnDelete(false);

            // Tool - AttandanceSyncRequest relationship (one-to-many)
            modelBuilder.Entity<AttandanceSyncRequest>()
                .HasRequired(sr => sr.Tool)
                .WithMany(t => t.SyncRequests)
                .HasForeignKey(sr => sr.ToolId)
                .WillCascadeOnDelete(false);

            // AttandanceSyncRequest - LoginSession relationship
            modelBuilder.Entity<AttandanceSyncRequest>()
                .HasRequired(sr => sr.Session)
                .WithMany()
                .HasForeignKey(sr => sr.SessionId)
                .WillCascadeOnDelete(false);

            // AttandanceSyncRequest - DatabaseConfiguration (one-to-many, enforced as 1:1 by unique index)
            // Note: EF6 requires this pattern when FK is not also the PK
            modelBuilder.Entity<DatabaseConfiguration>()
                .HasRequired(dc => dc.Request)
                .WithMany()
                .HasForeignKey(dc => dc.RequestId)
                .WillCascadeOnDelete(true);

            // DatabaseConfiguration - AssignedBy User
            modelBuilder.Entity<DatabaseConfiguration>()
                .HasRequired(dc => dc.AssignedByUser)
                .WithMany(u => u.AssignedConfigurations)
                .HasForeignKey(dc => dc.AssignedBy)
                .WillCascadeOnDelete(false);

            // Unique constraint on User.Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Unique constraint on LoginSession.SessionToken
            modelBuilder.Entity<LoginSession>()
                .HasIndex(ls => ls.SessionToken)
                .IsUnique();

            // Unique constraint on DatabaseConfiguration.RequestId
            modelBuilder.Entity<DatabaseConfiguration>()
                .HasIndex(dc => dc.RequestId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
