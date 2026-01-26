namespace GradProject.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixChanges : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CartItems", "ProductId", "dbo.Products");
            DropForeignKey("dbo.OrderItems", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.OrderItems", "ProductId", "dbo.Products");
            DropIndex("dbo.CartItems", new[] { "ProductId" });
            DropIndex("dbo.OrderItems", new[] { "OrderId" });
            DropIndex("dbo.OrderItems", new[] { "ProductId" });
            AlterColumn("dbo.Products", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.Products", "Description", c => c.String(maxLength: 500));
            AlterColumn("dbo.Categories", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Categories", "Description", c => c.String(maxLength: 255));
            DropTable("dbo.CartItems");
            DropTable("dbo.OrderItems");
            DropTable("dbo.Orders");
            DropTable("dbo.ShippingAddresses");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentMethod = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        ShipFullName = c.String(nullable: false, maxLength: 100),
                        ShipAddress1 = c.String(nullable: false, maxLength: 200),
                        ShipAddress2 = c.String(maxLength: 200),
                        ShipCity = c.String(nullable: false, maxLength: 100),
                        ShipCountry = c.String(nullable: false, maxLength: 100),
                        ShipPhone = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OrderItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CartItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Quantity = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            AlterColumn("dbo.Categories", "Description", c => c.String(maxLength: 1000));
            AlterColumn("dbo.Categories", "Name", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.Products", "Description", c => c.String(maxLength: 1000));
            AlterColumn("dbo.Products", "Name", c => c.String(nullable: false, maxLength: 200));
            CreateIndex("dbo.OrderItems", "ProductId");
            CreateIndex("dbo.OrderItems", "OrderId");
            CreateIndex("dbo.CartItems", "ProductId");
            AddForeignKey("dbo.OrderItems", "ProductId", "dbo.Products", "Id");
            AddForeignKey("dbo.OrderItems", "OrderId", "dbo.Orders", "Id", cascadeDelete: true);
            AddForeignKey("dbo.CartItems", "ProductId", "dbo.Products", "Id", cascadeDelete: true);
        }
    }
}
