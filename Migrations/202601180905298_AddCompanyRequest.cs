namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCompanyRequest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CompanyRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        EmployeeId = c.Int(nullable: false),
                        CompanyId = c.Int(nullable: false),
                        ToolId = c.Int(nullable: false),
                        SessionId = c.Int(nullable: false),
                        Status = c.String(nullable: false, maxLength: 2),
                        IsCancelled = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.CompanyId)
                .ForeignKey("dbo.Employees", t => t.EmployeeId)
                .ForeignKey("dbo.LoginSessions", t => t.SessionId)
                .ForeignKey("dbo.Tools", t => t.ToolId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.EmployeeId)
                .Index(t => t.CompanyId)
                .Index(t => t.ToolId)
                .Index(t => t.SessionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CompanyRequests", "UserId", "dbo.Users");
            DropForeignKey("dbo.CompanyRequests", "ToolId", "dbo.Tools");
            DropForeignKey("dbo.CompanyRequests", "SessionId", "dbo.LoginSessions");
            DropForeignKey("dbo.CompanyRequests", "EmployeeId", "dbo.Employees");
            DropForeignKey("dbo.CompanyRequests", "CompanyId", "dbo.Companies");
            DropIndex("dbo.CompanyRequests", new[] { "SessionId" });
            DropIndex("dbo.CompanyRequests", new[] { "ToolId" });
            DropIndex("dbo.CompanyRequests", new[] { "CompanyId" });
            DropIndex("dbo.CompanyRequests", new[] { "EmployeeId" });
            DropIndex("dbo.CompanyRequests", new[] { "UserId" });
            DropTable("dbo.CompanyRequests");
        }
    }
}
