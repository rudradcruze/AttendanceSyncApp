namespace AttendanceSyncApp.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using AttendanceSyncApp.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<AttendanceSyncApp.Models.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(AttendanceSyncApp.Models.AppDbContext context)
        {
            context.Companies.AddOrUpdate(
                c => c.CompanyId,
                new Company { CompanyId = 1, CompanyName = "ABC Corporation" },
                new Company { CompanyId = 2, CompanyName = "XYZ Industries" }
            );
        }
    }
}
