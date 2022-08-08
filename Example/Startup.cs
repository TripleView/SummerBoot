using Example.Feign;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SummerBoot.Core;
using SummerBoot.Feign;
using System;
using System.Collections.Generic;

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
            //��ʼ�����ݿ� initDatabase
            //using (var database = new Db())    //����
            //{
            //    database.Database.EnsureDeleted();
            //    database.Database.EnsureCreated();
            //}

            //services.AddSummerBoot();

            //services.AddSummerBootRepository(it =>
            //{
            //    it.DbConnectionType = typeof(SqliteConnection);
            //    it.ConnectionString = "Data source=./mydb.db";
            //});

            services.AddOptions();


            //���feign����������
            services.AddScoped<IRequestInterceptor,MyRequestInterceptor>();
            services.AddSummerBootFeign();

            services.AddControllers();
            services.AddSummerBootCache();
            //services.AddMemoryCache();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "sukcore��̨Api", Version = "v1" });
                var security = new Dictionary<string, IEnumerable<string>>
                    { { "sukcore", new string[] { } }};
                //c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                //{
                //    Description = "JWT��Ȩtokenǰ����Ҫ�����ֶ�Bearer��һ���ո�,��Bearer token",
                //    Name = "Authorization",
                //    In = ParameterLocation.Header,
                //    Type = SecuritySchemeType.ApiKey,
                //    BearerFormat = "JWT",
                //    Scheme = "Bearer"
                //});

                //c.AddSecurityRequirement(new OpenApiSecurityRequirement
                //{
                //    {
                //        new OpenApiSecurityScheme
                //        {
                //            Reference = new OpenApiReference {
                //                Type = ReferenceType.SecurityScheme,
                //                Id = "Bearer"
                //            }
                //        },
                //        new string[] { }
                //    }
                //});

                //// ��ȡxml�ļ���
                //var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //// ��ȡxml�ļ�·��
                //var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                //// ��ӿ�������ע�ͣ�true��ʾ��ʾ������ע��
                //c.IncludeXmlComments(xmlPath, true);
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

            //app.UseAuthorization();
            // ���Swagger�й��м��
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
