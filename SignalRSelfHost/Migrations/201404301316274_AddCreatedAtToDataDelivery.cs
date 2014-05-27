namespace SignalRSelfHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCreatedAtToDataDelivery : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataDeliveryItems", "CreatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataDeliveryItems", "CreatedAt");
        }
    }
}
