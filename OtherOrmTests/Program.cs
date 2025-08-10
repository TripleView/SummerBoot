using FreeSql.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SqlSugar;
using SqlSugarClient_Demo;


public class Program
{
    public static async Task Main(string[] args)
    {
        await TestEfCore();
    }

    public static async Task TestEfCore()
    {
        using (var db = new BloggingContext())
        {
            // 创建数据库和表
            db.Database.EnsureCreated();

            // 新增
            db.Blogs.Add(new Blog { Name = "EF Core 入门" });
            db.SaveChanges();

            // 查询
            var blog = db.Blogs.FirstOrDefault();
            Console.WriteLine($"查询到博客：{blog?.Name}");

            // 查询所有
            var blogs = db.Blogs.ToList();
            Console.WriteLine($"当前博客数量：{blogs.Count}");
            var sss = db.Blogs.Skip(1).Take(100).Where(x => x.Name == "abc").ToQueryString();
            var s1 = db.Blogs.Skip(1).Where(x => x.Name == "abc").ToQueryString();
            var s2 = db.Blogs.Take(100).Where(x => x.Name == "abc").ToQueryString();
            var s3 = db.Blogs.Skip(1).Take(100).Distinct().Where(x => x.Name == "abc").FirstOrDefault();

            var s4 = db.Blogs.GroupBy(x => x.Name).ToList();
        }
    }

    public static async Task TestFreeSql()
    {
        IFreeSql fsql = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=document.db")
            .UseMonitorCommand(x =>
            {

                var d = x.CommandText;
            }, (y, z) =>
            {
                var fdsf = y.CommandText;

            })
            .UseAutoSyncStructure(true) //自动同步实体结构到数据库
            .Build(); //请务必定义成 Singleton 单例模式

        var c = fsql.Select<Tag>().Skip(1).Take(100).Where(a => a.Name == "粤语").ToList();
    }

    public static async Task TestSqlSugar()
    {
        //创建数据库对象 (用法和EF Dappper一样通过new保证线程安全)
        SqlSugarClient Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "datasource=demo.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true
            },
            db => {

                db.Aop.OnLogExecuting = (sql, pars) =>
                {

                    //获取原生SQL推荐 5.1.4.63  性能OK
                    Console.WriteLine(UtilMethods.GetNativeSql(sql, pars));

                    //获取无参数化SQL 对性能有影响，特别大的SQL参数多的，调试使用
                    //Console.WriteLine(UtilMethods.GetSqlString(DbType.SqlServer,sql,pars))


                };

                //注意多租户 有几个设置几个
                //db.GetConnection(i).Aop

            });

        //建库
        Db.DbMaintenance.CreateDatabase();//达梦和Oracle不支持建库

        //建表（看文档迁移）
        Db.CodeFirst.InitTables<Student>(); //所有库都支持     

        //查询表的所有
        var list = Db.Queryable<Student>().ToList();

        //插入
        Db.Insertable(new Student() { SchoolId = 1, Name = "jack" }).ExecuteCommand();

        //更新
        Db.Updateable(new Student() { Id = 1, SchoolId = 2, Name = "jack2" }).ExecuteCommand();

        //删除
        Db.Deleteable<Student>().Where(it => it.Id == 1).ExecuteCommand();

        var list2 = Db.Queryable<Student>().Skip(2).Take(3).Where(x => x.Name == "a").OrderBy(x => x.Name).ToList();

        Console.WriteLine("恭喜你已经入门了,后面只需要用到什么查文档就可以了。");
        Console.ReadKey();
    }
}


//实体与数据库结构一样
public class Student
{
    //数据是自增需要加上IsIdentity 
    //数据库是主键需要加上IsPrimaryKey 
    //注意：要完全和数据库一致2个属性
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    public int? SchoolId { get; set; }
    public string? Name { get; set; }
}

class Tag
{
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public string Name { get; set; }

    public int? Parent_id { get; set; }
}