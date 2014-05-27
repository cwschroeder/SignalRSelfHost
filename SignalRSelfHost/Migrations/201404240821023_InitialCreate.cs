namespace SignalRSelfHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FilesystemItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Domain = c.Int(nullable: false),
                        University = c.Int(nullable: false),
                        FullPath = c.String(),
                        Size = c.Long(nullable: false),
                        CsvLines = c.Int(),
                        CreatedAt = c.DateTime(nullable: false),
                        FileType = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        LastCheckedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FilesystemItems");
        }
    }
}
