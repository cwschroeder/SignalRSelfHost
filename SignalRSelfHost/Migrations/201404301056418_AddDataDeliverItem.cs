namespace SignalRSelfHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDataDeliverItem : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataDeliveryItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Domain = c.Int(nullable: false),
                        University = c.Int(nullable: false),
                        FullPath = c.String(),
                        TimeFromFileName = c.DateTime(nullable: false),
                        TimeFromFileAttribute = c.DateTime(nullable: false),
                        FilesystemItem_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FilesystemItems", t => t.FilesystemItem_Id)
                .Index(t => t.FilesystemItem_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataDeliveryItems", "FilesystemItem_Id", "dbo.FilesystemItems");
            DropIndex("dbo.DataDeliveryItems", new[] { "FilesystemItem_Id" });
            DropTable("dbo.DataDeliveryItems");
        }
    }
}
