namespace SignalRSelfHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDiffHtml : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FilesystemItems", "DiffHtml", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.FilesystemItems", "DiffHtml");
        }
    }
}
