using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SummerBoot.Core;
using System;
using System.Collections.Generic;
using Example.WebApi.Model;
using MySql.Data.MySqlClient;
using Example.WebApi.Repository;

namespace Example.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //添加跨域
            services.AddCors(it => it.AddPolicy("all", policy => policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)));


            services.AddSummerBoot();

            services.AddSummerBootRepository(it =>
            {
                //-----------以下为必填参数---------
                //注册数据库类型，比如SqliteConnection，MySqlConnection,OracleConnection,SqlConnection
                it.DbConnectionType = typeof(MySqlConnection);
                //添加数据库连接字符串
                it.ConnectionString = "Server=localhost;Database=test;User ID=root;Password=123456;AllowLoadLocalInfile=true";
                
                it.AddDatabaseUnit<MySqlConnection, ITestUnitOfWork>("Server=localhost;Database=test;User ID=root;Password=123456;AllowLoadLocalInfile=true",
                    x=>
                    {
                        x.Bind<ICustomerRepository, Customer>();
                    });
            });

            services.AddSummerBootFeign(it => it.AddNacos(Configuration));
          
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "sukcore后台Api", Version = "v1" });
                var security = new Dictionary<string, IEnumerable<string>>
                    { { "sukcore", new string[] { } }};
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "JWT授权token前面需要加上字段Bearer与一个空格,如Bearer token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });

                // 获取xml文件名
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // 获取xml文件路径
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                // 添加控制器层注释，true表示显示控制器注释
                c.IncludeXmlComments(xmlPath, true);
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            
            app.UseCors("all");

            app.UseAuthorization();
            
            // 添加Swagger有关中间件
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Demo v1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
