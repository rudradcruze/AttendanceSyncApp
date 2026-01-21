namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveTheRouteUrlFromTool : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Tools", "RouteUrl");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tools", "RouteUrl", c => c.String(maxLength: 200));
        }
    }
}
