using Microsoft.Extensions.Logging;

namespace SqlSugarClient_Demo;

using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
}


public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options
            .UseSqlite("Data Source=blogging.db")
            .LogTo(Console.WriteLine, LogLevel.Information); // 输出SQL到控制台
    }
}
