namespace SignalRSelfHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFilesystemAlert : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FilesystemAlerts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Domain = c.Int(nullable: false),
                        University = c.Int(nullable: false),
                        AlertType = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        Committed = c.Boolean(nullable: false),
                        Message = c.String(),
                        UploadConfigJson = c.String(),
                        FilesystemItemId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FilesystemItems", t => t.FilesystemItemId)
                .Index(t => t.FilesystemItemId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FilesystemAlerts", "FilesystemItemId", "dbo.FilesystemItems");
            DropIndex("dbo.FilesystemAlerts", new[] { "FilesystemItemId" });
            DropTable("dbo.FilesystemAlerts");
        }
    }
}
