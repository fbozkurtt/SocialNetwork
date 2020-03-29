namespace SocialNetwork.Web.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dateBirthToString : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "DateBirth", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "DateBirth", c => c.DateTime(nullable: false));
        }
    }
}
