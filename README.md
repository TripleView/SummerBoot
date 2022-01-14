# SummerBoot的核心理念
> 将SpringBoot的先进理念与C#的简洁优雅合二为一，声明式编程，专注于”做什么”而不是”如何去做”。在更高层面写代码，更关心的是目标，而不是底层算法实现的过程，SummerBoot,致力于让.net开发变得更简单。

# 加入QQ群反馈建议
群号:799648362

## Getting Started
### Nuget
 你可以运行以下命令在你的项目中安装 SummerBoot。
 
 ```PM> Install-Package SummerBoot```
## 支持框架
net core 3.1,net 5,net 6

## 说明
这是一个全声明式框架，用户只需要声明接口，框架会通过Reflection Emit技术，自动生成接口的实现类。

# SummerBoot 如何操作数据库
底层基于dapper，上层通过模板模式，支持了常见的4种数据库类型（sqlserver，mysql，oracle，sqlite）的增删改查操作,如果有其他数据库需求，可以参考以上4个的源码，给本项目贡献代码，同时基于工作单元模式实现了事务。
## 准备工作
需要自己通过nuget安装相应的数据库依赖包，比如SqlServer的Microsoft.Data.SqlClient，mysql的Mysql.data, oracle的Oracle.ManagedDataAccess.Core

## 首先在startup.cs类中注册服务
````
services.AddSummerBoot();

services.AddSummerBootRepository(it =>
{
    //注册数据库类型，比如SqliteConnection，MySqlConnection,OracleConnection,SqlConnection
    it.DbConnectionType = typeof(SqliteConnection);
    //添加数据库连接字符串
    it.ConnectionString = "Data source=./mydb.db";
});

````
## 定义一个数据库实体类
其中注解大部分来自于系统自带的命名空间System.ComponentModel.DataAnnotations 和 System.ComponentModel.DataAnnotations.Schema，比如表名Table,主键Key,主键自增DatabaseGenerated(DatabaseGeneratedOption.Identity)，列名Column，不映射该字段NotMapped等

````
public class Customer
{
    [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { set; get; }

    public string Name { set; get; }
    
    public int Age { set; get; } = 0;
    
    /// <summary>
    /// 会员号
    /// </summary>
    public string CustomerNo { set; get; }
    
    /// <summary>
    /// 总消费金额
    /// </summary>
    public decimal TotalConsumptionAmount { set; get; }
}
````
## 定义接口，并继承于IBaseRepository，同时添加AutoRepository注解表示让框架自动注册并生成实现类
````
[AutoRepository]
public interface ICustomerRepository : IBaseRepository<Customer>
{
}
````
## 增删改查操作，均支持异步同步

### 1. 查，支持正常查询与分页查询
通过IOC注入获得接口以后，就可以开始查了，支持2种方式。
#### 1.1 IQueryable链式语法查询。
````
var customers= customerRepository.Where(it => it.Age > 5).OrderBy(it => it.Id).Take(10).ToList();
````

#### 1.2 直接在接口里定义方法，并且在方法上加上注解Select,然后在Select里写sql语句
````
[AutoRepository]
public interface ICustomerRepository : IBaseRepository<Customer>
{
    //async
    [Select("select od.productName from customer c join orderHeader oh on c.id=oh.customerid" +
            " join orderDetail od on oh.id=od.OrderHeaderId where c.name=@name")]
    Task<List<CustomerBuyProduct>> QueryAllBuyProductByNameAsync(string name);

    [Select("select * from customer where age>@age order by id")]
    Task<Page<Customer>> GetCustomerByPageAsync(IPageable pageable, int age);

    //sync
    [Select("select od.productName from customer c join orderHeader oh on c.id=oh.customerid" +
            " join orderDetail od on oh.id=od.OrderHeaderId where c.name=@name")]
    List<CustomerBuyProduct> QueryAllBuyProductByName(string name);

