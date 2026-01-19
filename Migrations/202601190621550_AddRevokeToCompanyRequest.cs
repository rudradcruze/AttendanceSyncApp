namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRevokeToCompanyRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyRequests", "IsRevoked", c => c.Boolean(nullable: false));
            AddColumn("dbo.CompanyRequests", "RevokedAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CompanyRequests", "RevokedAt");
            DropColumn("dbo.CompanyRequests", "IsRevoked");
        }
    }
}
