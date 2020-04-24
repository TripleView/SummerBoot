using Microsoft.EntityFrameworkCore;
using SummerBoot.WebApi;
using SummerBoot.WebApi.Models;

namespace SummerBoot.WebApi
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