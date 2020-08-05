# SummerBoot
> 将SpringBoot的先进理念与C#的简洁优雅合二为一，致力于让net core开发变得更简单。

## Getting Started
### Nuget
 你可以运行以下命令在你的项目中安装 SummerBoot。
 
 ```PM> Install-Package SummerBoot```
## 支持框架
net core 3.1

# 如何使用,可参考example项目
## 仓储，只需写接口，实现类由框架动态生成
仓储底层基于dapper,所以支持所有dapper支持的数据库，比如sqlserver，mysql，oracle，pgsql，sqlite等
### 在startup.cs类中注册服务
````
 services.AddSummerBoot();

            services.AddSummerBootRepository(it =>
            {
                it.DbConnectionType = typeof(SqliteConnection);
                it.ConnectionString = "Data source=./mydb.db";
            });
````
### 定义实体，主键上添加key注解
````
 /// <summary>
    /// 订单详情表
    /// </summary>
    public class OrderDetail
    {
        [Key]
        public int Id { set; get; }
        /// <summary>
        /// 订单表ID
        /// </summary>
        public int OrderHeaderId { set; get; }
        /// <summary>
        /// 商品编号
        /// </summary>
        public string ProductNo { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { set; get; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { set; get; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { set; get; }

    }
````

### 编写接口,继承IRepository接口，即可获得单表增删改查能力，包括批量操作，可自定义select，update，delete操作，支持异步同步
````
 [Repository]
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        /// <summary>
        /// 通过会员号获取消费详情
        /// </summary>
        /// <param name="customerNo"></param>
        /// <returns></returns>
        [Select("select od.* from orderHeader oh join orderDetail" +
                " od on oh.id=od.OrderHeaderId where oh.customerNo=@customerNo")]
        Task<List<OrderDetail>> GetOrderDetailByCustomerNoAsync(string customerNo);

        /// <summary>
        /// 分页,通过会员号获取消费详情
        /// </summary>
        /// <param name="customerNo"></param>
        /// <returns></returns>
        [Select("select od.* from orderHeader oh join orderDetail" +
                " od on oh.id=od.OrderHeaderId where oh.customerNo=@customerNo")]
        Task<Page<OrderDetail>> GetOrderDetailByCustomerNoByPageAsync(IPageable pageable,string customerNo);
    }
	 [Repository]
	    public interface IOrderHeaderRepository:IRepository<OrderHeader>
	    {
	        /// <summary>
	        /// 取消订单
	        /// </summary>
	        /// <param name="customerNo"></param>
	        /// <returns></returns>
	        [Update("update orderHeader set state=0 where customerNo=@customerNo")]
	        Task<int> CancelOrderByCustomerNoAsync(string customerNo);
	
	        /// <summary>
	        /// 删库跑路
	        /// </summary>
	        /// <returns></returns>
	        [Delete("delete from orderHeader")]
	        Task DeleteAllOrder();
	    }
````
### 注入即可直接调用

## http调用服务端feign，只需写接口，实现类由框架动态生成

### 在startup.cs类中注册服务
````
 services.AddSummerBoot();

   //添加feign请求拦截器
            services.AddScoped<IRequestInterceptor,MyRequestInterceptor>();
            services.AddSummerBootFeign();
````

### 编写接口,包括get，post(json方式),post(form表单形式),异常处理包括超时重试回退降级等
````
/// <summary>
    /// 会员接口
    /// </summary>
    [FeignClient(name: "CustomerService", url: "http://localhost:5001/", fallBack: typeof(CustomerFallBack))]
    [Polly(retry:3,timeout:2000,retryInterval:1000)]
    public interface ICustomerRepository
    {
        /// <summary>
        /// 更新会员总消费金额
        /// </summary>
        /// <param name="customerNo"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [PostMapping("/Customer/UpdateCustomerAmount")]
        [Headers("Content-Type:application/json", "Accept:application/json", "Charset:utf-8")]
        Task<bool> UpdateCustomerAmount(string customerNo, decimal amount);
    }
	
 [FeignClient(name: "testFeign", url: "http://localhost:5001/")]
     [Polly(retry:3,timeout:2000,retryInterval:1000)]
     public interface IFeignExampleRepository
     {
         /// <summary>
         /// post(json方式)
         /// </summary>
         /// <param name="name"></param>
         /// <param name="dto"></param>
         /// <returns></returns>
         [PostMapping("/demo/TestJson")]
         [Headers("Content-Type:application/json", "Accept:application/json", "Charset:utf-8")]
         Task<int> TestJson(string name,[Body]AddOrderDto dto);
 
         /// <summary>
         /// post(form方式)
         /// </summary>
         /// <param name="dto"></param>
         /// <returns></returns>
         [PostMapping("/demo/TestForm")]
         Task<bool> TestForm([Form]FeignFormDto dto);
 
         /// <summary>
         /// get
         /// </summary>
         /// <param name="name"></param>
         /// <param name="age"></param>
         /// <returns></returns>
         [GetMapping("/demo/GetTest")]
         Task<DateTime> TestGet(string name,int age);
     }
````
### 注入即可直接调用

## 框架中的一些接口
* IInitializing。如果一个类实现了该接口，那么在生成动态代理类,并执行属性注入后,从IOC容器中返回给调用方前，将会执行方法<b>AfterPropertiesSet</b>，可在该方法中执行类的初始化操作。

* IInterceptor。拦截器接口，实现该接口，可自定义拦截器，如有需要，可在注册服务的时候添加拦截器。

* ISerialization。框架中默认的序列化反序列化接口，可替换，默认为json。

* IUnitOfWork。工作单元接口，熟悉工作单元的，可重写该接口，并注入到框架中。

* ICacheManager。缓存管理器接口，可重写该接口实现各种有意思的缓存操作，比如多级缓存。

* IKeyGenerator。缓存键值生成器接口，可重写该接口，实现自定义key生成规则。

* IRedisCacheWriter。实际操作redis的接口，summerBootCache部分的底层核心，可直接属性注入使用。

* IPatternMatcher。校验字符串是否符合某规则的接口。

* IRepository。自定义仓储接口，框架默认底层实现基于dapper，可重写替换。

* IDbFactory。生成IDbConnection的工厂接口，可重写替换。

## 更新记录
* 2020-08-05 feign调用接口返回的状态码如果不是200，则抛出错误。feign里默认的client用的httpClient的超时时长改成3天。修复unitofwork里callback方法里的bug。