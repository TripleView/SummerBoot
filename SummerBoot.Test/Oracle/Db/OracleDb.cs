using Microsoft.EntityFrameworkCore;
using SummerBoot.Test;
using SummerBoot.Test.Oracle.Models;
using System;

namespace SummerBoot.Test.Oracle.Db
{
    public class OracleDb : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = MyConfiguration.GetConfiguration("oracleDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("oracle connectionString must not be null");
            }
            optionsBuilder.UseOracle(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<NullableTable>().HasComment("NullableTable");
            modelBuilder.Entity<NullableTable>().Property(it=>it.Int2).HasComment("Int2");
            modelBuilder.Entity<NullableTable>().Property(it => it.Long2).HasComment("Long2");
            modelBuilder.Entity<NotNullableTable>().HasComment("NotNullableTable");
            modelBuilder.Entity<NotNullableTable>().Property(it => it.Int2).HasComment("Int2");
            modelBuilder.Entity<NotNullableTable>().Property(it => it.Long2).HasComment("Long2");
            modelBuilder.Entity<NullableTable>().Property(it => it.Decimal3).HasPrecision(20, 4);
            modelBuilder.Entity<NotNullableTable>().Property(it => it.Decimal3).HasPrecision(20, 4);
        }
        
        public DbSet<Customer> Customer { get; set; }
        public DbSet<OrderHeader> OrderHeader { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<NullableTable> NullableTable { get; set; }
        public DbSet<NotNullableTable> NotNullableTable { get; set; }
    }
}