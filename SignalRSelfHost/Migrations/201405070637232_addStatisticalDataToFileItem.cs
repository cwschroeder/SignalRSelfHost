namespace SignalRSelfHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addStatisticalDataToFileItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FilesystemItems", "RemovedRowCount", c => c.Int(nullable: false));
            AddColumn("dbo.FilesystemItems", "ChangedRowCount", c => c.Int(nullable: false));
            AddColumn("dbo.FilesystemItems", "AddedRowCount", c => c.Int(nullable: false));
            AddColumn("dbo.FilesystemItems", "HasManyChanges", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FilesystemItems", "HasManyChanges");
            DropColumn("dbo.FilesystemItems", "AddedRowCount");
            DropColumn("dbo.FilesystemItems", "ChangedRowCount");
            DropColumn("dbo.FilesystemItems", "RemovedRowCount");
        }
    }
}
