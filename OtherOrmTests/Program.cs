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
            // �������ݿ�ͱ�
            db.Database.EnsureCreated();

            // ����
            db.Blogs.Add(new Blog { Name = "EF Core ����" });
            db.SaveChanges();

            // ��ѯ
            var blog = db.Blogs.FirstOrDefault();
            Console.WriteLine($"��ѯ�����ͣ�{blog?.Name}");

            // ��ѯ����
            var blogs = db.Blogs.ToList();
            Console.WriteLine($"��ǰ����������{blogs.Count}");
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
            .UseAutoSyncStructure(true) //�Զ�ͬ��ʵ��ṹ�����ݿ�
            .Build(); //����ض���� Singleton ����ģʽ

        var c = fsql.Select<Tag>().Skip(1).Take(100).Where(a => a.Name == "����").ToList();
    }

    public static async Task TestSqlSugar()
    {
        //�������ݿ���� (�÷���EF Dappperһ��ͨ��new��֤�̰߳�ȫ)
        SqlSugarClient Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "datasource=demo.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true
            },
            db => {

                db.Aop.OnLogExecuting = (sql, pars) =>
                {

                    //��ȡԭ��SQL�Ƽ� 5.1.4.63  ����OK
                    Console.WriteLine(UtilMethods.GetNativeSql(sql, pars));

                    //��ȡ�޲�����SQL ��������Ӱ�죬�ر���SQL������ģ�����ʹ��
                    //Console.WriteLine(UtilMethods.GetSqlString(DbType.SqlServer,sql,pars))


                };

                //ע����⻧ �м������ü���
                //db.GetConnection(i).Aop

            });

        //����
        Db.DbMaintenance.CreateDatabase();//���κ�Oracle��֧�ֽ���

        //�������ĵ�Ǩ�ƣ�
        Db.CodeFirst.InitTables<Student>(); //���пⶼ֧��     

        //��ѯ�������
        var list = Db.Queryable<Student>().ToList();

        //����
        Db.Insertable(new Student() { SchoolId = 1, Name = "jack" }).ExecuteCommand();

        //����
        Db.Updateable(new Student() { Id = 1, SchoolId = 2, Name = "jack2" }).ExecuteCommand();

        //ɾ��
        Db.Deleteable<Student>().Where(it => it.Id == 1).ExecuteCommand();

        var list2 = Db.Queryable<Student>().Skip(2).Take(3).Where(x => x.Name == "a").OrderBy(x => x.Name).ToList();

        Console.WriteLine("��ϲ���Ѿ�������,����ֻ��Ҫ�õ�ʲô���ĵ��Ϳ����ˡ�");
        Console.ReadKey();
    }
}


//ʵ�������ݿ�ṹһ��
public class Student
{
    //������������Ҫ����IsIdentity 
    //���ݿ���������Ҫ����IsPrimaryKey 
    //ע�⣺Ҫ��ȫ�����ݿ�һ��2������
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