namespace SignalRSelfHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tests",
                c => new
                    {
                        Bar = c.Int(nullable: false, identity: true),
                        Foo = c.String(),
                    })
                .PrimaryKey(t => t.Bar);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Tests");
        }
    }
}
