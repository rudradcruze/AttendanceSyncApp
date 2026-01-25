namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDatabaseAccess : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DatabaseAccess",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ServerIpId = c.Int(nullable: false),
                        DatabaseName = c.String(nullable: false, maxLength: 255),
                        HasAccess = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ServerIps", t => t.ServerIpId, cascadeDelete: true)
                .Index(t => new { t.ServerIpId, t.DatabaseName }, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DatabaseAccess", "ServerIpId", "dbo.ServerIps");
            DropIndex("dbo.DatabaseAccess", new[] { "ServerIpId", "DatabaseName" });
            DropTable("dbo.DatabaseAccess");
        }
    }
}
