namespace SignalRSelfHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendFilesystemItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FilesystemItems", "FirstRowAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.FilesystemItems", "LastRowAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FilesystemItems", "LastRowAt");
            DropColumn("dbo.FilesystemItems", "FirstRowAt");
        }
    }
}
