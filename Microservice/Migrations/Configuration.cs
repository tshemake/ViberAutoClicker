namespace Microservice.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microservice.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<TaskServiceContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(TaskServiceContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            context.Statuses.AddOrUpdate(
                new Status {
                    Id = Guid.Parse("761CF6D7-7B24-48A1-9D98-64F7C4F99B25"),
                    Name = "Pending"
                },
                new Status {
                    Id = Guid.Parse("8EE0B305-4D92-4438-89CE-DD0C8260451D"),
                    Name = "Active"
                },
                new Status {
                    Id = Guid.Parse("D0074B8C-50BF-4631-90EE-9E6C869AD7BF"),
                    Name = "Resume"
                },
                new Status {
                    Id = Guid.Parse("82421000-EA57-44E0-8A2F-D2EC159F8FDE"),
                    Name = "Delivered"
                },
                new Status {
                    Id = Guid.Parse("7E2F259B-8C03-4BA9-8363-5324466C475E"),
                    Name = "Sended"
                },
                new Status {
                    Id = Guid.Parse("09BDBDEE-46EA-451A-8049-4D1390BE8B25"),
                    Name = "Failure"
                },
                new Status {
                    Id = Guid.Parse("6B75DE8A-480E-4396-8368-E4ED2E851E9D"),
                    Name = "Not registered"
                }
            );
        }
    }
}
