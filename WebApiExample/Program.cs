using MySql.Data.MySqlClient;
using SummerBoot.Core;
using SummerBoot.Repository;
using SummerBoot.Repository.Generator;
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
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSummerBoot();
            
            var mysqlDbConnectionString = builder.Configuration.GetValue<string>("mysqlDbConnectionString");
            var sqlServerDbConnectionString= builder.Configuration.GetValue<string>("sqlServerDbConnectionString");

            builder.Services.AddSummerBootRepository(it =>
            {
                //��ӵ�һ��mysql������
                it.AddDatabaseUnit<MySqlConnection, IUnitOfWork1>(mysqlDbConnectionString,
                    x =>
                    {
                        //�󶨵����ִ�
                        //x.BindRepository<IMysqlCustomerRepository,Customer>();
                        //ͨ���Զ���ע�������󶨲ִ�
                        x.BindRepositorysWithAttribute<AutoRepository1Attribute>();

                        //�����ݿ����ɽӿ�
                        x.BindDbGeneratorType<IDbGenerator1>();
                        //�󶨲���ǰ�ص�
                        x.BeforeInsert += entity =>
                        {
                            if (entity is BaseEntity baseEntity)
                            {
                                baseEntity.CreateOn = DateTime.Now;
                            }
                        };
                        //�󶨸���ǰ�ص�
                        x.BeforeUpdate += entity =>
                        {
                            if (entity is BaseEntity baseEntity)
                            {
                                baseEntity.LastUpdateOn = DateTime.Now;
                            }
                        };
                        //����Զ�������ӳ��
                        //x.SetParameterTypeMap(typeof(DateTime), DbType.DateTime2);
                        //����Զ����ֶ�ӳ�䴦�����
                        //x.SetTypeHandler(typeof(Guid), new GuidTypeHandler());

                    });

                //��ӵڶ���sqlserver������
                it.AddDatabaseUnit<SqlConnection, IUnitOfWork2>(sqlServerDbConnectionString,
                    x =>
                    {
                        x.BindRepositorysWithAttribute<AutoRepository2Attribute>();
                        x.BindDbGeneratorType<IDbGenerator2>();
                    });
            });
            builder.Services.AddSummerBootCache();
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