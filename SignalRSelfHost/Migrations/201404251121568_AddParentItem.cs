namespace SignalRSelfHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddParentItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FilesystemItems", "ParentItem_Id", c => c.Int());
            CreateIndex("dbo.FilesystemItems", "ParentItem_Id");
            AddForeignKey("dbo.FilesystemItems", "ParentItem_Id", "dbo.FilesystemItems", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FilesystemItems", "ParentItem_Id", "dbo.FilesystemItems");
            DropIndex("dbo.FilesystemItems", new[] { "ParentItem_Id" });
            DropColumn("dbo.FilesystemItems", "ParentItem_Id");
        }
    }
}
