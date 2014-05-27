namespace SignalRSelfHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changed_datadeliveryitem : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DataDeliveryItems", "FilesystemItem_Id", "dbo.FilesystemItems");
            DropIndex("dbo.DataDeliveryItems", new[] { "FilesystemItem_Id" });
            AddColumn("dbo.DataDeliveryItems", "FilesystemItemId", c => c.Int(nullable: false));
            DropColumn("dbo.DataDeliveryItems", "FilesystemItem_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DataDeliveryItems", "FilesystemItem_Id", c => c.Int());
            DropColumn("dbo.DataDeliveryItems", "FilesystemItemId");
            CreateIndex("dbo.DataDeliveryItems", "FilesystemItem_Id");
            AddForeignKey("dbo.DataDeliveryItems", "FilesystemItem_Id", "dbo.FilesystemItems", "Id");
        }
    }
}
