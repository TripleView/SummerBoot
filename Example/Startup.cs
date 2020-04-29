using Example.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using SqlOnline.Utils;
using SummerBoot.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Example.Feign;
using Microsoft.Data.Sqlite;
using SummerBoot.Feign;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;
using System.IO;
using Microsoft.OpenApi.Models;

namespace Example
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
            //初始化数据库 initDatabase
            using (var database = new Db())    //新增
            {
                database.Database.EnsureDeleted();
                database.Database.EnsureCreated();
            }

            services.AddSummerBoot();

            services.AddSummerBootRepository(it =>
            {
                it.DbConnectionType = typeof(SqliteConnection);
                it.ConnectionString = "Data source=./mydb.db";
            });

            //添加feign请求拦截器
            services.AddScoped<IRequestInterceptor,MyRequestInterceptor>();
            services.AddSummerBootFeign();

            services.AddControllers();

            services.AddSummerBootCache(it =>
            {
                it.UseRedis("129.204.47.226,password=summerBoot,abortConnect=false");
            });

            services.AddMemoryCache();

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseRouting();

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
