using Microsoft.EntityFrameworkCore;
using SummerBoot.Test;
using SummerBoot.Test.OtherDatabase.Models;
using System;

namespace SummerBoot.Test.OtherDatabase.Db
{
    public class SqliteDb:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = MyConfiguration.GetConfiguration("sqliteDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("sqlite connectionString must not be null");
            }
            optionsBuilder.UseSqlite(connectionString);
        }

        public DbSet<Customer> Customer { get; set; }
        public DbSet<OrderHeader> OrderHeader { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
    }
}