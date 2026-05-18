using Microsoft.EntityFrameworkCore;
using System;
using SummerBoot.Test.DbExecute.Common.Models;

namespace SummerBoot.Test.MultiDatabaseMixed.Db
{
    public class SqlServerDb : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = MyConfiguration.GetConfiguration("sqlServerDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("sqlServer connectionString must not be null");
            }

            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Customer> Customer { get; set; }
    }
}