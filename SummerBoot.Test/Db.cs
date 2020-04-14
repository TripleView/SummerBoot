using Microsoft.EntityFrameworkCore;
using SummerBoot.Test;
using SummerBoot.Test.Models;

namespace SummerBoot.Test
{
    public class Db:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data source=./testDb.db");    //创建文件夹的位置        
        }

        public DbSet<Customer> Customer { get; set; }
        public DbSet<OrderHeader> OrderHeader { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
    }

    public class Db2 : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data source=./testDbAsync.db");    //创建文件夹的位置        
        }

        public DbSet<Customer> Customer { get; set; }
        public DbSet<OrderHeader> OrderHeader { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
    }
}