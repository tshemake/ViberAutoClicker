using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Database.Models;

namespace Client.Database
{
    public class ViberConfigDbContext : DbContext
    {
        public ViberConfigDbContext(string dataSource)
                  : base(new SQLiteConnection()
                  {
                      ConnectionString = new SQLiteConnectionStringBuilder() { DataSource = dataSource, ForeignKeys = true }.ConnectionString
                  }, true)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Account> Accounts { get; set; }
    }
}
