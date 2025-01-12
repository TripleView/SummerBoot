[English Document](https://github.com/TripleView/SummerBoot/blob/master/README.md) | [中文文档](https://github.com/TripleView/SummerBoot/blob/master/README.zh-cn.md)


[![ GitHub license ](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/dotnetcore/CAP/master/LICENSE.txt)

# Thanks for the ide license provided by jetbrain
<a href="https://jb.gg/OpenSourceSupport"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png?_ga=2.140768178.1037783001.1644161957-503565267.1643800664&_gl=1*1rs8z57*_ga*NTAzNTY1MjY3LjE2NDM4MDA2NjQ.*_ga_V0XZL7QHEB*MTY0NDE2MTk1Ny4zLjEuMTY0NDE2NTE2Mi4w" width = "200" height = "200" alt="" align=center /></a>
# SummerBoot (中文名：夏日启动)
In order to let everyone better understand the use of summerBoot , I created a sample project-[
SummerBootAdmin ](https://github.com/TripleView/SummerBootAdmin), a general back-end management framework based on the separation of front-end and back-end, you can check the code of this project to better understand how to use summerBoot .

# Core idea
Combine the advanced concepts of SpringBoot with the simplicity and elegance of C#, declarative programming, focus on"what to do"rather than"how to do it", and write code at a higher level.SummerBoot is committed to creating an easy-to-use and easy-to-maintain humanized framework, so that everyone can get off work early to do what they like.

# Framework description
This is a fully declarative framework that implements various calls in the form of annotations + interfaces, including but not limited to database, http, cache, etc.The framework will automatically generate interface implementation classes through Reflection Emit technology.

# Join the QQ group for feedback and suggestions
Group number : 799648362

# Getting Started
## Nuget
SummerBoot in your project .
 
 ```PM> Install-Package SummerBoot ```
# support frame
net core 3.1, net 6,net 8

# document directory
- [Thanks for the ide license provided by jetbrain](#thanks-for-the-ide-license-provided-by-jetbrain)
- [SummerBoot (中文名：夏日启动)](#summerboot-中文名夏日启动)
- [Core idea](#core-idea)
- [Framework description](#framework-description)
- [Join the QQ group for feedback and suggestions](#join-the-qq-group-for-feedback-and-suggestions)
- [Getting Started](#getting-started)
  - [Nuget](#nuget)
- [support frame](#support-frame)
- [document directory](#document-directory)
- [SummerBoot uses repository for database operations](#summerboot-uses-repository-for-database-operations)
  - [Preparation](#preparation)
  - [1.Registration service](#1registration-service)
  - [2.Define a database entity class](#2define-a-database-entity-class)
  - [3.Write a controller to automatically generate database tables through entity classes](#3write-a-controller-to-automatically-generate-database-tables-through-entity-classes)
  - [4.Define storage interface](#4define-storage-interface)
  - [5.Add, delete, modify and Query, all support asynchronous synchronization](#5add-delete-modify-and-query-all-support-asynchronous-synchronization)
    - [5.1 Added](#51-added)
      - [5.1.1 The interface has its own Insert method, which can insert a single entity or a list of entities](#511-the-interface-has-its-own-insert-method-which-can-insert-a-single-entity-or-a-list-of-entities)
      - [5.1.2 Fast batch insertion, the storage interface comes with the FastBatchInsert method, which can quickly insert the entity list.](#512-fast-batch-insertion-the-storage-interface-comes-with-the-fastbatchinsert-method-which-can-quickly-insert-the-entity-list)
    - [5.2 delete](#52-delete)
      - [5.2.1 The interface comes with a Delete method, which can delete a single entity or a list of entities](#521-the-interface-comes-with-a-delete-method-which-can-delete-a-single-entity-or-a-list-of-entities)
      - [5.2.2 Also supports deletion based on lambda expressions, returning the number of affected rows, for example](#522-also-supports-deletion-based-on-lambda-expressions-returning-the-number-of-affected-rows-for-example)
    - [5.3 update](#53-update)
      - [5.3.1 The interface comes with an Update method, which can update a single entity or a list of entities](#531-the-interface-comes-with-an-update-method-which-can-update-a-single-entity-or-a-list-of-entities)
      - [5.3.2 It also supports the update method based on Lambda chain syntax](#532-it-also-supports-the-update-method-based-on-lambda-chain-syntax)
    - [5.4 Query](#54-query)
      - [5.4.1 Lambda chain syntax query](#541-lambda-chain-syntax-query)
        - [5.4.1.1 Single table query](#5411-single-table-query)
        - [5.4.1.2 Multiple table joint query](#5412-multiple-table-joint-query)
      - [5.4.2 Define methods directly in the interface, and add annotations to the methods, such as Select, Update, Delete](#542-define-methods-directly-in-the-interface-and-add-annotations-to-the-methods-such-as-select-update-delete)
      - [5.4.4 Select annotations are spliced in this way where query conditions](#544-select-annotations-are-spliced-in-this-way-where-query-conditions)
    - [5.5 Transaction Support](#55-transaction-support)
    - [5.6 Custom implementation classes in special cases](#56-custom-implementation-classes-in-special-cases)
      - [5.6.1 Define an interface inherited from IBaseRepository , and define your own methods in the interface](#561-define-an-interface-inherited-from-ibaserepository--and-define-your-own-methods-in-the-interface)
      - [5.6.2 Add an implementation class, inherited from the CustomBaseRepository class and the custom ICustomCustomerRepository interface, and add the AutoRegister annotation to the implementation class.](#562-add-an-implementation-class-inherited-from-the-custombaserepository-class-and-the-custom-icustomcustomerrepository-interface-and-add-the-autoregister-annotation-to-the-implementation-class)
      - [5.6.3 Example of use](#563-example-of-use)
  - [6 Automatically generate entity classes based on database tables, or automatically generate ddl statements for database tables based on entity classes](#6-automatically-generate-entity-classes-based-on-database-tables-or-automatically-generate-ddl-statements-for-database-tables-based-on-entity-classes)
    - [6.1 Table Namespace](#61-table-namespace)
    - [6.2 Automatically generate the ddl statement of the database table according to the entity class](#62-automatically-generate-the-ddl-statement-of-the-database-table-according-to-the-entity-class)
    - [6.2.2 Type mapping or name mapping from custom entity class fields to database fields](#622-type-mapping-or-name-mapping-from-custom-entity-class-fields-to-database-fields)
    - [6.3 Automatically generate entity classes based on database tables](#63-automatically-generate-entity-classes-based-on-database-tables)
- [SummerBoot uses feign to make http calls](#summerboot-uses-feign-to-make-http-calls)
  - [1.Registration service](#1registration-service-1)
  - [2.Define the interface](#2define-the-interface)
  - [3.Set the request header (header)](#3set-the-request-header-header)
  - [4.Custom Interceptor](#4custom-interceptor)
  - [5.Define the method](#5define-the-method)
    - [5.1 Common parameters in the method](#51-common-parameters-in-the-method)
    - [5.2 Special parameters in the method](#52-special-parameters-in-the-method)
      - [5.2.1 Parameters add Query annotations](#521-parameters-add-query-annotations)
        - [5.2.1.1 The Query annotation is used with the Embedded annotation, and the Embedded annotation class can be added as a parameter as a whole](#5211-the-query-annotation-is-used-with-the-embedded-annotation-and-the-embedded-annotation-class-can-be-added-as-a-parameter-as-a-whole)
      - [5.2.2 Parameters add Body (BodySerializationKind.Form) annotations](#522-parameters-add-body-bodyserializationkindform-annotations)
      - [5.2.3 Parameters add Body (BodySerializationKind.Json) annotations](#523-parameters-add-body-bodyserializationkindjson-annotations)
      - [5.2.4 Use the special class HeaderCollection as a method parameter to add request headers in batches](#524-use-the-special-class-headercollection-as-a-method-parameter-to-add-request-headers-in-batches)
      - [5.2.5 Use the special class BasicAuthorization as a method parameter to add the Authorization request header for basic authentication](#525-use-the-special-class-basicauthorization-as-a-method-parameter-to-add-the-authorization-request-header-for-basic-authentication)
      - [5.2.6 Use the special class MultipartItem as a method parameter, and mark the Multipart annotation on the method to upload the attachment](#526-use-the-special-class-multipartitem-as-a-method-parameter-and-mark-the-multipart-annotation-on-the-method-to-upload-the-attachment)
      - [5.2.7 Use the class Stream as the return type of the method to receive streaming data, such as downloading files.](#527-use-the-class-stream-as-the-return-type-of-the-method-to-receive-streaming-data-such-as-downloading-files)
      - [5.2.8 Use the class HttpResponseMessage as the return type of the method to get the most original response message.](#528-use-the-class-httpresponsemessage-as-the-return-type-of-the-method-to-get-the-most-original-response-message)
      - [5.2.9 Use the class Task as the return type of the method, that is, no return value is required.](#529-use-the-class-task-as-the-return-type-of-the-method-that-is-no-return-value-is-required)
  - [6.Microservice - access to nacos](#6microservice---access-to-nacos)
    - [6.1 Add nacos configuration in the configuration file](#61-add-nacos-configuration-in-the-configuration-file)
    - [6.2 Access nacos configuration center](#62-access-nacos-configuration-center)
    - [6.3 Access nacos service center](#63-access-nacos-service-center)
      - [6.3.1 Add configuration in StartUp.cs](#631-add-configuration-in-startupcs)
      - [6.3.2 Define the interface for calling microservices](#632-define-the-interface-for-calling-microservices)
  - [7.Using cookies in context](#7using-cookies-in-context)
- [SummerBoot uses cache for cache operations](#summerboot-uses-cache-for-cache-operations)
  - [1.Registration service](#1registration-service-2)
  - [2.ICache interface](#2icache-interface)
  - [3.After injecting the interface, it can be used](#3after-injecting-the-interface-it-can-be-used)


# SummerBoot uses repository for database operations
summerBoot has developed its own ORM module based on the work unit and warehousing mode, that is, repository, which supports multiple databases and multiple links, including addition, deletion , modification and query operations of five common database types (sqlserver, mysql, oracle, sqlite, pgsql) , if there are other For database requirements, you can refer to the above 5 source codes and contribute codes to this project.

## Preparation
You need to install the corresponding database dependency package through nuget , such as Microsoft.Data.SqlClient of SqlServer , Mysql.data of mysql , Oracle.ManagedDataAccess.Core of oracle, Npgsql of pgsql

## 1.Registration service
The repository supports multiple databases and multiple links.In the repository, we call a link a database unit.The following example demonstrates adding two database units to a project.The first is the mysql database type, and the second is the sqlserver database type ., add a database unit through the AddDatabaseUnit method, the parameters are the dbConnection class of the corresponding database and the work unit interface (the work unit interface is used for transactions), the framework provides 9 work unit interfaces IUnitOfWork1\~IUnitOfWork9 by default, of course, you can also customize the work unit interface , you only need to inherit the interface from IUnitOfWork .Because there are multiple database units, the warehouse needs to be bound to the corresponding database unit.You can bind a single warehouse through BindRepository , or add a custom annotation on the warehouse (this annotation needs to be inherited from AutoRepositoryAttribute , which is provided by the framework by default.AutoRepository1Attribute~AutoRepository9Attribute), and then use the BindRepositorysWithAttribute method to bind warehouses in batches.At the same time, you can add pre-insert callbacks and pre-update callbacks in the unit (for example, it can be used to add creation time and update time), add custom type mappings, and add custom field mappings Handler, add table name mapping, add field name mapping (table name mapping and field name mapping can be used in situations where database fields and entity class fields are different, such as oracle database, table name field names are all uppercase, but entity classes are Pascal named), etc.

````csharp
builder.Services.AddSummerBoot();

builder.Services.AddSummerBootRepository(it =>
{
		var mysqlDbConnectionString = builder.Configuration.GetValue<string>("mysqlDbConnectionString");
		//Add the first mysql type database unit
		it.AddDatabaseUnit<MySqlConnection, IUnitOfWork1>(mysqlDbConnectionString,
				x =>
				{
						//Add field name mapping
						//x.ColumnNameMapping = (columnName) =>
						//{
						//    return columnName.ToUpper();
						//};

						//Add table name mapping
						//x.TableNameMapping = (tableName) =>
						//{
						//    return tableName.ToUpper();
						//};

						//Bind a single Repository
						//x.BindRepository<IMysqlCustomerRepository,Customer>();

						//Batch binding Repositorys through custom annotations
						x.BindRepositorysWithAttribute<AutoRepository1Attribute>();

						 //Bind database generation interface
						x.BindDbGeneratorType<IDbGenerator1>();

						 //Callback before binding insert
						x.BeforeInsert += entity =>
						{
								if (entity is BaseEntity baseEntity)
								{
										baseEntity.CreateOn = DateTime.Now;
								}
						};

						 //Callback before binding update
						x.BeforeUpdate += entity =>
						{
								if (entity is BaseEntity baseEntity)
								{
										baseEntity.LastUpdateOn = DateTime.Now;
								}
						};
						
						//Add custom type mapping
						//x.SetParameterTypeMap(typeof(DateTime), DbType.DateTime2);

						//Add custom field mapping handler
						//x.SetTypeHandler(typeof(Guid), new GuidTypeHandler());

				});

		//Add a second database unit of type sqlserver
		var sqlServerDbConnectionString = builder.Configuration.GetValue<string>("sqlServerDbConnectionString");
		it.AddDatabaseUnit<SqlConnection, IUnitOfWork2>(sqlServerDbConnectionString,
				x =>
				{
						x.BindRepositorysWithAttribute<AutoRepository2Attribute>();
						x.BindDbGeneratorType<IDbGenerator2>();
				});
});
````


## 2.Define a database entity class
Most of the entity class annotations come from the system namespace System.ComponentModel.DataAnnotations and System.ComponentModel.DataAnnotations.Schema , such as table name Table, column name Column, primary key Key, primary key self-incrementing DatabaseGenerated (DatabaseGeneratedOption.Identity), column name Column, This field NotMapped is not mapped, and Description is used to generate annotations when entity classes generate database tables.At the same time, some annotations are customized, such as ignoring the column IgnoreWhenUpdateAttribute when updating (mainly used for fields that do not need to be updated during update) , let's define a Customer entity class
```` csharp
[Description("Member")]
public class Customer
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { set; get; }
    ///<summary>
    /// Last update time
    ///</summary>
    [Description("Last update time")]
    public DateTime? LastUpdateOn { get; set; }
    ///<summary>
    /// Last updated by
    ///</summary>
    [Description("Last Updater")]
    public string LastUpdateBy { get; set; }
    ///<summary>
    /// Creation time
    ///</summary>
    [IgnoreWhenUpdate]
    [Description("Creation Time")]
    public DateTime? CreateOn { get; set; }
    ///<summary>
    /// founder
    ///</summary>
    [IgnoreWhenUpdate]
    [Description("Creator")]
    public string CreateBy { get; set; }
    ///<summary>
    /// is it effective
    ///</summary>
    [Description("Is it valid")]
    public int? Active { get; set; }
    [Description("Name")]
    public string Name { set; get; }
    [Description("age")]
    public int Age { set; get; }

    [Description("Membership Number")]
    public string CustomerNo { set; get; }

    [Description("total consumption amount")]
    public decimal TotalConsumptionAmount { set; get; }
}
````

SummerBoot comes with a basic entity class BaseEntity (oracle is OracleBaseEntity).The entity class includes five fields: self-increasing id, creator, creation time, updater, update time, and whether it is valid.It is recommended that the entity class directly inherit BaseEntity , then The above Customer can be abbreviated as:

```` csharp
[Description("Member")]
public class Customer : BaseEntity
{
    [Description("Name")]
    public string Name { set; get; }
    [Description("age")]
    public int Age { set; get; }

    [Description("Membership Number")]
    public string CustomerNo { set; get; }

    [Description("total consumption amount")]
    public decimal TotalConsumptionAmount { set; get; }
}
````
## 3.Write a controller to automatically generate database tables through entity classes
```` csharp
[ApiController]
[Route("[controller]/[action]")]
public class GenerateTableController : Controller
{
    private readonly IDbGenerator1 dbGenerator1;

    public GenerateTableController(IDbGenerator1 dbGenerator1)
    {
        this.dbGenerator1 = dbGenerator1;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var results = dbGenerator1.GenerateSql(new List<Type>() { typeof(Customer) });
        foreach (var result in results)
        {
            dbGenerator1.ExecuteGenerateSql(result);
        }
        return Content("ok");
    }
}
````
Access the index interface, and the framework will automatically generate a Customer table with annotations.

## 4.Define storage interface
The storage interface needs to be inherited from the IBaseRepository interface, and the interface is annotated with AutoRepository1 for automatic registration.
```` csharp
[AutoRepository1]
public interface ICustomerRepository : IBaseRepository<Customer>
{
}
````

## 5.Add, delete, modify and Query, all support asynchronous synchronization
### 5.1 Added
#### 5.1.1 The interface has its own Insert method, which can insert a single entity or a list of entities
If the primary key name of the entity class is Id, and there is a Key annotation, and it is self-increasing, as follows:
```` csharp
[Key, DatabaseGenerated (DatabaseGeneratedOption.Identity)]
public int Id { set ; get; }
````
Then after insertion, the framework will automatically assign a value to the ID field of the entity, which is an auto-incremented ID value.
```` csharp
[ApiController]
[Route("[controller]/[action]")]
public class CustomerController : Controller
{
    private readonly ICustomerRepository customerRepository;

    public CustomerController(ICustomerRepository customerRepository)
    {
        this.customerRepository = customerRepository; _
    }

    [HttpGet]
    public async Task<IActionResult> Insert()
    {
        var customer = new Customer() { Name = "testCustomer" };
        await customerRepository.InsertAsync(customer);

        var customer2 = new Customer() { Name = "testCustomer2" };
        var customer3 = new Customer() { Name = "testCustomer3" };
        var customerList = new List<Customer>() { customer2, customer3 };
        await customerRepository.InsertAsync(customerList);

        return Content("ok");
    }
}
````
#### 5.1.2 Fast batch insertion, the storage interface comes with the FastBatchInsert method, which can quickly insert the entity list.
In the case of fast batch insertion, the framework will not automatically assign a value to the ID field of the entity.At the same time, if the database is mysql , there are some special circumstances.First, the driver library must have MySqlConnector .This library can coexist with mysql.data and will not Conflicts, so there is no need to worry, and the database connection string must be followed by"; AllowLoadLocalInfile =true", and at the same time execute"set global local_infile =1"on the mysql database to enable batch upload.SQLite does not support batch fast insert.
```` csharp
var customer2 = new Customer() { Name = "testCustomer2" };
var customer3 = new Customer() { Name = "testCustomer3" };
var customerList = new List<Customer>() { customer2, customer3 };
customerRepository.FastBatchInsert(customerList);
````
### 5.2 delete
#### 5.2.1 The interface comes with a Delete method, which can delete a single entity or a list of entities
```` csharp
customerRepository.Delete(customer);

customerRepository.Delete(customerList);
````
#### 5.2.2 Also supports deletion based on lambda expressions, returning the number of affected rows, for example
```` csharp
var deleteCount = customerRepository.Delete(it => it.Age > 5);
````
### 5.3 update
#### 5.3.1 The interface comes with an Update method, which can update a single entity or a list of entities
Update according to the key primary key.If the primary key is combined, key annotations can be added to multiple fields.
```` csharp
customerRepository.Update(customer);

customerRepository.Update(customerList);
````
#### 5.3.2 It also supports the update method based on Lambda chain syntax
The generated update sql contains only the set fields and returns the number of affected rows, for example
```` csharp
var updateCount = customerRepository.Where(it => it.Name == "testCustomer")
    .SetValue(it => it.Age, 5)
    .SetValue(it => it.TotalConsumptionAmount, 100)
    .ExecuteUpdate();
````

### 5.4 Query
It supports normal query and paged query, and there are two ways to query.
#### 5.4.1 Lambda chain syntax query
##### 5.4.1.1 Single table query
```` csharp
//regular query
var allCustomers = await customerRepository.GetAllAsync();

var customerById = await customerRepository.GetAsync(1);

var customers = await customerRepository.Where(it => it.Age > 5).ToListAsync();

var maxTotalConsumptionAmount = await customerRepository.MaxAsync(it => it.TotalConsumptionAmount);

var names = new List<string>() { "testCustomer", "testCustomer3" };
var customers2 = await customerRepository.Where(it => names.Contains(it.Name)).ToListAsync();

var firstItem = await customerRepository.FirstOrDefaultAsync(it => it.Name == "testCustomer");
// pagination
var pageResult = await customerRepository.Where(it => it.Age > 5).Skip(0).Take(10).ToPageAsync();

var pageable = new Pageable(1, 10);
var pageResult2 = await customerRepository.ToPageAsync(pageable);
````

##### 5.4.1.2 Multiple table joint query
Supports InnerJoin, LeftJoin, RightJoin, WhereIf, OrWhereIf and other extensions, supports up to 4 table joint queries, and supports returning entity types or anonymous types.
````csharp
//Back to list
 var orderList8 = await orderHeaderRepository
     .InnerJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
     .InnerJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
     .InnerJoin(new Address(),it=>it.T4.CustomerId==it.T3.Id &&it.T4.CreateOn== date3)
     .OrderBy(it => it.T2.Quantity)
     .Where(it => it.T3.CustomerNo == "A1")
     .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age,CustomerCity = it.T4.City}, x => x.T1)
     .ToListAsync();

var result7 = await customerRepository
    .LeftJoin(new Address(), it => it.T1.Id == it.T2.CustomerId)
    .Where(it => it.T1.CustomerNo == "A2")
    .OrWhereIf(false, it => it.T2.City == "B")
    .Select(it => new { it.T1.CustomerNo, it.T2.City })
    .ToListAsync();

// pagination
var pageable = new Pageable(1, 5);
var orderPageList = orderHeaderRepository
    .LeftJoin(new OrderDetail(), it => it.T1.Id == it.T2.OrderHeaderId)
    .LeftJoin(new Customer(), it => it.T1.CustomerId == it.T3.Id)
    .LeftJoin(new Address(), it => it.T3.Id == it.T4.CustomerId)
    .OrderBy(it => it.T2.Quantity)
    .Where(it => it.T1.State == 1)
    .Select(it => new OrderDto() { ProductName = it.T2.ProductName, Quantity = it.T2.Quantity, CustomerNo = it.T3.CustomerNo, Age = it.T3.Age, CustomerCity = it.T4.City }, x => x.T1)
    .ToPage(pageable);
````

#### 5.4.2 Define methods directly in the interface, and add annotations to the methods, such as Select, Update, Delete
Then write sql statements in Select, Update, Delete , such as
```` csharp
[AutoRepository1]
public interface ICustomerRepository : IBaseRepository<Customer>
{
    //async
    [Select("select od.productName from customer c join orderHeader oh on c.id= oh.customerid" +
            "join orderDetail od on oh.id= od.OrderHeaderId where c.name=@name")]
    Task<List<CustomerBuyProduct>> QueryAllBuyProductByNameAsync(string name);

    [Select("select * from customer where age>@age order by id")]
    Task<Page<Customer>> GetCustomerByPageAsync(IPageable pageable, int age);

    //sync
    [Select("select od.productName from customer c join orderHeader oh on c.id= oh.customerid" +
            "join orderDetail od on oh.id= od.OrderHeaderId where c.name=@name")]
    List<CustomerBuyProduct> QueryAllBuyProductByName(string name);

    [Select("select * from customer where age>@age order by id")]
    Page<Customer> GetCustomerByPage(IPageable pageable, int age);

}
````
Instructions:

```` csharp
var result = await customerRepository.QueryAllBuyProductByNameAsync("testCustomer");

//page
var pageable = new Pageable(1, 10);
var page = customerRepository.GetCustomerByPage(pageable, 5);
````
> Note: 5.4.2 Pagination support in the query, the return value of the method is wrapped by the Page class, and the method parameter must include the paging parameter IPageable , and the sql statement must also have order by, for example:
```` csharp
[Select("select * from customer where age>@age order by id")]
Page<Customer> GetCustomerByPage(IPageable pageable, int age);
````

The sql in the annotation supports reading from the configuration
The configured json is as follows:
```` json
{
    "mysqlSql": {
        "QueryListSql":"select * from customer",
        "QueryByPageSql":"select * from customer order by age",
        "UpdateByNameSql":"update customer set age=@age where name=@name",
        "DeleteByNameSql":"delete from customer where name=@name"
    }
}
````
The configuration items are wrapped by ${}, and the interface is as follows:
```` csharp
[AutoRepository]
public interface ICustomerTestConfigurationRepository : IBaseRepository<Customer>
{
    //asynchronous
    [Select("${ mysqlSql:QueryListSql }")]
    Task<List<Customer>> QueryListAsync();

    [Select("${ mysqlSql:QueryByPageSql }")]
    Task<Page<Customer>> QueryByPageAsync(IPageable pageable);
    //asynchronous
    [Update("${ mysqlSql:UpdateByNameSql }")]
    Task<int> UpdateByNameAsync(string name, int age);

    [Delete("${ mysqlSql:DeleteByNameSql }")]
    Task<int> DeleteByNameAsync(string name);

    //Synchronize
    [Select("${ mysqlSql:QueryListSql }")]
    List<Customer> QueryList();

    [Select("${ mysqlSql:QueryByPageSql }")]
    Page<Customer> QueryByPage(IPageable pageable);
    //asynchronous
    [Update("${ mysqlSql:UpdateByNameSql }")]
    int UpdateByName(string name, int age);

    [Delete("${ mysqlSql:DeleteByNameSql }")]
    int DeleteByName(string name);
}
````

#### 5.4.4 Select annotations are spliced in this way where query conditions
Wrap a single query condition with {{}}, and only one variable can be included in a condition.At the same time, when defining a method, the parameter is defined as WhereItem \<T\> , and T is a generic parameter, indicating the real parameter type.In this way , summerboot will automatically process the query conditions.The processing rules are as follows.If the active of whereItem is true, the condition is activated, and the query conditions wrapped in {{ }} in the sql statement will expand and participate in the query.If active is false, the sql The query condition wrapped in {{ }} in the statement is automatically replaced with an empty string and does not participate in the query.In order to make whereItem more useful, the WhereBuilder method is provided.The usage example is as follows:
```` csharp
//definition
[AutoRepository]
public interface ICustomerRepository : IBaseRepository<Customer>
{
    [Select("select * from customer where 1=1 {{ and name = @name}}{{ and age = @age}}")]
    Task<List<CustomerBuyProduct>> GetCustomerByConditionAsync(WhereItem<string> name, WhereItem<int> age);

    [Select("select * from customer where 1=1 {{ and name = @name}}{{ and age = @age}} order by id")]
    Task<Page<Customer>> GetCustomerByPageByConditionAsync(IPageable pageable, WhereItem<string> name, WhereItem<int> age);
}

//use
var nameEmpty = WhereBuilder.Empty<string>(); //var nameEmpty = new WhereItem<string>(false,"");

var ageEmpty = WhereBuilder.Empty<int>();
var nameWhereItem = WhereBuilder.HasValue("page5"); // var nameWhereItem = WhereItem<string>(true, "page5");
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

### 5.5 Transaction Support
Use the work unit interface IUnitOfWork to implement database transactions.While injecting the custom storage interface, it also injects the IUnitOfWork interface corresponding to the database unit.Here it is IUnitOfWork1.The usage is as follows

```` csharp
[ApiController]
[Route("[controller]/[action]")]
public class CustomerController : Controller
{
    private readonly ICustomerRepository customerRepository;
    private readonly IUnitOfWork1 unitOfWork1;

    public CustomerController(ICustomerRepository customerRepository, IUnitOfWork1 unitOfWork1)
    {
        this.customerRepository = customerRepository; _
        this.unitOfWork1 = unitOfWork1;
    }

    [HttpGet]
    public async Task<IActionResult> UnitOfWorkTest()
    {
        try
        {
            // start transaction
            unitOfWork1.BeginTransaction();
            var customer = new Customer() { Name = "testCustomer" };
            await customerRepository.InsertAsync(customer);

            var customer2 = new Customer() { Name = "testCustomer2" };
            var customer3 = new Customer() { Name = "testCustomer3" };
            var customerList = new List<Customer>() { customer2, customer3 };
            await customerRepository.InsertAsync(customerList);
            await customerRepository.DeleteAsync(it => it.Age == 0);
            //commit transaction
            unitOfWork1.Commit();
        }
        catch (Exception e)
        {
            // rollback transaction
            unitOfWork1.RollBack();
            throw;
        }

        return Content("ok");
    }
}
````

### 5.6 Custom implementation classes in special cases
#### 5.6.1 Define an interface inherited from IBaseRepository , and define your own methods in the interface
> Note that there is no need to add AutoRepository annotations to this interface at this time
```` csharp
public interface ICustomCustomerRepository : IBaseRepository<Customer>
{
    Task<List<Customer>> GetCustomersAsync(string name);

    Task<Customer> GetCustomerAsync(string name);

    Task<int> UpdateCustomerNameAsync(string oldName, string newName);

    Task<int> CustomQueryAsync();
}
````
#### 5.6.2 Add an implementation class, inherited from the CustomBaseRepository class and the custom ICustomCustomerRepository interface, and add the AutoRegister annotation to the implementation class.
the AutoRegister annotation are the type of the custom interface ICustomCustomerRepository and the declaration cycle ServiceLifetime of the service (the cycle defaults to the scope level).The purpose of adding the AutoRegister annotation is to allow the framework to automatically register the custom interface and the corresponding custom class in the IOC container.It can be directly injected and used.CustomBaseRepository comes with Execute, QueryFirstOrDefault and QueryList and other methods.If you want to contact the lower-level dbConnection for query, refer to the CustomQueryAsync method below.First, OpenDb () opens the database connection, and then query.The query must contain On the database unit information this.databaseUnit and transaction:dbTransaction these two parameters, CloseDb () closes the database connection after the query is completed;

```` csharp
[AutoRegister(typeof(ICustomCustomerRepository))]
public class CustomCustomerRepository : CustomBaseRepository<Customer>, ICustomCustomerRepository
{
    public CustomCustomerRepository(IUnitOfWork1 uow) : base(uow, uow.DbFactory)
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

    public async Task<int> CustomQueryAsync() _
    {
        this.OpenDb();
        var grid = await this.dbConnection.QueryMultipleAsync(this.databaseUnit, "select id from customer", transaction: dbTransaction);
        var ids = grid.Read<int>().ToList();
        this.CloseDb();
        return ids[0];
    }
}
````
#### 5.6.3 Example of use
```` csharp
[ApiController]
[Route("[controller]/[action]")]
public class CustomerController : Controller
{
    private readonly ICustomerRepository customerRepository;
    private readonly IUnitOfWork1 unitOfWork1;
    private readonly ICustomCustomerRepository customCustomerRepository;

    public CustomerController(ICustomerRepository customerRepository, IUnitOfWork1 unitOfWork1, ICustomCustomerRepository customCustomerRepository)
    {
        this.customerRepository = customerRepository; _
        this.unitOfWork1 = unitOfWork1;
        this.customCustomerRepository = customCustomerRepository; _
    }

    [HttpGet]
    public async Task<IActionResult> CustomClass()
    {
        try
        {
            // start transaction
            unitOfWork1.BeginTransaction();
            var customer = new Customer() { Name = "testCustomer" };
            await customerRepository.InsertAsync(customer);

            var customer2 = new Customer() { Name = "testCustomer2" };
            var customer3 = new Customer() { Name = "testCustomer3" };
            var customerList = new List<Customer>() { customer2, customer3 };
            await customerRepository.InsertAsync(customerList);

            var result1 = await customCustomerRepository.GetCustomerAsync("testCustomer");
            var result2 = await customCustomerRepository.CustomQueryAsync();

            var result3 = await customCustomerRepository.UpdateCustomerNameAsync("testCustomer3", "testCustomer33");
            var result4 = await customCustomerRepository.GetCustomersAsync("testCustomer");
            //commit transaction
            unitOfWork1.Commit();
        }
        catch (Exception e)
        {
            // rollback transaction
            unitOfWork1.RollBack();
            throw;
        }

        return Content("ok");
    }
}
````
## 6 Automatically generate entity classes based on database tables, or automatically generate ddl statements for database tables based on entity classes

### 6.1 Table Namespace
Namespaces in sqlserver are schemas, namespaces in oracle are schemas, and namespaces in sqlite and mysql are databases.
If you want to define tables under different namespaces, just add [Table("CustomerWithSchema", Schema ="test")] annotation.
```` csharp
[Table("CustomerWithSchema", Schema = "test")]
public class CustomerWithSchema
{
    public string Name { set; get; }
    public int Age { set; get; }
}
````
### 6.2 Automatically generate the ddl statement of the database table according to the entity class
For usage, please refer to the previous 3 examples.Here, mysql is taken as an example.The generated sql is as follows:
```` sql
CREATE TABLE testSummerboot.`Customer` (
    `Name` text NULL,
    `Age` int NOT NULL,
    `CustomerNo` text NULL,
    `TotalConsumptionAmount` decimal(18,2) NOT NULL,
    `Id` int NOT NULL AUTO_INCREMENT,
    `LastUpdateOn` datetime NULL,
    `LastUpdateBy` text NULL,
    `CreateOn` datetime NULL,
    `CreateBy` text NULL,
    `Active` int NULL,
    PRIMARY KEY (`Id`)
)

ALTER TABLE testSummerboot.`Customer` COMMENT = 'Member'
ALTER TABLE testSummerboot.`Customer` MODIFY `Name` text NULL COMMENT 'Name'
ALTER TABLE testSummerboot.`Customer` MODIFY `Age` int NOT NULL COMMENT 'age'
ALTER TABLE testSummerboot.`Customer` MODIFY `CustomerNo` text NULL COMMENT 'member number'
ALTER TABLE testSummerboot.`Customer` MODIFY `TotalConsumptionAmount` decimal(18,2) NOT NULL COMMENT 'total consumption amount'
ALTER TABLE testSummerboot.`Customer` MODIFY `Id` int NOT NULL AUTO_INCREMENT COMMENT 'Id'
ALTER TABLE testSummerboot.`Customer` MODIFY `LastUpdateOn` datetime NULL COMMENT 'last update time'
ALTER TABLE testSummerboot.`Customer` MODIFY `LastUpdateBy` text NULL COMMENT 'last updated by'
ALTER TABLE testSummerboot.`Customer` MODIFY `CreateOn` datetime NULL COMMENT 'Creation time'
ALTER TABLE testSummerboot.`Customer` MODIFY `CreateBy` text NULL COMMENT 'Created by'
ALTER TABLE testSummerboot.`Customer` MODIFY `Active` int NULL COMMENT 'Valid or not'
````
The generated sql is sql for adding new fields or sql for updating comments .In order to avoid data loss, there will be no sql for deleting fields.It is more convenient for daily use to divide the operation of generating sql and executing sql into two parts.You can quickly get the sql to be executed and check it.After confirming that there is no problem, you can save it and leave it to the dba for review when the application is officially released

### 6.2.2 Type mapping or name mapping from custom entity class fields to database fields
The column annotation is uniformly used here, such as [Column("Age",TypeName ="float")]
```` csharp
[Description("Member")]
public class Customer : BaseEntity
{
    [Description("Name")]
    public string Name { set; get; }
    [Description("age")]
    [Column("Age", TypeName = "float")]
    public int Age { set; get; }

    [Description("Membership Number")]
    public string CustomerNo { set; get; }

    [Description("total consumption amount")]
    public decimal TotalConsumptionAmount { set; get; }
}
````
The generated sql is as follows
````sql
CREATE TABLE testSummerboot.`Customer` (
    `Name` text NULL,
    `Age` float NOT NULL,
    `CustomerNo` text NULL,
    `TotalConsumptionAmount` decimal(18,2) NOT NULL,
    `Id` int NOT NULL AUTO_INCREMENT,
    `LastUpdateOn` datetime NULL,
    `LastUpdateBy` text NULL,
    `CreateOn` datetime NULL,
    `CreateBy` text NULL,
    `Active` int NULL,
    PRIMARY KEY (`Id`)
)

````

### 6.3 Automatically generate entity classes based on database tables
Inject the database unit corresponding to the IDbGenerator interface, here is the IDbGenerator1 interface, call the GenerateCsharpClass method to generate the text of the c# class, the parameters are the collection of database table names and the namespace of the generated entity class, the code is as follows
```` csharp
[ApiController]
[Route("[controller]/[action]")]
public class GenerateTableController : Controller
{
    private readonly IDbGenerator1 dbGenerator1;

    public GenerateTableController(IDbGenerator1 dbGenerator1)
    {
        this.dbGenerator1 = dbGenerator1;
    }

    [HttpGet]
    public async Task<IActionResult> GenerateClass()
    {
        var generateClasses = dbGenerator1.GenerateCsharpClass(new List<string>() { "Customer" }, "Test.Model");
        return Content("ok");
    }

    [HttpGet]
    public IActionResult Index()
    {
        var results = dbGenerator1.GenerateSql(new List<Type>() { typeof(Customer) });
        foreach (var result in results)
        {
            dbGenerator1.ExecuteGenerateSql(result);
        }
        return Content("ok");
    }
}
````
Call the GenerateClass interface, the generated c# entity class is as follows , just create a new class file and paste the text into it
```` csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Test.Model
{
    ///<summary>
    ///member
    ///</summary>
    [Table("Customer")]
    public class Customer
    {
        ///<summary>
        ///Name
        ///</summary>
        [Column("Name")]
        public string Name { get; set; }
        ///<summary>
        ///age
        ///</summary>
        [Column("Age")]
        public int Age { get; set; }
        ///<summary>
        ///Member ID
        ///</summary>
        [Column("CustomerNo")]
        public string CustomerNo { get; set; }
        ///<summary>
        ///total consumption amount
        ///</summary>
        [Column("TotalConsumptionAmount")]
        public decimal TotalConsumptionAmount { get; set; }
        ///<summary>
        ///Id
        ///</summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; }
        ///<summary>
        ///Last update time
        ///</summary>
        [Column("LastUpdateOn")]
        public DateTime? LastUpdateOn { get; set; }
        ///<summary>
        ///Last updated by
        ///</summary>
        [Column("LastUpdateBy")]
        public string LastUpdateBy { get; set; }
        ///<summary>
        ///Creation time
        ///</summary>
        [Column("CreateOn")]
        public DateTime? CreateOn { get; set; }
        ///<summary>
        ///founder
        ///</summary>
        [Column("CreateBy")]
        public string CreateBy { get; set; }
        ///<summary>
        ///is it effective
        ///</summary>
        [Column("Active")]
        public int? Active { get; set; }
    }
}

````

# SummerBoot uses feign to make http calls
> The bottom layer of feign is based on httpClient .

## 1.Registration service
```` csharp
services.AddSummerBoot();
services.AddSummerBootFeign();
````
## 2.Define the interface
Define an interface, and add FeignClient annotation to the interface.In the FeignClient annotation, you can customize the public part of the http interface url - url (the url requested by the entire interface is composed of the url in FeignClient plus the path in the method), whether to ignore the remote interface https certificate verification - IsIgnoreHttpsCertificateValidate , interface timeout - Timeout (unit s), custom interceptor - InterceptorType .

```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/query")]
    Task<Test> TestQuery([Query] Test tt);
}
````
At the same time, the url and path can be obtained by reading the configuration, and the configuration items are wrapped by ${}.The json of the configuration is as follows:
```` json
{
    "configurationTest": {
        "url":"http://localhost:5001/home",
        "path":"/query"
    }
}
````
The interface is as follows:
```` csharp
[FeignClient(Url = "${ configurationTest:url }")]
public interface ITestFeignWithConfiguration
{
    [GetMapping("${ configurationTest:path }")]
    Task<Test> TestQuery([Query] Test tt);
}
````
Sometimes we only want to use the path in the method as a complete url to initiate an http request, then we can define the interface as follows, set UsePathAsUrl to true (the default is false)
```` csharp
[FeignClient(Url = "http://localhost:5001/home")]
public interface ITestFeign
{
    [PostMapping("http://localhost:5001/home/ json", UsePathAsUrl = true)]
    Task TestUsePathAsUrl([Body(BodySerializationKind.Json)] Test tt);
}
````
   

## 3.Set the request header (header)
You can choose to add Headers annotation on the interface, which means that all http requests under this interface will carry the request header in the annotation.The parameter of Headers is a variable-length string type parameter.At the same time, Headers can also be added to the method, which means that when the method is called, the request header will be added.The Headers parameter on the interface can be superimposed with the Headers parameter on the method.At the same time Variables can be used in headers, and the placeholders for variables are {{}}, such as
```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
[Headers("a:a", "b:b")]
public interface ITestFeign
{
    [GetMapping("/ testGet")]
    Task<Test> TestAsync();

    [GetMapping("/ testGetWithHeaders")]
    [Headers("c:c")]
    Task<Test> TestWithHeadersAsync();

    //header replacement
    [Headers("a:{ {methodName}}")]
    [PostMapping("/ abc")]
    Task<Test> TestHeaderAsync(string methodName);
}

await TestFeign.TestAsync()
    >>> get, http: //localhost:5001/home/testGet, headers are"a:a"and"b:b"

await TestFeign.TestWithHeadersAsync()
    >>> get, http: //localhost:5001/home/testGetWithHeaders, headers are"a:a","b:b"and"c:c"

await TestFeign.TestHeaderAsync("abc");
    >>> post, http: //localhost:5001/home/abc, and the request header is"a:abc"
````

## 4.Custom Interceptor
Custom interceptors are effective for all methods under the interface.The application scenario of the interceptor is mainly to do some operations before the request.For example, before requesting a third-party business interface, you need to log in to the third-party system first, then you can use it in the interceptor First request the third-party login interface, after obtaining the credentials, put them in the header, the interceptor needs to implement the IRequestInterceptor interface, the example is as follows
```` csharp
//loginFeign client for login
[FeignClient(Url = "http://localhost:5001/login", IsIgnoreHttpsCertificateValidate = true, Timeout = 100)]
public interface ILoginFeign
{
    [PostMapping("/login")]
    Task<LoginResultDto> LoginAsync([Body()] LoginDto loginDto);
}

//Then customize the login interceptor
public class LoginInterceptor : IRequestInterceptor
{

    private readonly ILoginFeign loginFeign;
    private readonly IConfiguration configuration;

    public LoginInterceptor(ILoginFeign loginFeign, IConfiguration configuration)
    {
        this.loginFeign = loginFeign; _
        this.configuration = configuration;
    }


    public async Task ApplyAsync(RequestTemplate requestTemplate)
    {
        var username = configuration.GetSection("username").Value;
        var password = configuration.GetSection("password").Value;

        var loginResultDto = await this.loginFeign.LoginAsync(new _ LoginDto(){ Name = username, Password = password});
        if (loginResultDto! = null)
        {
            requestTemplate.Headers.Add("Authorization", new List<string>() { "Bearer" + loginResultDto.Token });
        }

        await Task.CompletedTask;
    }
}

//testFegn client that accesses the business interface , and define the interceptor on the client as loginInterceptor
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(LoginInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/ testGet")]
    Task<Test> TestAsync();
}

await TestFeign.TestAsync();
>>> get to http: //localhost:5001/home/testGet, header is"Authorization:Bearer abc"

````
IgnoreInterceptor to the method , then the request initiated by this method will ignore the interceptor, such as
```` csharp
//testFegn client that accesses the business interface , and define the interceptor on the client as loginInterceptor
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(LoginInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/ testGet")]
    [IgnoreInterceptor]
    Task<Test> TestAsync();
}

await TestFeign.TestAsync ();
>>> get to http: //localhost:5001/home/testGet, no header

````
You can also add a global interceptor.When registering AddSummerBootFeign , the calling method is as follows:
```` csharp
services.AddSummerBootFeign(it =>
{
    it.SetGlobalInterceptor(typeof(GlobalInterceptor));
});
````
## 5.Define the method
Each method should add annotations to represent the type of request and the url to be accessed .There are 4 built-in annotations, GetMapping , PostMapping , PutMapping , DeleteMapping , and the return value of the method must be Task<> type
```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/ testGet")]
    Task<Test> TestAsync();

    [PostMapping("/ testPost")]
    Task<Test> TestPostAsync();

    [PutMapping("/ testPut")]
    Task<Test> TestPutAsync();

    [DeleteMapping("/ testDelete")]
    Task<Test> TestDeleteAsync();
}
````

### 5.1 Common parameters in the method
If the parameter has no special annotation, or is not a special class, it will be used as a dynamic parameter to participate in the replacement of variables in the url and header (if the parameter is a class, read the attribute value of the class), and the variables in the url and header use placeholder { {}}, if the variable name is inconsistent with the parameter name, you can use the AliasAs annotation (which can be used on parameters or class attributes) to specify an alias, such as
```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    // url replacement
    [PostMapping("/{{ methodName }}")]
    Task<Test> TestAsync(string methodName);

    //header replacement
    [Headers("a:{ {methodName}}")]
    [PostMapping("/ abc")]
    Task<Test> TestHeaderAsync(string methodName);


    // AliasAs specifies the alias
    [Headers("a:{ {methodName}}")]
    [PostMapping("/ abc")]
    Task<Test> TestAliasAsAsync([AliasAs("methodName")] string name);
}

await TestFeign.TestAsync ("abc");
>>> post to http: //localhost:5001/home/abc

await TestFeign.TestAliasAsAsync ("abc");
>>> post, http: //localhost:5001/home/abc

await TestFeign.TestHeaderAsync ("abc");
>>> post, http: //localhost:5001/home/abc, and the request header is"a:abc"
````

### 5.2 Special parameters in the method
#### 5.2.1 Parameters add Query annotations
After the query annotation is added to the parameter, the parameter value will be added to the url in the form of key1=value1&key2=value2 .
```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/ TestQuery")]
    Task<Test> TestQuery([Query] string name);

    [GetMapping("/ TestQueryWithClass")]
    Task<Test> TestQueryWithClass([Query] Test tt);
}

await TestFeign.TestQuery ("abc");
>>> get, http: //localhost:5001/home/TestQuery?name=abc

await TestFeign.TestQueryWithClass (new Test() { Name ="abc", Age = 3 });
>>> get, http: //localhost:5001/home/TestQueryWithClass?Name=abc&Age=3
````
##### 5.2.1.1 The Query annotation is used with the Embedded annotation, and the Embedded annotation class can be added as a parameter as a whole
```` csharp
public class EmbeddedTest2
{
    public int Age { get; set; }
}

public class EmbeddedTest3
{
    public string Name { get; set; }
    [Embedded]
    public EmbeddedTest2 Test { get; set; }
}

[FeignClient(Url = "http://localhost:5001/home")]
public interface ITestFeign
{
    ///<summary>
    /// Test the Embedded annotation, indicating whether the parameter is embedded, the test is embedded
    ///</summary>
    ///<param name ="tt"></param>
    ///<returns></returns>
    [GetMapping("/ testEmbedded")]
    Task<string> TestEmbedded([Query] EmbeddedTest3 tt);
}
    
await testFeign.TestEmbedded(new EmbeddedTest3()
{
    Name = "sb",
    Test = new EmbeddedTest2()
    {
        Age = 3
    }
});

>>> get, http: //localhost:5001/home/testEmbedded?Name=sb&Test=%7B%22Age%22%3A%223%22%7D

````
If there is no Embedded annotation, the request becomes
```` csharp
>>> get, http: //localhost:5001/home/testEmbedded?Name=sb&Age=3
````

#### 5.2.2 Parameters add Body (BodySerializationKind.Form) annotations
It is equivalent to simulating the form submission in html.The parameter value will be URL-encoded and added to the payload (body) in the form of key1=value1&key2=value2.
```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [PostMapping("/form")]
    Task<Test> TestForm([Body(BodySerializationKind.Form)] Test tt);
}

await TestFeign.TestForm(new Test() { Name = "abc", Age = 3 });
>>> post, http: //localhost:5001/home/form, and the value in the body is Name= abc&Age =3
````

#### 5.2.3 Parameters add Body (BodySerializationKind.Json) annotations
That is, submit it in the form of application/ json , and the parameter value will be serialized by json and added to the payload (body).Similarly, if the field in the class has an alias, you can also use the AliasAs annotation.
```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [PostMapping("/ json")]
    Task<Test> TestJson([Body(BodySerializationKind.Json)] Test tt);
}

await TestFeign.TestJson(new Test() { Name = "abc", Age = 3 });
>>> post, http: //localhost:5001/home/json, and the value in the body is {"Name":"abc","Age":3}
````

#### 5.2.4 Use the special class HeaderCollection as a method parameter to add request headers in batches
```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [PostMapping("/ json")]
    Task<Test> TestJson([Body(BodySerializationKind.Json)] Test tt, HeaderCollection headers);
}

var headerCollection = new HeaderCollection()
{ new KeyValuePair<string , string>("a","a"),
    new KeyValuePair<string , string>("b","b") };

await TestFeign.TestJson(new Test() { Name = "abc", Age = 3 }, headerCollection);
>>> post, http: //localhost:5001/home/json, and the value in the body is {"Name":"abc","Age":3}, and the header is"a:a"and"b: b"
````

#### 5.2.5 Use the special class BasicAuthorization as a method parameter to add the Authorization request header for basic authentication
```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/ testBasicAuthorization")]
    Task<Test> TestBasicAuthorization(BasicAuthorization basicAuthorization);
}

var username = "abc";
var password = "123";

await TestFeign.TestBasicAuthorization(new BasicAuthorization(username, password));
>>> get, http: //localhost:5001/home/testBasicAuthorization, header is"Authorization: Basic YWJjOjEyMw =="
````

#### 5.2.6 Use the special class MultipartItem as a method parameter, and mark the Multipart annotation on the method to upload the attachment
```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    //Only upload the file
    [Multipart]
    [PostMapping("/multipart")]
    Task<Test> MultipartTest(MultipartItem item);
    //While uploading the attachment, you can also attach parameters
    [Multipart]
    [PostMapping("/multipart")]
    Task<Test> MultipartTest([Body(BodySerializationKind.Form)] Test tt, MultipartItem item);
}
// Only upload the file
var basePath = Path.Combine(AppContext.BaseDirectory, "123.txt");
var name = "file";
var fileName = "123.txt";
//Method 1, use byteArray
var byteArray = File.ReadAllBytes(basePath);
var result = await testFeign.MultipartTest(new MultipartItem(byteArray, name, fileName));

//Method 2, use stream
var fileStream = new FileInfo(basePath).OpenRead();
var result = await testFeign.MultipartTest(new MultipartItem(fileStream, name, fileName));

//Method 3, use fileInfo
var result = await testFeign.MultipartTest(new MultipartItem(new FileInfo(basePath), name, fileName));

>>> post, http: //localhost:5001/home/multipart, with attachments in the body
 
 //While uploading the attachment, you can also attach parameters
var basePath = Path.Combine(AppContext.BaseDirectory, "123.txt");
var name = "file";
var fileName = "123.txt";
//Method 1, use byteArray
var byteArray = File.ReadAllBytes(basePath);
var result = await testFeign.MultipartTest(new Test() { Name = "sb", Age = 3 }, new MultipartItem(byteArray, name, fileName));

//Method 2, use stream
var fileStream = new FileInfo(basePath).OpenRead();
var result = await testFeign.MultipartTest(new Test() { Name = "sb", Age = 3 }, new MultipartItem(fileStream, name, fileName));

//Method 3, use fileInfo
var result = await testFeign.MultipartTest(new Test() { Name = "sb", Age = 3 }, new MultipartItem(new FileInfo(basePath), name, fileName));
 
>>> post, http: //localhost:5001/home/multipart, and the value in the body is Name= abc&Age =3, and it has attachments
````

#### 5.2.7 Use the class Stream as the return type of the method to receive streaming data, such as downloading files.
```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/ downLoadWithStream")]
    Task<Stream> TestDownLoadWithStream();
}

using var streamResult = await testFeign.TestDownLoadStream();
using var newfile = new FileInfo("D:\\123.txt").OpenWrite();
streamResult.CopyTo(newfile);

>>> get, http: //localhost:5001/home/downLoadWithStream, the return value is streaming data, and then it can be saved as a file.
````

#### 5.2.8 Use the class HttpResponseMessage as the return type of the method to get the most original response message.
```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/test")]
    Task<HttpResponseMessage> Test();
}

var rawResult = await testFeign.Test();

>>> get, http: //localhost:5001/home/Test, the return value is the original return data of httpclient .
````

#### 5.2.9 Use the class Task as the return type of the method, that is, no return value is required.
```` csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/test")]
    Task Test();
}

await testFeign.Test();

>>> get, http: //localhost:5001/home/Test, ignore the return value
````
## 6.Microservice - access to nacos
### 6.1 Add nacos configuration in the configuration file
in appsettings.json / appsettings.Development.json configuration file
```` json
"nacos": {
    //Username and password are required only when nacos enables authentication, otherwise they are empty.
    "username": "",
    "password": "",
    //--------Using nacos , serviceAddress and namespaceId are required------
    // nacos service address, such as http://172.16.189.242:8848
    "serviceAddress":"http://172.16.189.242:8848/",
    // Namespace id, such as 832e754e-e845-47db-8acc-46ae3819b638 or public
   "namespaceId":"dfd8de72-e5ec-4595-91d4-49382f500edf",

    //--------If you just access the microservices in nacos , you only need to configure lbStrategy , defaultNacosGroupName and defaultNacosNamespaceId are optional ------
       //Client load balancing algorithm, there are multiple instances under one service, lbStrategy is used to select instances under the service, the default is Random (random), you can also choose WeightRandom (random after being weighted according to the service weight)
      "lbStrategy":"Random",
       // defaultNacosGroupName , optional, is the default value of NacosGroupName in the FeignClient annotation , if it is empty, it defaults to DEFAULT_GROUP
      "defaultNacosGroupName":"",
       // defaultNacosNamespaceId , optional, is the default value of NacosNamespaceId in the FeignClient annotation , if it is empty, it defaults to public
      "defaultNacosNamespaceId":"",

    //--------If you need to use the nacos configuration center, ConfigurationOption is required，Allows listening to multiple configurations------
    "configurationOption": [
      {
        //Configuration namespaceId
        "namespaceId": "f3dfa56a-a72c-4035-9612-1f9a8ca6f1d2",
        //Configuration grouping
        "groupName": "DEFAULT_GROUP",
         //configured dataId ,
        "dataId": "def"
      },
      {
        "namespaceId": "public",
        //配置的分组
        "groupName": "DEFAULT_GROUP",
        //配置的dataId,
        "dataId": "abc"
      }
    ],
    //-------If you want to register this application as a service instance, all parameters need to be configured --------------

    //Do you want to register the application as a service instance
   "registerInstance": true ,

    //The name of the service to be registered
   "serviceName":"test",
    //Service group name
   "groupName":"DEFAULT_GROUP",
    //Weight, there are multiple instances under one service, the higher the weight, the greater the probability of accessing the instance, for example, if some instances are located on a server with high configuration, then the weight can be higher, and more traffic will be diverted to the instance, which is the same as the above The parameter lbStrategy is set to WeightRandom for use with
   "weight": 1 ,
    //The external network protocol of this application, http or https
   "protocol":"http",
    //The external port number of this application, such as 5000
   "port": 5000

}
````
### 6.2 Access nacos configuration center
Accessing the nacos configuration center is very simple, just add a line .UseNacosConfiguration () in Program.cs , currently supports json format, xml format and yaml format.

net core3.1 example is as follows
```` csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseNacosConfiguration()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>().UseUrls("http://*:5001");
        });
````
The net6 example is as follows
```` csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseNacosConfiguration();
````
### 6.3 Access nacos service center
#### 6.3.1 Add configuration in StartUp.cs
If the current application is registered as a microservice instance, then this step is over, and feign will automatically register the application as a microservice instance according to the configuration in the configuration file.If this application wants to call the microservice interface, please see 6.3.2

```` csharp
services.AddSummerBoot();
services.AddSummerBootFeign(it =>
{
    it.AddNacos(Configuration);
});
````

#### 6.3.2 Define the interface for calling microservices
Set the name of the microservice ServiceName , the group name NacosGroupName (you can fill in the global default group name in the configuration file nacos:defaultNacosGroupName , if you don’t fill it in, it defaults to DEFAULT_GROUP), the namespace NacosNamespaceId (you can fill in the global default namespace in the configuration file nacos:defaultNacosNamespaceId , If not filled, it defaults to public), and MicroServiceMode is set to true.The url does not need to be configured, and the rest is the same as the normal feign interface.

```` csharp
[FeignClient(ServiceName = "test", MicroServiceMode = true, NacosGroupName = "DEFAULT_GROUP", NacosNamespaceId = "dfd8de72-e5ec-4595-91d4-49382f500edf")]
public interface IFeignService
{
    [GetMapping("/home/index")]
    Task<string> TestGet();
}
````
At the same time , ServiceName , NacosGroupName , and NacosNamespaceId also support reading from configuration files, such as
```` json
{
    "ServiceName":"test",
    "NacosGroupName":"DEFAULT_GROUP",
    "NacosNamespaceId":"dfd8de72-e5ec-4595-91d4-49382f500edf"
}
````
```` csharp
[FeignClient(ServiceName = "${ ServiceName }", MicroServiceMode = true, NacosGroupName = "${ NacosGroupName }", NacosNamespaceId = "${ NacosNamespaceId }")]
public interface IFeignService
{
    [GetMapping("/home/index")]
    Task<string> TestGet();
}
````
## 7.Using cookies in context
In the unit of work mode in feign, cookies can be set in the context, so that the interface will automatically bring cookies when the interface initiates an http request in the context.To use the unit of work mode, you need to inject the IFeignUnitOfWork interface, and then operate as follows :
```` csharp
var feignUnitOfWork = serviceProvider.GetRequiredService<IFeignUnitOfWork>();
//Open the context
feignUnitOfWork.BeginCookie();
//add cookie
feignUnitOfWork.AddCookie("http://localhost:5001/home/TestCookieContainer2", "abc =1");
await testFeign.TestCookieContainer2();
//end context
feignUnitOfWork.StopCookie();

````
At the same time, if the interface returns the cookie setting information, the work unit will also save the cookie, and when the interface in the context scope initiates http access, it will automatically bring the cookie information.A typical scenario is that we first After the first interface is logged in, the interface will return the cookie to us.When we visit the subsequent interface, we must bring the cookie returned to us by the first interface.:
```` csharp
var feignUnitOfWork = serviceProvider.GetRequiredService<IFeignUnitOfWork>();
//Open the context
feignUnitOfWork.BeginCookie();

// get cookie after login
await testFeign.LoginAsync("sb", "123");
    / / Automatically bring the cookie after login when requesting
await testFeign.TestCookieContainer3();
//end context
feignUnitOfWork.StopCookie();
````
# SummerBoot uses cache for cache operations
## 1.Registration service
The cache is divided into memory cache and redis cache.The memory cache registration method is as follows:
```` csharp
services.AddSummerBoot();
services.AddSummerBootCache(it => it.UseMemory());
````
The redis cache registration method is as follows, connectionString is the redis connection string:
```` csharp
services.AddSummerBoot();
services.AddSummerBootCache(it =>
{
    it.UseRedis(connectionString);
});
````
## 2.ICache interface
The ICache interface mainly has the following methods and the corresponding asynchronous methods
```` csharp
///<summary>
/// Absolute time cache, the cache value becomes invalid after a fixed time
///</summary>
///<typeparam name ="T"></typeparam>
///<param name ="key"></param>
///<param name ="value"></param>
///<param name ="absoluteExpiration"></param>
///<returns></returns>
bool SetValueWithAbsolute<T>(string key, T value, TimeSpan absoluteExpiration);
///<summary>
/// Sliding time cache, if there is a hit within the time, the time will continue to be extended, and the cache value will be invalid if it is not hit
///</summary>
///<typeparam name ="T"></typeparam>
///<param name ="key"></param>
///<param name ="value"></param>
///<param name ="slidingExpiration"></param>
///<returns></returns>
bool SetValueWithSliding<T>(string key, T value, TimeSpan slidingExpiration);
///<summary>
/// get value
///</summary>
///<typeparam name ="T"></typeparam>
///<param name ="key"></param>
///<returns></returns>
CacheEntity<T> GetValue<T>(string key);
///<summary>
/// remove value
///</summary>
///<param name ="key"></param>
///<returns></returns>
bool Remove(string key);
````
## 3.After injecting the interface, it can be used
```` csharp
var cache = serviceProvider.GetRequiredService<ICache>();
    / / Set a fixed time cache
cache.SetValueWithAbsolute("test", "test", TimeSpan.FromSeconds(3));
//Set sliding time cache
var cache = serviceProvider.GetRequiredService<ICache>();
cache.SetValueWithSliding("test", "test", TimeSpan.FromSeconds(3));
// get cache
var value = cache.GetValue<string>("test");
// remove cache
cache.Remove("test");
````

Human-friendly design in #SummerBoot
function that comes with net core mvc .What if we want to configure the ip and port of the web application in appsettings.json ? Write directly in appsettings.json
````
{
"urls":"http://localhost:7002;http://localhost:7012"
}
````

 2.The AutoRegister annotation is used to let the framework automatically register the interface and the implementation class of the interface into the IOC container, and mark it on the implementation class.The parameters of the annotation are the type of the custom interface corresponding to this class and the service life cycle ServiceLifetime (the cycle defaults to scope level), the usage is as follows:
```` csharp
public interface ITest
{

}

[AutoRegister(typeof(ITest), ServiceLifetime.Transient)]
public class Test : ITest
{

}
````
 3.ApiResult interface return value packaging class, including code, msg and data, 3 fields, so that the return value of the entire system is unified and orderly, which is conducive to the unified interception and unified operation of the front end.The way to use it is as follows:
```` csharp
[HttpPost("CreateServerConfigAsync")]
public async Task<ApiResult<bool>> CreateServerConfigAsync(ServerConfigDto dto)
{
    var result = await serverConfigService.CreateServerConfigAsync(dto);
    return ApiResult<bool>.Ok(result);
}
````
 4.Some enhanced operations for net core mvc , including the global error interceptor, and the processing after the interface parameter verification fails, cooperate with ApiResult , so that when the system reports an error, it can also return uniformly.The usage is as follows.First, register this in startUp Service, note that it should be placed after mvc registration:
```` csharp
services.AddControllersWithViews();
services.AddSummerBootMvcExtension(it =>
{
    // Whether to enable global error handling
    it.UseGlobalExceptionHandle = true;
    //Whether to enable parameter verification processing
    it.UseValidateParameterHandle = true;
});
````
4.1 The effect of using the global error interceptor
We can throw an error directly in the business code, and the global error interceptor will catch the error, and then return it to the front end in a unified format.The business code is as follows:
```` csharp
private void ValidateData(EnvConfigDto dto)
{
    if (dto == null)
    {
        throw new ArgumentNullException("Argument cannot be empty");
    }
    if (dto.ServerConfigs == null || dto.ServerConfigs.Count == 0)
    {
        throw new ArgumentNullException("There is no server configured in the environment");
    }
}
````
If an error is reported in the business code, the return value is as follows:
```` csharp
{
 "code": 40000 ,
 "msg":"Value cannot be null.(Parameter 'There is no server configured in the environment')",
 "data": null
}
````
4.2 The effect of processing after the interface parameter verification fails
add verification annotations in the parameter dto of the interface , the code is as follows
```` csharp
public class EnvConfigDto : BaseEntity
{
    ///<summary>
    /// Environment name
    ///</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Environment name cannot be empty")]
    public string Name { get; set; }
    ///<summary>
    /// The corresponding server in the environment
    ///</summary>
    [NotMapped]
    public List<int> ServerConfigs _ { get ; set ; }
}
````
If the parameter verification fails, the return value is as follows:
```` csharp
{
 "code": 40000 ,
 "msg":"Environment name cannot be empty",
 "data": null
}
````

5.QueryCondition , the combination of lambda query conditions, solves the pain point of filtering and querying from the front-end .In addition to the basic And and Or methods , a more humanized method is added.Generally, the attributes in the dto passed from the front-end have string types.If they have values, they are added to the query condition, so two methods are specially extracted, including AndIfStringIsNotEmpty (if the string is not empty, the and operation is performed, otherwise the original expression is returned), OrIfStringIsNotEmpty (if the string is not empty, then Perform or operation, otherwise return to the original expression),
At the same time , the attributes in dto may also be of nullable type, that is, nullable type, such as int? test represents whether the user fills in a certain filter condition, if hasValue is added to the query condition, so two methods are specially extracted, AndIfNullableHasValue (If the nullable value is not empty, the AND operation is performed, otherwise the original expression is returned), OrIfNullableHasValue (if the nullable value is not empty, the AND operation is performed, otherwise the original expression is returned) usage is as follows:
```` csharp
// dto
public class ServerConfigPageDto : IPageable
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    ///<summary>
    /// ip address
    ///</summary>
    public string Ip { get; set; }
    ///<summary>
    /// connection name
    ///</summary>
    public string ConnectionName { get; set; }

    public int? Test { get; set; }
}
//condition
var queryCondition = QueryCondition.True<ServerConfig>()
    .And(it => it.Active == 1)
    //If the string is not empty, perform the and operation, otherwise return the original expression
    .AndIfStringIsNotEmpty(dto.Ip, it => it.Ip.Contains(dto.Ip))
    //If the nullable value is not empty, perform the and operation, otherwise return to the original expression
    .AndIfNullableHasValue(dto.Test, it => it.Test == dto.Test)
    .AndIfStringIsNotEmpty(dto.ConnectionName, it => it.ConnectionName.Contains(dto.ConnectionName));

var queryResult = await serverConfigRepository.Where(queryCondition)
    .Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize).ToPageAsync();
````


