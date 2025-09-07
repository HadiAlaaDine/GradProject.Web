namespace GradProject.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrderShippingInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "ShipFullName", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Orders", "ShipAddress1", c => c.String(nullable: false, maxLength: 200));
            AddColumn("dbo.Orders", "ShipAddress2", c => c.String(maxLength: 200));
            AddColumn("dbo.Orders", "ShipCity", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Orders", "ShipCountry", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Orders", "ShipPhone", c => c.String(maxLength: 30));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "ShipPhone");
            DropColumn("dbo.Orders", "ShipCountry");
            DropColumn("dbo.Orders", "ShipCity");
            DropColumn("dbo.Orders", "ShipAddress2");
            DropColumn("dbo.Orders", "ShipAddress1");
            DropColumn("dbo.Orders", "ShipFullName");
        }
    }
}
