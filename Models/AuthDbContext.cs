using System.Data.Entity;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Models.SalaryGarbge;

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
        public DbSet<Employee> Employees { get; set; }
        public DbSet<AttandanceSyncRequest> AttandanceSyncRequests { get; set; }
        public DbSet<CompanyRequest> CompanyRequests { get; set; }
        public DbSet<DatabaseConfiguration> DatabaseConfigurations { get; set; }
        public DbSet<DatabaseAssign> DatabaseAssignments { get; set; }
        public DbSet<UserTool> UserTools { get; set; }

        // SalaryGarbge entities
        public DbSet<ServerIp> ServerIps { get; set; }
        public DbSet<DatabaseAccess> DatabaseAccess { get; set; }

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

            // Employee - AttandanceSyncRequest relationship (one-to-many)
            modelBuilder.Entity<AttandanceSyncRequest>()
                .HasRequired(sr => sr.Employee)
                .WithMany(e => e.SyncRequests)
                .HasForeignKey(sr => sr.EmployeeId)
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

            // Company - DatabaseConfiguration (one-to-one or one-to-many depending on business logic)
            // Assuming 1:1 or 1:Many (Company has 1 Config)
            // But DatabaseConfiguration has CompanyId FK.
            // Let's assume standard One-to-Many where Company has Many Configs (or at least 1).
            // But unique index logic suggests 1:1 per company is likely intended or allowed.
            // If strict 1:1, we can enforce it.
            // Model says DatabaseConfiguration has CompanyId.
            modelBuilder.Entity<DatabaseConfiguration>()
                .HasRequired(dc => dc.Company)
                .WithMany() // Or WithOptional() if it was 1:1 reverse navigation
                .HasForeignKey(dc => dc.CompanyId)
                .WillCascadeOnDelete(true);

            // Unique constraint on User.Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Unique constraint on LoginSession.SessionToken
            modelBuilder.Entity<LoginSession>()
                .HasIndex(ls => ls.SessionToken)
                .IsUnique();

            // Unique constraint on DatabaseConfiguration.CompanyId to enforce 1 configuration per company
            modelBuilder.Entity<DatabaseConfiguration>()
                .HasIndex(dc => dc.CompanyId)
                .IsUnique();

            // User - CompanyRequest relationship (one-to-many)
            modelBuilder.Entity<CompanyRequest>()
                .HasRequired(cr => cr.User)
                .WithMany(u => u.CompanyRequests)
                .HasForeignKey(cr => cr.UserId)
                .WillCascadeOnDelete(false);

            // Employee - CompanyRequest relationship (one-to-many)
            modelBuilder.Entity<CompanyRequest>()
                .HasRequired(cr => cr.Employee)
                .WithMany(e => e.CompanyRequests)
                .HasForeignKey(cr => cr.EmployeeId)
                .WillCascadeOnDelete(false);

            // SyncCompany - CompanyRequest relationship (one-to-many)
            modelBuilder.Entity<CompanyRequest>()
                .HasRequired(cr => cr.Company)
                .WithMany(c => c.CompanyRequests)
                .HasForeignKey(cr => cr.CompanyId)
                .WillCascadeOnDelete(false);

            // Tool - CompanyRequest relationship (one-to-many)
            modelBuilder.Entity<CompanyRequest>()
                .HasRequired(cr => cr.Tool)
                .WithMany(t => t.CompanyRequests)
                .HasForeignKey(cr => cr.ToolId)
                .WillCascadeOnDelete(false);

            // CompanyRequest - LoginSession relationship
            modelBuilder.Entity<CompanyRequest>()
                .HasRequired(cr => cr.Session)
                .WithMany()
                .HasForeignKey(cr => cr.SessionId)
                .WillCascadeOnDelete(false);

            // DatabaseAssign - CompanyRequest relationship (one-to-one)
            modelBuilder.Entity<DatabaseAssign>()
                .HasRequired(da => da.CompanyRequest)
                .WithMany()
                .HasForeignKey(da => da.CompanyRequestId)
                .WillCascadeOnDelete(false);

            // DatabaseAssign - User (AssignedBy) relationship
            modelBuilder.Entity<DatabaseAssign>()
                .HasRequired(da => da.AssignedByUser)
                .WithMany()
                .HasForeignKey(da => da.AssignedBy)
                .WillCascadeOnDelete(false);

            // DatabaseAssign - DatabaseConfiguration relationship
            modelBuilder.Entity<DatabaseAssign>()
                .HasRequired(da => da.DatabaseConfiguration)
                .WithMany()
                .HasForeignKey(da => da.DatabaseConfigurationId)
                .WillCascadeOnDelete(false);

            // Unique constraint on DatabaseAssign.CompanyRequestId (one assignment per request)
            modelBuilder.Entity<DatabaseAssign>()
                .HasIndex(da => da.CompanyRequestId)
                .IsUnique();

            // UserTool - User relationship (one-to-many)
            modelBuilder.Entity<UserTool>()
                .HasRequired(ut => ut.User)
                .WithMany(u => u.UserTools)
                .HasForeignKey(ut => ut.UserId)
                .WillCascadeOnDelete(false);

            // UserTool - Tool relationship (one-to-many)
            modelBuilder.Entity<UserTool>()
                .HasRequired(ut => ut.Tool)
                .WithMany(t => t.UserTools)
                .HasForeignKey(ut => ut.ToolId)
                .WillCascadeOnDelete(false);

            // UserTool - AssignedByUser relationship
            modelBuilder.Entity<UserTool>()
                .HasRequired(ut => ut.AssignedByUser)
                .WithMany()
                .HasForeignKey(ut => ut.AssignedBy)
                .WillCascadeOnDelete(false);

            // Composite index on UserTool (UserId, ToolId) for active assignments
            modelBuilder.Entity<UserTool>()
                .HasIndex(ut => new { ut.UserId, ut.ToolId });

            // Unique constraint on ServerIp.IpAddress
            modelBuilder.Entity<ServerIp>()
                .HasIndex(s => s.IpAddress)
                .IsUnique();

            // DatabaseAccess - ServerIp relationship (one-to-many)
            modelBuilder.Entity<DatabaseAccess>()
                .HasRequired(da => da.ServerIp)
                .WithMany()
                .HasForeignKey(da => da.ServerIpId)
                .WillCascadeOnDelete(true);

            // Unique constraint on DatabaseAccess (ServerIpId + DatabaseName)
            modelBuilder.Entity<DatabaseAccess>()
                .HasIndex(da => new { da.ServerIpId, da.DatabaseName })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}