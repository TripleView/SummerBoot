using System;
using Microsoft.EntityFrameworkCore;

namespace SummerBoot.Test.SqlServer.Db
{
    public class SqliteDb:DbContext
    {

        private string connectionString;

        public SqliteDb(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
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