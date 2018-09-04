namespace Microservice.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Histories",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        TaskId = c.Guid(nullable: false),
                        StatusId = c.Guid(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Status", t => t.StatusId, cascadeDelete: false)
                .ForeignKey("dbo.Messages", t => t.TaskId, cascadeDelete: false)
                .Index(t => t.TaskId)
                .Index(t => t.StatusId);
            
            CreateTable(
                "dbo.Status",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        PhoneNumber = c.String(nullable: false),
                        Content = c.String(nullable: false),
                        StatusId = c.Guid(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Status", t => t.StatusId, cascadeDelete: false)
                .Index(t => t.StatusId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Histories", "TaskId", "dbo.Messages");
            DropForeignKey("dbo.Messages", "StatusId", "dbo.Status");
            DropForeignKey("dbo.Histories", "StatusId", "dbo.Status");
            DropIndex("dbo.Messages", new[] { "StatusId" });
            DropIndex("dbo.Histories", new[] { "StatusId" });
            DropIndex("dbo.Histories", new[] { "TaskId" });
            DropTable("dbo.Messages");
            DropTable("dbo.Status");
            DropTable("dbo.Histories");
        }
    }
}
