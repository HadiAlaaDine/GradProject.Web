namespace GradProject.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class FixDateTimeTypes : DbMigration
    {
        public override void Up()
        {
            // اختياري لكن مُستحسن: تأمين قيم غير NULL قبل تحويل النوع
            Sql("UPDATE dbo.Products   SET CreatedAt = GETUTCDATE() WHERE CreatedAt IS NULL");
            Sql("UPDATE dbo.Categories SET CreatedAt = GETUTCDATE() WHERE CreatedAt IS NULL");
            Sql("UPDATE dbo.CartItems  SET CreatedAt = GETUTCDATE() WHERE CreatedAt IS NULL");

            // توحيد الأنواع إلى DateTime (بدون datetime2)
            AlterColumn("dbo.CartItems", "CreatedAt", c => c.DateTime(nullable: false));
            AlterColumn("dbo.CartItems", "UpdatedAt", c => c.DateTime());
            AlterColumn("dbo.Products", "CreatedAt", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Categories", "CreatedAt", c => c.DateTime(nullable: false));

            // نفس تحسينات القيود على النصوص اللي طلّعها السكافولد عندك
            AlterColumn("dbo.Products", "Name", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.Products", "Description", c => c.String(maxLength: 1000));
            AlterColumn("dbo.Categories", "Name", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.Categories", "Description", c => c.String(maxLength: 1000));
        }

        public override void Down()
        {
            // رجوع للوراء: نعيد datetime2 فقط إن رجعت بالهجرة
            AlterColumn("dbo.Categories", "Description", c => c.String(maxLength: 255));
            AlterColumn("dbo.Categories", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Products", "Description", c => c.String(maxLength: 500));
            AlterColumn("dbo.Products", "Name", c => c.String(nullable: false, maxLength: 150));

            AlterColumn("dbo.Categories", "CreatedAt", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.Products", "CreatedAt", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.CartItems", "UpdatedAt", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.CartItems", "CreatedAt", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
    }
}
