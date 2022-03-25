[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/dotnetcore/CAP/master/LICENSE.txt)

# 感谢jetbrain提供的ide许可证
<a href="https://jb.gg/OpenSourceSupport"> <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png?_ga=2.140768178.1037783001.1644161957-503565267.1643800664&_gl=1*1rs8z57*_ga*NTAzNTY1MjY3LjE2NDM4MDA2NjQ.*_ga_V0XZL7QHEB*MTY0NDE2MTk1Ny4zLjEuMTY0NDE2NTE2Mi4w" width = "200" height = "200" alt="" align=center /></a>

# SummerBoot的核心理念
> 将SpringBoot的先进理念与C#的简洁优雅合二为一，声明式编程，专注于”做什么”而不是”如何去做”。在更高层面写代码，更关心的是目标，而不是底层算法实现的过程，SummerBoot,致力于打造一个人性化框架，让.net开发变得更简单。

# 框架说明
这是一个注解 + 接口的方式实现各种调用的全声明式框架，框架会通过Reflection Emit技术，自动生成接口的实现类。

# 加入QQ群反馈建议
群号:799648362

# Getting Started
## Nuget
 你可以运行以下命令在你的项目中安装 SummerBoot。
 
 ```PM> Install-Package SummerBoot```
# 支持框架
net core 3.1,net 6

