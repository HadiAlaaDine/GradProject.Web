namespace GradProject.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncSchema : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CartItems", "CreatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CartItems", "CreatedAt");
        }
    }
}
