namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDatabaseAssign : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DatabaseAssignments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CompanyRequestId = c.Int(nullable: false),
                        AssignedBy = c.Int(nullable: false),
                        DatabaseConfigurationId = c.Int(nullable: false),
                        AssignedAt = c.DateTime(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.AssignedBy)
                .ForeignKey("dbo.CompanyRequests", t => t.CompanyRequestId)
                .ForeignKey("dbo.DatabaseConfigurations", t => t.DatabaseConfigurationId)
                .Index(t => t.CompanyRequestId, unique: true)
                .Index(t => t.AssignedBy)
                .Index(t => t.DatabaseConfigurationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DatabaseAssignments", "DatabaseConfigurationId", "dbo.DatabaseConfigurations");
            DropForeignKey("dbo.DatabaseAssignments", "CompanyRequestId", "dbo.CompanyRequests");
            DropForeignKey("dbo.DatabaseAssignments", "AssignedBy", "dbo.Users");
            DropIndex("dbo.DatabaseAssignments", new[] { "DatabaseConfigurationId" });
            DropIndex("dbo.DatabaseAssignments", new[] { "AssignedBy" });
            DropIndex("dbo.DatabaseAssignments", new[] { "CompanyRequestId" });
            DropTable("dbo.DatabaseAssignments");
        }
    }
}
