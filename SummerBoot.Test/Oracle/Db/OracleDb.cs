using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
            //modelBuilder.Entity<NullableTable>(entity =>
            //{
            //    entity.HasComment("NullableTable");
            //});
            //    //.HasComment("NullableTable");
            //modelBuilder.Entity<NullableTable>().Property(it=>it.Int2).HasComment("Int2");
            //modelBuilder.Entity<NullableTable>().Property(it => it.Long2).HasComment("Long2");
            //modelBuilder.Entity<NotNullableTable>().HasComment("NotNullableTable");
            //modelBuilder.Entity<NotNullableTable>().Property(it => it.Int2).HasComment("Int2");
            //modelBuilder.Entity<NotNullableTable>().Property(it => it.Long2).HasComment("Long2");
            modelBuilder.Entity<NullableTable>().Property(it => it.Decimal3).HasPrecision(20, 4);
            modelBuilder.Entity<NotNullableTable>().Property(it => it.Decimal3).HasPrecision(20, 4);
            // 对所有实体应用相同的表名和列名规则
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // 将表名设置为大写
                entity.SetTableName(entity.GetTableName().ToUpper());

                // 将列名设置为大写
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName(), entity.GetSchema())).ToUpper());
                }

                // 将主键和索引列名设置为大写
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName().ToUpper());
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index.GetDatabaseName().ToUpper());
                }
            }
        }
        
        public DbSet<Customer> Customer { get; set; }
        public DbSet<OrderHeader> OrderHeader { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<NullableTable> NullableTable { get; set; }
        public DbSet<NotNullableTable> NotNullableTable { get; set; }
        public DbSet<GuidModel> GuidModel { get; set; }

        public DbSet<Address> Address { get; set; }

        public DbSet<PropNullTest> PropNullTest { get; set; }

        public DbSet<PropNullTestItem> PropNullTestItem { get; set; }
    }
}