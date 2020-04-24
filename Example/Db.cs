using Example.Models;
using Microsoft.EntityFrameworkCore;


namespace Example
{
    public class Db:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data source=./mydb.db");    //创建文件夹的位置        
        }

        public DbSet<OrderHeader> OrderHeader { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
    }
}