    [Select("select * from customer where age>@age order by id")]
    Page<Customer> GetCustomerByPage(IPageable pageable, int age);

}
````
使用方法:

````
var result = await customerRepository.QueryAllBuyProductByNameAsync("testCustomer");

//page
var pageable = new Pageable(1, 10);
var page = customerRepository.GetCustomerByPage(pageable, 5);
````
#### 1.3 注意：方式2查询里的分页支持，方法的返回值由Page这个类包裹，同时方法参数里必须包含 IPageable这个分页参数，sql语句里也要有order by，例如:
````
[Select("select * from customer where age>@age order by id")]
Page<Customer> GetCustomerByPage(IPageable pageable, int age);
````
#### 1.4 接口同时自带了Get方法，通过id获取结果，和GetAll(),获取表里的所有结果集。

### 增
#### 接口自带了Insert方法，可以插入单个实体，或者实体列表，如果实体类的主键名称为Id,且有Key注解，并且是自增的，那么插入后，框架会自动为实体的ID这个字段赋值，值为自增的ID值。
````
var customer = new Customer() { Name = "testCustomer" };
customerRepository.Insert(customer);

var customer2 = new Customer() { Name = "testCustomer2" };
var customer3 = new Customer() { Name = "testCustomer3" };
var customerList = new List<Customer>() { customer2, customer3 };
customerRepository.Insert(customerList);
````
### 删
#### 1.1 接口自带了Delete方法，可以删除单个实体，或者实体列表
````
customerRepository.Delete(customer);

customerRepository.Delete(customerList);
````
#### 1.2 同时还支持基于lambda表达式的删除，返回受影响的行数，例如
````
 var deleteCount = customerRepository.Delete(it => it.Age > 5);
````
### 改
#### 1.1 接口自带了Update方法，可以更新单个实体，或者实体列表
````
customerRepository.Update(customer);

customerRepository.Update(customerList);
````
#### 1.2 同时还支持基于IQueryable链式语法的更新方式，返回受影响的行数，例如
````
var updateCount= customerRepository.Where(it=>it.Name == "testCustomer")
.SetValue(it=>it.Age,5)
.SetValue(it=>it.TotalConsumptionAmount,100)
.ExecuteUpdate();
````

### 事务支持
#### 事务支持，需要在使用的时候注入自定义仓储接口的同时，也注入框架自带的IUnitOfWork接口，用法如下

````
//uow is IUnitOfWork interface
try
{
    uow.BeginTransaction();
    customerRepository.Insert(new Customer() { Name = "testCustomer2" });
    var orderDetail3 = new OrderDetail
    {
        OrderHeaderId = orderHeader.Id,
        ProductName = "ball",
        Quantity = 3
    };
    orderDetailRepository.Insert(orderDetail3);
    uow.Commit();
}
catch (Exception e)
{
    uow.RollBack();
}
````

### 如果有些特殊情况要求自己手写实现类怎么办?
#### 1.1 定义一个接口继承于IBaseRepository，并且在接口中定义自己的方法，注意，此时该接口无需添加AutoRepository注解
````
public interface ICustomCustomerRepository : IBaseRepository<Customer>
{
    Task<List<Customer>> GetCustomersAsync(string name);

    Task<Customer> GetCustomerAsync(string name);

    Task<int> UpdateCustomerNameAsync(string oldName, string newName);

    Task<int> CustomQueryAsync();
}
````
#### 1.2 添加一个实现类，继承于BaseRepository类和自定义的ICustomCustomerRepository接口，实现类添加ManualRepository注解，注解的参数为这个类对应的自定义接口的类型，添加ManualRepository注解的目的是让模块自动将自定义接口和自定义类注册到IOC容器中，后续直接注入使用即可，BaseRepository自带了Execute，QueryFirstOrDefault和QueryList方法，如果要接触更底层的dbConnection进行查询，参考下面的CustomQueryAsync方法，首先OpenDb()，然后查询，查询中一定要带上transaction:dbTransaction这个参数，查询结束以后CloseDb();

````
[ManualRepository(typeof(ICustomCustomerRepository))]
public class CustomCustomerRepository : BaseRepository<Customer>, ICustomCustomerRepository
{
    public CustomCustomerRepository(IUnitOfWork uow, IDbFactory dbFactory, RepositoryOption repositoryOption) : base(uow, dbFactory, repositoryOption)
    {
    }

    public async Task<Customer> GetCustomerAsync(string name)
    {
        var result =
            await this.QueryFirstOrDefaultAsync<Customer>("select * from customer where name=@name", new { name });
        return result;
    }

    public async Task<List<Customer>> GetCustomersAsync(string name)
    {
        var result = await this.QueryListAsync<Customer>("select * from customer where name=@name", new { name });

        return result;
    }

    public async Task<int> UpdateCustomerNameAsync(string oldName, string newName)
    {
        var result = await this.ExecuteAsync("update customer set name=@newName where name=@oldName", new { newName, oldName });
        return result;
    }

    public async Task<int> CustomQueryAsync()
    {
        this.OpenDb();
        var grid = await this.dbConnection.QueryMultipleAsync("select id from customer",transaction:dbTransaction);
        var id = grid.Read().FirstOrDefault()?.id;
        this.CloseDb();
        return id;
    }
}
````

## SummerBoot如何操作http调用，我们使用Feign

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
* IUnitOfWork。工作单元接口，熟悉工作单元的，可重写该接口，并注入到框架中。

* IRepository。自定义仓储接口，框架默认底层实现基于dapper，可重写替换。

* IDbFactory。生成IDbConnection的工厂接口，可重写替换。

## 更新记录
* 2020-08-05 feign调用接口返回的状态码如果不是200，则抛出错误。feign里默认的client用的httpClient的超时时长改成3天。修复unitofwork里callback方法里的bug。