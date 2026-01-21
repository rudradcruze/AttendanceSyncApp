namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUnderDevelopmentInTool : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tools", "IsUnderDevelopment", c => c.Boolean(nullable: false));
            AddColumn("dbo.Tools", "RouteUrl", c => c.String(maxLength: 200));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tools", "RouteUrl");
            DropColumn("dbo.Tools", "IsUnderDevelopment");
        }
    }
}
