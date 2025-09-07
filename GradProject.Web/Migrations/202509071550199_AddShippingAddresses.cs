namespace GradProject.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddShippingAddresses : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ShippingAddresses",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    UserId = c.String(nullable: false, maxLength: 128),
                    FullName = c.String(nullable: false, maxLength: 100),
                    Phone = c.String(nullable: false, maxLength: 20),
                    AddressLine1 = c.String(nullable: false, maxLength: 200),
                    AddressLine2 = c.String(maxLength: 200),
                    City = c.String(nullable: false, maxLength: 100),
                    State = c.String(maxLength: 100),
                    PostalCode = c.String(maxLength: 20),
                    Country = c.String(nullable: false, maxLength: 100),
                    IsDefault = c.Boolean(nullable: false),
                    CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.UserId, t.IsDefault }); // فهرس مفيد للاستعلامات
        }

        public override void Down()
        {
            DropIndex("dbo.ShippingAddresses", new[] { "UserId", "IsDefault" });
            DropTable("dbo.ShippingAddresses");
        }

    }
}
