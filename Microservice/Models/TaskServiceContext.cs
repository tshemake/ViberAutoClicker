using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Microservice.Models
{
    public class TaskServiceContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public TaskServiceContext() : base("name=TaskServiceContext")
        {
        }

        public DbSet<Status> Statuses { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<History> Histories { get; set; }

        public override async Task<int> SaveChangesAsync()
        {
            var changeSet = ChangeTracker.Entries<TimeTrackingModel>();

            if (changeSet != null)
            {
                DateTime now = DateTime.UtcNow;
                foreach (var entry in changeSet.Where(c => c.State != EntityState.Unchanged))
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Entity.CreatedAt = now;
                            entry.Entity.UpdatedAt = now;
                            break;
                        case EntityState.Modified:
                            entry.Entity.UpdatedAt = now;
                            break;
                    }
                    var histories = GetHistoryForChange(entry);
                    if (histories.Count > 0)
                    {
                        Histories.AddRange(histories);
                    }
                }
            }
            return await base.SaveChangesAsync();
        }

        private List<History> GetHistoryForChange(DbEntityEntry dbEntry)
        {
            List<History> result = new List<History>();
            DateTime changeTime = DateTime.UtcNow;
            TableAttribute tableAttr = dbEntry.Entity.GetType().GetCustomAttributes(typeof(TableAttribute), false).SingleOrDefault() as TableAttribute;
            string tableName = tableAttr != null ? tableAttr.Name : dbEntry.Entity.GetType().Name;
            if (tableName.Equals("Message", StringComparison.CurrentCultureIgnoreCase))
            {
                if (dbEntry.State == EntityState.Modified)
                {
                    var statusId = dbEntry.CurrentValues.GetValue<Guid>("StatusId");
                    if (statusId != null)
                    {
                        result.Add(new History 
                        {
                            TaskId = dbEntry.CurrentValues.GetValue<Guid>("Id"),
                            StatusId = statusId,
                            CreatedAt = changeTime
                        });
                    }
                }
            }
            return result;
        }
    }
}
