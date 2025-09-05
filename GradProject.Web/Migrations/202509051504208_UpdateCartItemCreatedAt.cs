namespace GradProject.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateCartItemCreatedAt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartItems", "UpdatedAt", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.CartItems", "CreatedAt", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CartItems", "CreatedAt", c => c.DateTime(nullable: false));
            DropColumn("dbo.CartItems", "UpdatedAt");
        }
    }
}
