using System;
using Microsoft.EntityFrameworkCore;
using SummerBoot.Test.Sqlite.Models;

namespace SummerBoot.Test.Sqlite.Db
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

        public DbSet<NullableTable> NullableTable { get; set; }
        public DbSet<NotNullableTable> NotNullableTable { get; set; }
        public DbSet<GuidModel> GuidModel { get; set; }

        public DbSet<Address> Address { get; set; }
    }
}