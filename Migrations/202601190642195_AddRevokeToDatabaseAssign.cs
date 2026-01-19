namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRevokeToDatabaseAssign : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DatabaseAssignments", "IsRevoked", c => c.Boolean(nullable: false));
            AddColumn("dbo.DatabaseAssignments", "RevokedAt", c => c.DateTime());
            DropColumn("dbo.CompanyRequests", "IsRevoked");
            DropColumn("dbo.CompanyRequests", "RevokedAt");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CompanyRequests", "RevokedAt", c => c.DateTime());
            AddColumn("dbo.CompanyRequests", "IsRevoked", c => c.Boolean(nullable: false));
            DropColumn("dbo.DatabaseAssignments", "RevokedAt");
            DropColumn("dbo.DatabaseAssignments", "IsRevoked");
        }
    }
}
