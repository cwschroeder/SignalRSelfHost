namespace SignalRSelfHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removedParentItem : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.FilesystemItems", "ParentItem_Id", "dbo.FilesystemItems");
            DropIndex("dbo.FilesystemItems", new[] { "ParentItem_Id" });
            AddColumn("dbo.FilesystemItems", "ParentItemId", c => c.Int(nullable: false));
            DropColumn("dbo.FilesystemItems", "ParentItem_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FilesystemItems", "ParentItem_Id", c => c.Int());
            DropColumn("dbo.FilesystemItems", "ParentItemId");
            CreateIndex("dbo.FilesystemItems", "ParentItem_Id");
            AddForeignKey("dbo.FilesystemItems", "ParentItem_Id", "dbo.FilesystemItems", "Id");
        }
    }
}
