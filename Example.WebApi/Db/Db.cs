using Example.WebApi.Model;
using Microsoft.EntityFrameworkCore;


namespace Example.WebApi.Db
{
    public class Db:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data source=./Customer.db");    //创建文件夹的位置        
        }

        public DbSet<Customer> Customer { get; set; }
    }

}