# 文档目录
- [感谢jetbrain提供的ide许可证](#感谢jetbrain提供的ide许可证)
- [SummerBoot的核心理念](#summerboot的核心理念)
- [框架说明](#框架说明)
- [加入QQ群反馈建议](#加入qq群反馈建议)
- [Getting Started](#getting-started)
	- [Nuget](#nuget)
- [支持框架](#支持框架)
- [文档目录](#文档目录)
- [SummerBoot中操作数据库](#summerboot中操作数据库)
	- [准备工作](#准备工作)
	- [1.首先在startup.cs类中注册服务](#1首先在startupcs类中注册服务)
	- [2.定义一个数据库实体类](#2定义一个数据库实体类)
	- [3.定义接口，并继承于IBaseRepository，同时在接口上添加AutoRepository注解表示让框架自动注册并生成实现类](#3定义接口并继承于ibaserepository同时在接口上添加autorepository注解表示让框架自动注册并生成实现类)
	- [4.增删改查操作，均支持异步同步](#4增删改查操作均支持异步同步)
		- [4.1 查](#41-查)
			- [4.1.1 IQueryable链式语法查询。](#411-iqueryable链式语法查询)
			- [4.1.2 直接在接口里定义方法，并且在方法上加上注解Select,然后在Select里写sql语句](#412-直接在接口里定义方法并且在方法上加上注解select然后在select里写sql语句)
			- [4.1.3 select注解这种方式拼接where查询条件](#413-select注解这种方式拼接where查询条件)
			- [4.1.4 IBaseRepository接口自带的查方法](#414-ibaserepository接口自带的查方法)
		- [4.2 增](#42-增)
			- [4.2.1 接口自带了Insert方法，可以插入单个实体，或者实体列表，如果实体类的主键名称为Id,且有Key注解，并且是自增的，那么插入后，框架会自动为实体的ID这个字段赋值，值为自增的ID值。](#421-接口自带了insert方法可以插入单个实体或者实体列表如果实体类的主键名称为id且有key注解并且是自增的那么插入后框架会自动为实体的id这个字段赋值值为自增的id值)
		- [4.3 删](#43-删)
			- [4.3.1 接口自带了Delete方法，可以删除单个实体，或者实体列表](#431-接口自带了delete方法可以删除单个实体或者实体列表)
			- [4.3.2 同时还支持基于lambda表达式的删除，返回受影响的行数，例如](#432-同时还支持基于lambda表达式的删除返回受影响的行数例如)
		- [4.4 改](#44-改)
			- [4.4.1 接口自带了Update方法，可以更新单个实体，或者实体列表,联合主键的话，数据库实体类对应的多字段都添加key注解即可。](#441-接口自带了update方法可以更新单个实体或者实体列表联合主键的话数据库实体类对应的多字段都添加key注解即可)
			- [4.4.2 同时还支持基于IQueryable链式语法的更新方式，返回受影响的行数，例如](#442-同时还支持基于iqueryable链式语法的更新方式返回受影响的行数例如)
		- [4.5.事务支持](#45事务支持)
		- [4.6 如果有些特殊情况需要自己手写实现类怎么办?](#46-如果有些特殊情况需要自己手写实现类怎么办)
			- [4.6.1 定义一个接口继承于IBaseRepository，并且在接口中定义自己的方法](#461-定义一个接口继承于ibaserepository并且在接口中定义自己的方法)
			- [4.6.2 添加一个实现类，继承于BaseRepository类和自定义的ICustomCustomerRepository接口，实现类添加AutoRegister注解。](#462-添加一个实现类继承于baserepository类和自定义的icustomcustomerrepository接口实现类添加autoregister注解)
- [SummerBoot中使用feign进行http调用](#summerboot中使用feign进行http调用)
	- [1.在startup.cs类中注册服务](#1在startupcs类中注册服务)
	- [2.定义接口](#2定义接口)
	- [3.设置请求头(header)](#3设置请求头header)
	- [4.自定义拦截器](#4自定义拦截器)
	- [5.定义方法](#5定义方法)
		- [5.1方法里的普通参数](#51方法里的普通参数)
		- [5.2方法里的特殊参数](#52方法里的特殊参数)
			- [5.2.1参数添加Query注解](#521参数添加query注解)
			- [5.2.2参数添加Body(BodySerializationKind.Form)注解](#522参数添加bodybodyserializationkindform注解)
			- [5.2.3参数添加Body(BodySerializationKind.Json)注解](#523参数添加bodybodyserializationkindjson注解)
			- [5.2.4使用特殊类HeaderCollection作为方法参数，即可批量添加请求头](#524使用特殊类headercollection作为方法参数即可批量添加请求头)
			- [5.2.5使用特殊类BasicAuthorization作为方法参数，即可添加basic认证的Authorization请求头](#525使用特殊类basicauthorization作为方法参数即可添加basic认证的authorization请求头)
			- [5.2.6使用特殊类MultipartItem作为方法参数，并且在方法上标注Multipart注解，即可上传附件](#526使用特殊类multipartitem作为方法参数并且在方法上标注multipart注解即可上传附件)
			- [5.2.7使用类Stream作为方法返回类型，即可接收流式数据，比如下载文件。](#527使用类stream作为方法返回类型即可接收流式数据比如下载文件)
			- [5.2.8使用类HttpResponseMessage作为方法返回类型，即可获得最原始的响应消息。](#528使用类httpresponsemessage作为方法返回类型即可获得最原始的响应消息)
			- [5.2.9使用类Task作为方法返回类型，即无需返回值。](#529使用类task作为方法返回类型即无需返回值)
- [SummerBoot中的人性化的设计](#summerboot中的人性化的设计)

# SummerBoot中操作数据库
summerBoot基于工作单元与仓储模式开发了自己的ORM->repository，底层基于dapper，上层通过模板模式，支持了常见的4种数据库类型（sqlserver，mysql，oracle，sqlite）的增删改查操作,如果有其他数据库需求，可以参考以上4个的源码，给本项目贡献代码。orm不支持多表的lambda查询，因为多表查询直接写sql更好更易理解。

## 准备工作
需要自己通过nuget安装相应的数据库依赖包，比如SqlServer的Microsoft.Data.SqlClient，mysql的Mysql.data, oracle的Oracle.ManagedDataAccess.Core

## 1.首先在startup.cs类中注册服务
````
services.AddSummerBoot();

services.AddSummerBootRepository(it =>
{
	//注册数据库类型，比如SqliteConnection，MySqlConnection,OracleConnection,SqlConnection
	it.DbConnectionType = typeof(SqliteConnection);
	//添加数据库连接字符串
	it.ConnectionString = "Data source=./mydb.db";

	//以下为可选操作
	//插入的时候自动添加创建时间，数据库实体类必须继承于BaseEntity,oracle继承于OracleBaseEntity
	it.AutoUpdateLastUpdateOn = true;
	//插入的时候自动添加创建时间，使用utc时间
	it.AutoUpdateLastUpdateOnUseUtc = false;
	//update的时候自动更新最后更新时间字段，数据库实体类必须继承于BaseEntity,oracle继承于OracleBaseEntity
	it.AutoAddCreateOn = true;
	//update的时候自动更新最后更新时间字段,使用utc时间
	it.AutoAddCreateOnUseUtc = false;
	//启用软删除，数据库实体类必须继承于BaseEntity,oracle继承于OracleBaseEntity
	it.IsUseSoftDelete = true;
});

````
## 2.定义一个数据库实体类
其中注解大部分来自于系统自带的命名空间System.ComponentModel.DataAnnotations 和 System.ComponentModel.DataAnnotations.Schema，比如表名Table,主键Key,主键自增DatabaseGenerated(DatabaseGeneratedOption.Identity)，列名Column，不映射该字段NotMapped等,同时自定义了一部分注解，比如更新时忽略该列IgnoreWhenUpdateAttribute(主要用在创建时间这种在update的时候不需要更新的字段),
同时SummerBoot自带了一个基础实体类BaseEntity（oracle 为OracleBaseEntity），实体类里包括自增的id，创建人，创建时间，更新人，更新时间以及软删除标记，推荐实体类直接继承BaseEntity

````
public class Customer:BaseEntity
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

## 3.定义接口，并继承于IBaseRepository，同时在接口上添加AutoRepository注解表示让框架自动注册并生成实现类
````
[AutoRepository]
public interface ICustomerRepository : IBaseRepository<Customer>
{
}
````
## 4.增删改查操作，均支持异步同步
### 4.1 查
通过DI注入自定义仓储接口以后，就可以开始查了，支持正常查询与分页查询，查询有2种方式。
#### 4.1.1 IQueryable链式语法查询。
````
//常规查询
var customers= customerRepository.Where(it => it.Age > 5).OrderBy(it => it.Id).Take(10).ToList();
//分页
var page2 = await customerRepository.Where(it => it.Age > 5).Skip(0).Take(10).ToPageAsync();
````

#### 4.1.2 直接在接口里定义方法，并且在方法上加上注解Select,然后在Select里写sql语句
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
> 注意：4.1.2查询里的分页支持，方法的返回值由Page这个类包裹，同时方法参数里必须包含 IPageable这个分页参数，sql语句里也要有order by，例如:
````
[Select("select * from customer where age>@age order by id")]
Page<Customer> GetCustomerByPage(IPageable pageable, int age);
````

#### 4.1.3 select注解这种方式拼接where查询条件
将单个查询条件用{{}}包裹起来，一个条件里只能包括一个变量，同时在定义方法的时候，参数定义为WhereItem\<T\>,T为泛型参数，表示真正的参数类型，这样summerboot就会自动处理查询条件，处理规则如下，如果whereItem的active为true，即激活该条件，则sql语句中{{ }}包裹的查询条件会展开并参与查询，如果active为false，则sql语句中{{ }}包裹的查询条件自动替换为空字符串，不参与查询，为了使whereItem更好用，提供了WhereBuilder这种方式，使用例子如下所示：
````
//definition
[AutoRepository]
public interface ICustomerRepository : IBaseRepository<Customer>
{
[Select("select * from customer where 1=1 {{ and name=@name}}{{ and age=@age}}")]
Task<List<CustomerBuyProduct>> GetCustomerByConditionAsync(WhereItem<string> name, WhereItem<int> age);

[Select("select * from customer where 1=1 {{ and name=@name}}{{ and age=@age}} order by id")]
Task<Page<Customer>> GetCustomerByPageByConditionAsync(IPageable pageable, WhereItem<string> name, WhereItem<int> age);
}

//use
var nameEmpty = WhereBuilder.Empty<string>();//var nameEmpty =  new WhereItem<string>(false,"");

var ageEmpty = WhereBuilder.Empty<int>();
var nameWhereItem = WhereBuilder.HasValue("page5");//var nameWhereItem =WhereItem<string>(true,"page5"); 
var ageWhereItem = WhereBuilder.HasValue(5);
var pageable = new Pageable(1, 10);

var bindResult = customerRepository.GetCustomerByCondition(nameWhereItem, ageEmpty);
Assert.Single(bindResult);
var bindResult2 = customerRepository.GetCustomerByCondition(nameEmpty, ageEmpty);
Assert.Equal(102, bindResult2.Count);
var bindResult5 = customerRepository.GetCustomerByPageByCondition(pageable, nameWhereItem, ageEmpty);
Assert.Single(bindResult5.Data);
var bindResult6 = customerRepository.GetCustomerByPageByCondition(pageable, nameEmpty, ageEmpty);
````

>如果还有更复杂的自定义查询怎么办？参考 6.如果有些特殊情况要求自己手写实现类怎么办?

#### 4.1.4 IBaseRepository接口自带的查方法
Get方法，通过id获取结果，GetAll(),获取表里的所有结果集。

### 4.2 增
#### 4.2.1 接口自带了Insert方法，可以插入单个实体，或者实体列表，如果实体类的主键名称为Id,且有Key注解，并且是自增的，那么插入后，框架会自动为实体的ID这个字段赋值，值为自增的ID值。
````
var customer = new Customer() { Name = "testCustomer" };
customerRepository.Insert(customer);

var customer2 = new Customer() { Name = "testCustomer2" };
var customer3 = new Customer() { Name = "testCustomer3" };
var customerList = new List<Customer>() { customer2, customer3 };
customerRepository.Insert(customerList);
````
### 4.3 删
#### 4.3.1 接口自带了Delete方法，可以删除单个实体，或者实体列表
````
customerRepository.Delete(customer);

customerRepository.Delete(customerList);
````
#### 4.3.2 同时还支持基于lambda表达式的删除，返回受影响的行数，例如
````
 var deleteCount = customerRepository.Delete(it => it.Age > 5);
````
### 4.4 改
#### 4.4.1 接口自带了Update方法，可以更新单个实体，或者实体列表,联合主键的话，数据库实体类对应的多字段都添加key注解即可。
````
customerRepository.Update(customer);

customerRepository.Update(customerList);
````
#### 4.4.2 同时还支持基于IQueryable链式语法的更新方式，返回受影响的行数，例如
````
var updateCount= customerRepository.Where(it=>it.Name == "testCustomer")
.SetValue(it=>it.Age,5)
.SetValue(it=>it.TotalConsumptionAmount,100)
.ExecuteUpdate();
````

### 4.5.事务支持
事务支持，需要在注入自定义仓储接口的同时，也注入框架自带的IUnitOfWork接口，用法如下

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

### 4.6 如果有些特殊情况需要自己手写实现类怎么办?
#### 4.6.1 定义一个接口继承于IBaseRepository，并且在接口中定义自己的方法
>注意，此时该接口无需添加AutoRepository注解
````
public interface ICustomCustomerRepository : IBaseRepository<Customer>
{
    Task<List<Customer>> GetCustomersAsync(string name);

    Task<Customer> GetCustomerAsync(string name);

    Task<int> UpdateCustomerNameAsync(string oldName, string newName);

    Task<int> CustomQueryAsync();
}
````
#### 4.6.2 添加一个实现类，继承于BaseRepository类和自定义的ICustomCustomerRepository接口，实现类添加AutoRegister注解。
注解的参数为这个类对应的自定义接口的类型和服务的声明周期ServiceLifetime（周期默认为scope级别），添加AutoRegister注解的目的是让模块自动将自定义接口和自定义类注册到IOC容器中，后续直接注入使用即可，BaseRepository自带了Execute，QueryFirstOrDefault和QueryList方法，如果要接触更底层的dbConnection进行查询，参考下面的CustomQueryAsync方法，首先OpenDb()，然后查询，查询中一定要带上transaction:dbTransaction这个参数，查询结束以后CloseDb();

````
[AutoRegister(typeof(ICustomCustomerRepository))]
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

# SummerBoot中使用feign进行http调用
>我们使用Feign，feign底层基于httpClient。

## 1.在startup.cs类中注册服务
````
services.AddSummerBoot();
services.AddSummerBootFeign();
````
## 2.定义接口
 定义一个接口，并且在接口上添加FeignClient注解，FeignClient注解里可以自定义接口名称-Name，http接口url的公共部分-url（整个接口请求的url由FeignClient里的url加上方法里的path组成）,是否忽略远程接口的https证书校验-IsIgnoreHttpsCertificateValidate,接口超时时间-Timeout（单位s），自定义拦截器-InterceptorType。

````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
public interface ITestFeign
{
   
}
````

## 3.设置请求头(header)
接口上可以选择添加Headers注解，代表这个接口下所有http请求都带上注解里的请求头。Headers的参数为变长的string类型的参数，同时Headers也可以添加在方法上，代表该方法调用的时候，会加该请求头，接口上的Headers参数可与方法上的Headers参数互相叠加，同时headers里可以使用变量，变量的占位符为{}，如
````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
[Headers("a:a","b:b")]
public interface ITestFeign
{
	[GetMapping("/testGet")]
	Task<Test> TestAsync();
	
	[GetMapping("/testGetWithHeaders")]
	[Headers("c:c")]
	Task<Test> TestWithHeadersAsync();
	
	//header替换
	[Headers("a:{methodName}")]
	[PostMapping("/abc")]
	Task<Test> TestHeaderAsync(string methodName);
}

await TestFeign.TestAsync()
>>> get, http://localhost:5001/home/testGet,header为 "a:a" 和 "b:b"

await TestFeign.TestWithHeadersAsync()
>>> get, http://localhost:5001/home/testGetWithHeaders,header为 "a:a" ,"b:b"和 "c:c"

await TestFeign.TestHeaderAsync("abc");
>>> post, http://localhost:5001/home/abc，同时请求头为 "a:abc"
````

## 4.自定义拦截器
自定义拦截器对接口下的所有方法均生效，拦截器的应用场景主要是在请求前做一些操作，比如请求第三方业务接口前，需要先登录第三方系统，那么就可以在拦截器里先请求第三方登录接口，获取到凭证以后，放到header里，拦截器需要实现IRequestInterceptor接口，例子如下
````
//先定义一个用来登录的loginFeign客户端
 [FeignClient(Url = "http://localhost:5001/login", IsIgnoreHttpsCertificateValidate = true,Timeout = 100)]
    public interface ILoginFeign
    {
        [PostMapping("/login")]
        Task<LoginResultDto> LoginAsync([Body()] LoginDto loginDto );
    }
	
//接着自定义登录拦截器
public class LoginInterceptor : IRequestInterceptor
{

	private readonly ILoginFeign loginFeign;
	private readonly IConfiguration configuration;

	public LoginInterceptor(ILoginFeign loginFeign, IConfiguration configuration)
	{
		this.loginFeign = loginFeign;
		this.configuration = configuration;
	}
	

	public async Task ApplyAsync(RequestTemplate requestTemplate)
	{
		var username = configuration.GetSection("username").Value;
		var password = configuration.GetSection("password").Value;

		var loginResultDto = await this.loginFeign.LoginAsync(new LoginDto(){Name = username,Password = password});
		if (loginResultDto != null)
		{
			requestTemplate.Headers.Add("Authorization", new List<string>() { "Bearer "+loginResultDto.Token });
		}

		await Task.CompletedTask;
	}
}

//定义访问业务接口的testFegn客户端，在客户端上定义拦截器为loginInterceptor
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(LoginInterceptor),Timeout = 100)]
public interface ITestFeign
{
	[GetMapping("/testGet")]
	Task<Test> TestAsync();
}

await TestFeign.TestAsync();
>>> get to http://localhost:5001/home/testGet,header为 "Authorization:Bearer abc"

````
忽略拦截器，有时候我们接口中的某些方法，是不需要拦截器的，那么就可以在方法上添加注解IgnoreInterceptor，那么该方法发起的请求，就会忽略拦截器，如
````
//定义访问业务接口的testFegn客户端，在客户端上定义拦截器为loginInterceptor
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(LoginInterceptor),Timeout = 100)]
public interface ITestFeign
{
	[GetMapping("/testGet")]
	[IgnoreInterceptor]
	Task<Test> TestAsync();
}

await TestFeign.TestAsync();
>>> get to http://localhost:5001/home/testGet,没有header

````

## 5.定义方法
每个方法都应该添加注解代表发起请求的类型和要访问的url，有4个内置注解， GetMapping，PostMapping，PutMapping，DeleteMapping，同时方法的返回值必须是Task<>类型
````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
public interface ITestFeign
{
	[GetMapping("/testGet")]
	Task<Test> TestAsync();
	
	[PostMapping("/testPost")]
	Task<Test> TestPostAsync();
	
	[PutMapping("/testPut")]
	Task<Test> TestPutAsync();
	
	[DeleteMapping("/testDelete")]
	Task<Test> TestDeleteAsync();
}
````

### 5.1方法里的普通参数
参数如果没有特殊注解，或者不是特殊类，均作为动态参数参与url，header里变量的替换，(参数如果为类，则读取类的属性值)，url和header中的变量使用占位符{}，如果变量名和参数名不一致，则可以使用AliasAs注解（可以用在参数或者类的属性上）来指定别名，如
````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
public interface ITestFeign
{
	//url替换
	[PostMapping("/{methodName}")]
	Task<Test> TestAsync(string methodName);	
	
	//header替换
	[Headers("a:{methodName}")]
	[PostMapping("/abc")]
	Task<Test> TestHeaderAsync(string methodName);
	
	
	//AliasAs指定别名
	[Headers("a:{methodName}")]
	[PostMapping("/abc")]
	Task<Test> TestAliasAsAsync([AliasAs("methodName")] string name);
}

await TestFeign.TestAsync("abc");
>>> post to http://localhost:5001/home/abc

await TestFeign.TestAliasAsAsync("abc");
>>> post, http://localhost:5001/home/abc

await TestFeign.TestHeaderAsync("abc");
>>> post, http://localhost:5001/home/abc，同时请求头为 "a:abc"
````

### 5.2方法里的特殊参数
#### 5.2.1参数添加Query注解
参数添加query注解后参数值将以key1=value1&key2=value2的方式添加到url后面。
````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
public interface ITestFeign
{
	 [GetMapping("/TestQuery")]
	 Task<Test> TestQuery([Query] string name);
	 
	 [GetMapping("/TestQueryWithClass")]
	 Task<Test> TestQueryWithClass([Query]Test tt);
}

await TestFeign.TestQuery("abc");
>>> get, http://localhost:5001/home/TestQuery?name=abc

await TestFeign.TestQueryWithClass(new Test() { Name = "abc", Age = 3 });
>>> get, http://localhost:5001/home/TestQueryWithClass?Name=abc&Age=3
````

#### 5.2.2参数添加Body(BodySerializationKind.Form)注解
相当于模拟html里的form提交，参数值将被URL编码后，以key1=value1&key2=value2的方式添加到载荷（body）里。
````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
public interface ITestFeign
{
	 [PostMapping("/form")]
	 Task<Test> TestForm([Body(BodySerializationKind.Form)] Test tt);
}

await TestFeign.TestForm(new Test() { Name = "abc", Age = 3 });
>>> post, http://localhost:5001/home/form,同时body里的值为Name=abc&Age=3
````

#### 5.2.3参数添加Body(BodySerializationKind.Json)注解
即以application/json的方式提交，参数值将会被json序列化后添加到载荷（body）里。
````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
public interface ITestFeign
{
	 [PostMapping("/json")]
	 Task<Test> TestJson([Body(BodySerializationKind.Json)] Test tt);
}

await TestFeign.TestJson(new Test() { Name = "abc", Age = 3 });
>>> post, http://localhost:5001/home/json,同时body里的值为{"Name":"abc","Age":3}
````

#### 5.2.4使用特殊类HeaderCollection作为方法参数，即可批量添加请求头
````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
public interface ITestFeign
{
	 [PostMapping("/json")]
	 Task<Test> TestJson([Body(BodySerializationKind.Json)] Test tt, HeaderCollection headers);
}

 var headerCollection = new HeaderCollection()
                { new KeyValuePair<string, string>("a", "a"), 
                    new KeyValuePair<string, string>("b", "b") };
					
await TestFeign.TestJson(new Test() { Name = "abc", Age = 3 },headerCollection);
>>> post, http://localhost:5001/home/json,同时body里的值为{"Name":"abc","Age":3},header为 "a:a" 和 "b:b"
````

#### 5.2.5使用特殊类BasicAuthorization作为方法参数，即可添加basic认证的Authorization请求头
````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
public interface ITestFeign
{
	[GetMapping("/testBasicAuthorization")]
	Task<Test> TestBasicAuthorization(BasicAuthorization basicAuthorization);
}

 var username="abc";
 var password="123";
				 
await TestFeign.TestBasicAuthorization(new BasicAuthorization(username,password));
>>> get, http://localhost:5001/home/testBasicAuthorization,header为 "Authorization:Basic YWJjOjEyMw=="
````

#### 5.2.6使用特殊类MultipartItem作为方法参数，并且在方法上标注Multipart注解，即可上传附件
````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
public interface ITestFeign
{
	//仅上传文件
	[Multipart]
	[PostMapping("/multipart")]
	Task<Test> MultipartTest(MultipartItem item);
	//在上传附件的同时，也可以附带参数
	[Multipart]
	[PostMapping("/multipart")]
	Task<Test> MultipartTest([Body(BodySerializationKind.Form)] Test tt, MultipartItem item);
}
//仅上传文件
var basePath = Path.Combine(AppContext.BaseDirectory, "123.txt");
var name="file";
var fileName="123.txt";
//方式1，使用byteArray
var byteArray=   File.ReadAllBytes(basePath);
var result = await testFeign.MultipartTest(new MultipartItem(byteArray, name, fileName));

//方式2 ，使用stream 
var fileStream= new FileInfo(basePath).OpenRead();
var result = await testFeign.MultipartTest(new MultipartItem(fileStream, name, fileName));

//方式3，使用fileInfo
var result = await testFeign.MultipartTest(new MultipartItem(new FileInfo(basePath),name,fileName));

 >>> post, http://localhost:5001/home/multipart,同时body里带有附件
 
 //在上传附件的同时，也可以附带参数
 var basePath = Path.Combine(AppContext.BaseDirectory, "123.txt");
 var name="file";
 var fileName="123.txt";
 //方式1，使用byteArray
 var byteArray=   File.ReadAllBytes(basePath);
 var result = await testFeign.MultipartTest(new Test() { Name = "sb", Age = 3 }, new MultipartItem(byteArray, name, fileName));
 
 //方式2 ，使用stream 
 var fileStream= new FileInfo(basePath).OpenRead();
 var result = await testFeign.MultipartTest(new Test() { Name = "sb", Age = 3 }, new MultipartItem(fileStream, name, fileName));
 
 //方式3，使用fileInfo
 var result = await testFeign.MultipartTest(new Test() { Name = "sb", Age = 3 },new MultipartItem(new FileInfo(basePath),name,fileName));
 
  >>> post, http://localhost:5001/home/multipart,同时body里的值为Name=abc&Age=3，并且带有附件
````

#### 5.2.7使用类Stream作为方法返回类型，即可接收流式数据，比如下载文件。
````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
public interface ITestFeign
{
	 [GetMapping("/downLoadWithStream")]
	Task<Stream> TestDownLoadWithStream();
}
				 
using var streamResult =await testFeign.TestDownLoadStream();
using var newfile = new FileInfo("D:\\123.txt").OpenWrite();
streamResult.CopyTo(newfile);

>>> get, http://localhost:5001/home/downLoadWithStream,返回值为流式数据，然后就可以保存为文件。
````

#### 5.2.8使用类HttpResponseMessage作为方法返回类型，即可获得最原始的响应消息。
````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
public interface ITestFeign
{
	[GetMapping("/test")]
	Task<HttpResponseMessage> Test();
}
				 
var rawResult =await testFeign.Test();

>>> get, http://localhost:5001/home/Test,返回值为httpclient的原始返回数据。
````

#### 5.2.9使用类Task作为方法返回类型，即无需返回值。
````
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor),Timeout = 100)]
public interface ITestFeign
{
	[GetMapping("/test")]
	Task Test();
}
				 
await testFeign.Test();

>>> get, http://localhost:5001/home/Test,忽略返回值
````

# SummerBoot中的人性化的设计
 1.先说一个net core mvc自带的功能，如果我们想要在appsettings.json里配置web应用的ip和port该怎么办？在appsettings.json里直接写
 ````
 {
     "urls":"http://localhost:7002;http://localhost:7012"
 }
 ````

 2. AutoRegister注解，作用是让框架自动将接口和接口的实现类注册到IOC容器中，标注在实现类上，注解的参数为这个类对应的自定义接口的type和服务的生命周期ServiceLifetime（周期默认为scope级别），使用方式如下:
````
 public interface ITest
    {

    }

    [AutoRegister(typeof(ITest),ServiceLifetime.Transient)]
    public class Test:ITest
    {

    }
````
 3. ApiResult 接口返回值包装类，包含 code，msg和data，3个字段，让整个系统的返回值统一有序，有利于前端的统一拦截，统一操作。使用方式如下:
 ````
[HttpPost("CreateServerConfigAsync")]
public async Task<ApiResult<bool>> CreateServerConfigAsync(ServerConfigDto dto)
{
		var result = await serverConfigService.CreateServerConfigAsync(dto);
		return ApiResult<bool>.Ok(result);
}
 ````
 4. 对net core mvc的一些增强操作，包括全局错误拦截器，和接口参数校验失败后的处理，配合ApiResult，使得系统报错时，也能统一返回，使用方式如下,首先在startUp里注册该服务，注意，要放在mvc注册之后:
 ````
services.AddControllersWithViews();
services.AddSummerBootMvcExtension(it =>
{
		//是否启用全局错误处理
		it.UseGlobalExceptionHandle = true;
		//是否启用参数校验处理
		it.UseValidateParameterHandle = true;
});
 ````
4.1 全局错误拦截器使用后的效果
 我们可以直接在业务代码里抛出错误，全局错误拦截器会捕捉到该错误，然后使用统一格式返回给前端，业务代码如下:
````
private void ValidateData(EnvConfigDto dto)
{
		if (dto == null)
		{
			throw new ArgumentNullException("参数不能为空");
		}
		if(dto.ServerConfigs==null|| dto.ServerConfigs.Count==0)
		{
			throw new ArgumentNullException("环境下没有配置服务器");
		}
}
````
如果业务代码里报错,则返回值如下:
````
{
  "code": 40000,
  "msg": "Value cannot be null. (Parameter '环境下没有配置服务器')",
  "data": null
}
````
4.2 接口参数校验失败后的处理的效果
我们在接口的参数dto里添加校验注解，代码如下
````
public class EnvConfigDto : BaseEntity
{
		/// <summary>
		/// 环境名
		/// </summary>
		[Required(AllowEmptyStrings = false, ErrorMessage = "环境名称不能为空")]
		public string Name { get; set; }
		/// <summary>
		/// 环境下对应的服务器
		/// </summary>
		[NotMapped]
		public List<int> ServerConfigs { get; set; }
}
````
如果参数校验不通过,则返回值如下:
````
{
  "code": 40000,
  "msg": "环境名称不能为空",
  "data": null
}
````

5. QueryCondition，lambda查询条件组合，解决前端传条件过来进行过滤查询的痛点，除了基本的And和Or方法，还添加了更人性化的方法，一般前端传过来的dto里的属性，有字符串类型，如果他们有值则添加到查询条件里，所以特地提取了2个方法，包括了AndIfStringIsNotEmpty（如果字符串不为空则进行and操作，否则返回原表达式），OrIfStringIsNotEmpty（如果字符串不为空则进行or操作，否则返回原表达式），
同时dto里的属性，还有可能是nullable类型，即可空类型，比如 int? test代表用户是否填写某个过滤条件，如果hasValue则添加到查询条件里，所以特地提取了2个方法，AndIfNullableHasValue（如果可空值不为空则进行and操作，否则返回原表达式），OrIfNullableHasValue（如果可空值不为空则进行and操作，否则返回原表达式）用法如下:
````
//dto
public class ServerConfigPageDto : IPageable
{
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	/// <summary>
	/// ip地址
	/// </summary>
	public string Ip { get; set; }
	/// <summary>
	/// 连接名
	/// </summary>
	public string ConnectionName { get; set; }
	
	public int? Test { get; set; }
}
//condition
 var queryCondition = QueryCondition.True<ServerConfig>()
	.And(it => it.Active == 1)
	//如果字符串不为空则进行and操作，否则返回原表达式
	.AndIfStringIsNotEmpty(dto.Ip, it => it.Ip.Contains(dto.Ip))
	//如果可空值不为空则进行and操作，否则返回原表达式
	.AndIfNullableHasValue(dto.Test,it=>it.Test==dto.Test)
	.AndIfStringIsNotEmpty(dto.ConnectionName,it=>it.ConnectionName.Contains(dto.ConnectionName));
				
 var queryResult = await serverConfigRepository.Where(queryCondition)
	.Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize).ToPageAsync();
````

