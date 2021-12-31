using System;
using Microsoft.EntityFrameworkCore;
using SummerBoot.Test;
using SummerBoot.WebApi;
using SummerBoot.WebApi.Models;

namespace SummerBoot.WebApi
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