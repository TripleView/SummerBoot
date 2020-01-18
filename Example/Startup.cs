using Castle.DynamicProxy;
using Example.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SummerBoot.Core;
using SummerBoot.Core.Aop;
using SummerBoot.Core.Aop.Express;

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
            services.AddSummerBoot(
                builder =>
                {
                    builder.AddSummerBootCache(it => { it.UseRedis("129.204.47.226,password=summerBoot"); })
                        .AddSummerBootRepository(it =>
                        {
                            it.DbConnectionType = typeof(SqliteConnection);
                            it.ConnectionString = "Data source=./DbFile/mydb.db";
                        })
                        .RegisterExpression<AStandardInterceptor>(new ExecutionAopExpress("* .*PersonService Insert.*")) //表达式注册
                        .RegisterDefaultAttribute<BStandardInterceptor>() //属性注册
                        .RegisterExpression<BStandardInterceptor>(new WithinAopExpress(typeof(IPersonService)));  //接口或类注册
                }

            );
            services.AddControllers().AddSummerBootMvcExtention();
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }


    public class AStandardInterceptor : StandardInterceptor
    {
        protected override void PostProceed(IInvocation invocation)
        {
            
        }
    }

    public class BStandardInterceptor : StandardInterceptor
    {
        protected override void PostProceed(IInvocation invocation)
        {

        }
    }
}
