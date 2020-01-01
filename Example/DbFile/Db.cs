using Example.Models;
using Microsoft.EntityFrameworkCore;

namespace Example.DbFile
{
    public class Db:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data source=./DbFile/mydb.db");    //创建文件夹的位置        
        }

        public DbSet<Person> Person { get; set; }
    }
}