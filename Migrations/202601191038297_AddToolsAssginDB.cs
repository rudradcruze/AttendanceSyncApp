namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddToolsAssginDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserTools",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ToolId = c.Int(nullable: false),
                        AssignedBy = c.Int(nullable: false),
                        AssignedAt = c.DateTime(nullable: false),
                        IsRevoked = c.Boolean(nullable: false),
                        RevokedAt = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.AssignedBy)
                .ForeignKey("dbo.Tools", t => t.ToolId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => new { t.UserId, t.ToolId })
                .Index(t => t.AssignedBy);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserTools", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserTools", "ToolId", "dbo.Tools");
            DropForeignKey("dbo.UserTools", "AssignedBy", "dbo.Users");
            DropIndex("dbo.UserTools", new[] { "AssignedBy" });
            DropIndex("dbo.UserTools", new[] { "UserId", "ToolId" });
            DropTable("dbo.UserTools");
        }
    }
}
