namespace SignalRSelfHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveTest : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.Tests");
        }
        
        public override void Down()
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
    }
}
