namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewServerIpForSalaryGarbageTool : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ServerIps",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IpAddress = c.String(nullable: false, maxLength: 100),
                        DatabaseUser = c.String(nullable: false, maxLength: 100),
                        DatabasePassword = c.String(nullable: false, maxLength: 500),
                        Description = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.IpAddress, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.ServerIps", new[] { "IpAddress" });
            DropTable("dbo.ServerIps");
        }
    }
}
