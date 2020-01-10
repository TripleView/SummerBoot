# SummerBoot
> 将SpringBoot的先进理念与C#的简洁优雅合二为一
## 优点：

> 1. 利用注解+AOP+微软原生DI，实现普通类级别的拦截（非middleWare和controller层面上的拦截），从而在net core里复现了许多springBoot里的功能，先进的编程理念带来的影响是，使用summerBoot的团队，将会有统一的，更优雅的代码实现方式，相同功能的代码将会被剥离出来，放到AOP层面上来统一实现，一个注解顶20行代码，不是梦。

> 2. 完全面向接口设计，任何模块可自定义替换。

> 3. 文档齐全，每个模块是如何进行设计以及实现的，本系列博客都有相应的文章去讲解，免去了看源码却没注释的烦恼，同时，也可以清晰的看到设计演进，授人以鱼莫若授人以渔，做最有诚意的开源。

## Getting Started
### Nuget
 你可以运行以下命令在你的项目中安装 SummerBoot。
 
 ```PM> Install-Package SummerBoot```
## 支持框架
net core 3.1，当然2.1也可，不过需要自行编译

## 注册服务
### 接口-实现类注册方式
	services.AddSbScoped<IPersonService, PersonService>();
### 仅有实现类注册方式
	services.AddSbScoped<Engine>();

> 注意，动态代理仅有实现类时，代理的方法，只能是*虚方法* ，如下。

	public virtual void Test()
    {

    }	

### 注册时，可以添加自定义拦截器
	 services.AddSbScoped<ICar, Car>(typeof(MyInterceptor));

> 注意，自定义拦截器必须实现*IInterceptor*接口。

### 带服务名称的注册方式
	services.AddSbScoped<IWheel, WheelA>("A轮胎", typeof(TransactionalInterceptor));
	services.AddSbScoped<IWheel, WheelB>("B轮胎", typeof(TransactionalInterceptor));

> 这种方式，主要用于一个接口，对应多个实现类，需要进行精准属性注入的时候使用的。

> 以上模仿微软原生DI的注册方式，均实现了3个级别的注册，分别是AddSbScope,AddSbSingleton,AddSbTransient,以及对应的try版本，比如TryAddSbTransient

## 添加summerBoot核心依赖
	services.AddSummerBoot();
 
## 添加summerBoot注解式缓存
	services.AddSummerBootCache(it =>
	{
	   it.UseRedis("129.204.47.226,password=summerBoot");
	});
	
## 添加summerBoot仓储
	 services.AddSummerBootRepository(it =>
	 {
	   it.DbConnectionType = typeof(SqliteConnection);
	   it.ConnectionString = "Data source=./DbFile/mydb.db";
	 });
 
 > 支持所有dapper支持的数据库，只需要将对应的IDbConnection传入DbConnectionType即可，比如sqlite传入
 > SqliteConnection，mysql传入MysqlConnection。</b>
 
## 添加MVC扩展
	services.AddControllers().AddSummerBootMvcExtention();

> 添加扩展，主要是为了替换MVC自带的控制器激活器，达到controller层面属性注入，值注入的效果。

## 已支持的注解

* Autowired。属性注入，只对属性生效，参数require，表示是否要求强制注入，默认为false，如果为true，当从IOC容器里取不到相应的依赖项时，则报错。  
```
	 public class PersonService : IPersonService
	 {
		[Autowired(true)]
		private IPersonRepository IPersonRepository { set; get; }
	 }
```  

* Qualifier。精准属性注入，参数name，服务名称，主要与Autowired注解联合使用，通过服务名称，达到一个接口，多个实现类，精准注入的目的。
```
    public class Car : ICar
    {
        [Autowired]
        [Qualifier("A轮胎")]
        private IWheel Wheel { set; get; }
	}
```

> 注册服务的时候要像这样,指定服务名称

	services.AddSbScoped<IWheel, WheelA>("A轮胎", typeof(TransactionalInterceptor));
	services.AddSbScoped<IWheel, WheelB>("B轮胎", typeof(TransactionalInterceptor));

* Value。配置值注入，参数value，参数名称。这个参数名称，即appsettings.json或者appsettings.Development.json里的参数。

```
	public class Engine
    {
        [Value("HelpNumber")]
        public string HelpNumber { set; get; }
	}
```

* Transactional。事务注解，在一个方法上加上这个注解，代表整个方法开启事务,当执行方法过程中报错了，则整个事务回滚。

```
	[Transactional]
	public void AddOil()
	{
		//初始化金额
		var cashBalance = CashBalanceRepository.Insert(new CashBalance() { Balance = 100 });
		//初始化油量
		var oilQuantity = OilQuantityRepository.Insert(new OilQuantity() { Quantity = 5 });

		cashBalance.Balance -= 95;
		oilQuantity.Quantity += 50;

		CashBalanceRepository.Update(cashBalance);
		//throw new Exception("throw err");
		OilQuantityRepository.Update(oilQuantity);
	}
```

* Repository。仓储标记，当一个接口加上这个注解以后，代表接口为仓储接口，会自动注册为仓储，与Select等注解配合使用,达到只需要写接口，无须实现类，即可实现增删改查的目的。

```
    [Repository]
    public interface IPersonRepository:IRepository<Person>
    {
        [Select("select * from person where name=@name")]
        Person GetPersonsByName(string name);

        [Select("select * from person where birthDate>@birthDate")]
        Person GetPersonsByBirthDate(DateTime birthDate);

        [Select("select * from person where birthDate>@birthDate and name=@name")]
        Person GetPersonsByBirthDateAndName(CityDto dto,string name);

        [Select("select * from address where city=@city and id=@id")]
        List<Address> GetAddressByCityName(CityDto dto,string id);

        [Select("select count(id) from address where city=@city")]
        int GetAddressCountByCityName(string city);

        [Select("select birthDate from person where name=@name")]
        DateTime GetPersonBirthDayByName(string name);
    }
```

* Cacheable。注解式缓存，用在方法上，代表请求的时候缓存结果，如果缓存里有，直接返回缓存的值，如果没有，则执行方法，返回方法返回值，并缓存这个返回值，参数cacheName-缓存名称, key缓存的键值, condition 条件限定，符合条件才缓存, unless 条件限定，排除这个条件, keyGenerator键值生成器名称, cacheManager缓存管理器，比如redis，memory等

```
     [Cacheable("db1", "Person", cacheManager: "redis",condition:"args[0]==\"1\"")]
	  public Person FindPerson(string name)
	  {
		  return IPersonRepository.GetPersonsByName(name);
	  }
```

* CachePut。注解式缓存，用在方法上，代表更新的时候，执行完方法后缓存结果值，参数cacheName-缓存名称, key缓存的键值, condition 条件限定，符合条件才缓存, unless 条件限定，排除这个条件, keyGenerator键值生成器名称, cacheManager缓存管理器，比如redis，memory等

```
    [CachePut("db1", key: "Person", cacheManager: "redis")]
	public Person UpdatePerson(Person person)
	{
		return new Person() { Name = "春光灿烂喜洋洋", Id = 3, Age = 5 };
	}
```

* CacheEvict。注解式缓存，用在方法上，代表执行方法后，删除缓存，，参数cacheName-缓存名称, key缓存的键值, condition 条件限定，符合条件才缓存, unless 条件限定，排除这个条件, keyGenerator键值生成器名称, cacheManager缓存管理器，比如redis，memory等

```
    [CacheEvict("db1", "Person", cacheManager: "redis")]
	public bool DeletePerson(Person person)
	{
		return true;
	}
```

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