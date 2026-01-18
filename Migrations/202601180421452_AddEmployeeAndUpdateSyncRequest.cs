namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEmployeeAndUpdateSyncRequest : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.CompanyRequests", newName: "AttandanceSyncRequests");
            DropForeignKey("dbo.CompanyRequests", "UserId", "dbo.Users");
            AddColumn("dbo.AttandanceSyncRequests", "ExternalSyncId", c => c.Int());
            AddColumn("dbo.AttandanceSyncRequests", "IsSuccessful", c => c.Boolean());
            AddColumn("dbo.AttandanceSyncRequests", "FromDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.AttandanceSyncRequests", "ToDate", c => c.DateTime(nullable: false));
            AddForeignKey("dbo.AttandanceSyncRequests", "UserId", "dbo.Users", "Id");

            DropIndex("dbo.AttandanceSyncRequests", "IX_SyncRequests_Status");

            DropColumn("dbo.AttandanceSyncRequests", "Email");
            DropColumn("dbo.AttandanceSyncRequests", "Status");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AttandanceSyncRequests", "Status", c => c.String(nullable: false, maxLength: 5));
            AddColumn("dbo.AttandanceSyncRequests", "Email", c => c.String(nullable: false, maxLength: 255));
            DropForeignKey("dbo.AttandanceSyncRequests", "UserId", "dbo.Users");
            DropColumn("dbo.AttandanceSyncRequests", "ToDate");
            DropColumn("dbo.AttandanceSyncRequests", "FromDate");
            DropColumn("dbo.AttandanceSyncRequests", "IsSuccessful");
            DropColumn("dbo.AttandanceSyncRequests", "ExternalSyncId");
            AddForeignKey("dbo.CompanyRequests", "UserId", "dbo.Users", "Id", cascadeDelete: true);
            RenameTable(name: "dbo.AttandanceSyncRequests", newName: "CompanyRequests");
        }
    }
}
