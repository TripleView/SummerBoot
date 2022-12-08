using System;
using Microsoft.EntityFrameworkCore;
using SummerBoot.Test.MultiDatabaseMixed.Models;

namespace SummerBoot.Test.MultiDatabaseMixed.Db
{
    public class MysqlDb : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("mysql connectionString must not be null");
            }
            optionsBuilder.UseMySQL(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
         
        }
        public DbSet<Customer> Customer { get; set; }
      
    }
}