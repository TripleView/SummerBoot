using MySql.Data.MySqlClient;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.Generator;
using System.Configuration;
using System.Data.SqlClient;

namespace WebApiExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.WebHost.UseUrls("http://*:5000");
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSummerBoot();
            builder.Services.AddSummerBootFeign(it =>
            {
                it.AddNacos(builder.Configuration);
            });
            var mysqlDbConnectionString = builder.Configuration.GetValue<string>("mysqlDbConnectionString");
            var sqlServerDbConnectionString= builder.Configuration.GetValue<string>("sqlServerDbConnectionString");

            builder.Services.AddSummerBootRepository(it =>
            {
                //添加第一个mysql的链接
                it.AddDatabaseUnit<MySqlConnection, IUnitOfWork1>(mysqlDbConnectionString,
                    x =>
                    {
                        //绑定单个仓储
                        //x.BindRepository<IMysqlCustomerRepository,Customer>();
                        //通过自定义注解批量绑定仓储
                        x.BindRepositoriesWithAttribute<AutoRepository1Attribute>();

                        //绑定数据库生成接口
                        x.BindDbGeneratorType<IDbGenerator1>();
                        //绑定插入前回调
                        x.BeforeInsert += entity =>
                        {
                            if (entity is BaseEntity baseEntity)
                            {
                                baseEntity.CreateOn = DateTime.Now;
                            }
                        };
                        //绑定更新前回调
                        x.BeforeUpdate += entity =>
                        {
                            if (entity is BaseEntity baseEntity)
                            {
                                baseEntity.LastUpdateOn = DateTime.Now;
                            }
                        };
                        //添加自定义类型映射
                        //x.SetParameterTypeMap(typeof(DateTime), DbType.DateTime2);
                        //添加自定义字段映射处理程序
                        //x.SetTypeHandler(typeof(Guid), new GuidTypeHandler());

                    });

                //添加第二个sqlserver的链接
                it.AddDatabaseUnit<SqlConnection, IUnitOfWork2>(sqlServerDbConnectionString,
                    x =>
                    {
                        x.BindRepositoriesWithAttribute<AutoRepository2Attribute>();
                        x.BindDbGeneratorType<IDbGenerator2>();
                    });
            });
            builder.Services.AddSummerBootCache();
            builder.Host.UseNacosConfiguration();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapDefaultControllerRoute();

            app.Run();
        }
    }
}