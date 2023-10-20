using System;
using Microsoft.EntityFrameworkCore;
using SummerBoot.Test.Pgsql.Models;

namespace SummerBoot.Test.Pgsql.Db
{
    public class PgsqlDb : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = MyConfiguration.GetConfiguration("pgsqlDbConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("pgsql connectionString must not be null");
            }
            optionsBuilder.UseNpgsql(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<NullableTable>().HasComment("NullableTable");
            modelBuilder.Entity<NullableTable>().Property("Int2").HasComment("Int2");
            modelBuilder.Entity<NullableTable>().Property(it => it.Long2).HasComment("Long2");
            modelBuilder.Entity<NotNullableTable>().HasComment("NotNullableTable");
            modelBuilder.Entity<NotNullableTable>().Property(it => it.Int2).HasComment("Int2").IsRequired(true);
            modelBuilder.Entity<NotNullableTable>().Property(it => it.Long2).HasComment("Long2").IsRequired(true);
            modelBuilder.Entity<NullableTable>().Property(it => it.Decimal3).HasPrecision(20, 4);
            modelBuilder.Entity<NotNullableTable>().Property(it => it.Decimal3).HasPrecision(20, 4);
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