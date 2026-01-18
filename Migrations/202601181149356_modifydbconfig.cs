namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modifydbconfig : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DatabaseConfigurations", "RequestId", "dbo.AttandanceSyncRequests");
            DropIndex("dbo.DatabaseConfigurations", new[] { "RequestId" });
            DropIndex("dbo.DatabaseConfigurations", new[] { "AssignedBy" });
            RenameColumn(table: "dbo.DatabaseConfigurations", name: "AssignedBy", newName: "User_Id");
            AddColumn("dbo.DatabaseConfigurations", "CompanyId", c => c.Int(nullable: false));
            AlterColumn("dbo.DatabaseConfigurations", "User_Id", c => c.Int());
            CreateIndex("dbo.DatabaseConfigurations", "CompanyId", unique: true);
            CreateIndex("dbo.DatabaseConfigurations", "User_Id");
            AddForeignKey("dbo.DatabaseConfigurations", "CompanyId", "dbo.Companies", "Id", cascadeDelete: true);
            DropColumn("dbo.DatabaseConfigurations", "RequestId");
            DropColumn("dbo.DatabaseConfigurations", "AssignedAt");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DatabaseConfigurations", "AssignedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.DatabaseConfigurations", "RequestId", c => c.Int(nullable: false));
            DropForeignKey("dbo.DatabaseConfigurations", "CompanyId", "dbo.Companies");
            DropIndex("dbo.DatabaseConfigurations", new[] { "User_Id" });
            DropIndex("dbo.DatabaseConfigurations", new[] { "CompanyId" });
            AlterColumn("dbo.DatabaseConfigurations", "User_Id", c => c.Int(nullable: false));
            DropColumn("dbo.DatabaseConfigurations", "CompanyId");
            RenameColumn(table: "dbo.DatabaseConfigurations", name: "User_Id", newName: "AssignedBy");
            CreateIndex("dbo.DatabaseConfigurations", "AssignedBy");
            CreateIndex("dbo.DatabaseConfigurations", "RequestId", unique: true);
            AddForeignKey("dbo.DatabaseConfigurations", "RequestId", "dbo.AttandanceSyncRequests", "Id", cascadeDelete: true);
        }
    }
}
