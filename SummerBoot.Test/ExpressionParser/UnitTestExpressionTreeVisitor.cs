
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using SummerBoot.Repository;
using SummerBoot.Repository.Attributes;
using SummerBoot.Repository.ExpressionParser.Parser;
using SummerBoot.Test;
using SummerBoot.Test.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Oracle.ManagedDataAccess.Client;
using SummerBoot.Core;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Priority;

namespace ExpressionParser.Test
{
    [Table("PERSON")]
    public class OraclePerson
    {
        [Column("NAME")]
        public string Name { get; set; }
        [Key]
        [Column("AGE")]
        public int Age { get; set; }
        [Column("HAVECHILDREN")]
        public bool HaveChildren { get; set; }
    }
    public class Person
    {
        public string Name { get; set; }
        [Key]
        public int Age { get; set; }
        public bool HaveChildren { get; set; }
    }

    public class Employee
    {
        [Key]
        public int Id { set; get; }
        public string Name { get; set; }
        public int Age { get; set; }
        public bool HaveChildren { get; set; }
    }

    [Table("Hr2")]
    public class Hr
    {
        [Key]
        [Column("hrEmployeeNo")]
        public string EmployeNo { set; get; }
        [Key]
        public string Name { get; set; }
        public int Age { get; set; }
        public bool HaveChildren { get; set; }

        [IgnoreWhenUpdate]
        public DateTime CreateOn { set; get; }
    }
    public class Pet
    {
        public string Name { get; set; }
        public int Age;
        public string Address;
    }

    public class Dog
    {
        public Dog()
        {

        }

        public Dog(string name, int? active)
        {
            Name = name;
            Active = active;
        }
        public string Name { get; set; }
        public int? Active { get; set; }

    }

    public class Dog2
    {
        public Dog2()
        {

        }
        public Dog2(string name, int? active, Enum2? enum2)
        {
            Name = name;
            Active = active;
            Enum2 = enum2;
        }
        public string Name { get; set; }
        public int? Active { get; set; }
        public Enum2? Enum2 { get; set; }

    }

    public class DogRepository : Repository<Dog>
    {
        public DogRepository() : base(new DatabaseUnit(typeof(UnitOfWork), typeof(SqlConnection), ""))
        {

        }
    }


    public class EmployeeRepository : Repository<Employee>
    {
        public EmployeeRepository() : base(new DatabaseUnit(typeof(UnitOfWork), typeof(SqlConnection), "") { SqlServerVersion = 10 })
        {

        }
    }

    public class HrRepository : Repository<Hr>
    {
        public HrRepository() : base(new DatabaseUnit(typeof(UnitOfWork), typeof(SqlConnection), "") { SqlServerVersion = 10 })
        {

        }
    }

    public class PersonRepository : Repository<Person>, IPersonRepository
    {
        public PersonRepository() : base(new DatabaseUnit(typeof(UnitOfWork), typeof(SqlConnection), "") { SqlServerVersion = 10 })
        {

        }
        public void ExecuteDelete()
        {

        }
    }
    public class SqlServer14PersonRepository : Repository<Person>, IPersonRepository
    {
        public SqlServer14PersonRepository() : base(new DatabaseUnit(typeof(UnitOfWork), typeof(SqlConnection), "") { SqlServerVersion = 14 })
        {

        }
        public void ExecuteDelete()
        {

        }
    }


    public class MysqlPersonRepository : Repository<Person>, IPersonRepository
    {
        public MysqlPersonRepository() : base(new DatabaseUnit(typeof(UnitOfWork), typeof(MySqlConnection), ""))
        {

        }
        public void ExecuteDelete()
        {

        }
    }
    public interface IOraclePersonRepository : IRepository<OraclePerson>
    {

    }
    public class OraclePersonRepository : Repository<OraclePerson>, IOraclePersonRepository
    {
        public OraclePersonRepository(DatabaseUnit databaseUnit) : base(databaseUnit)
        {

        }
    }
    public interface IPersonRepository : IRepository<Person>
    {
        void ExecuteDelete();
    }

    /// <summary>
    /// 测试new datetime这种情况
    /// </summary>
    public class TestWhereNewDatetime
    {
        public DateTime Time { get; set; }
    }
    public interface ITestWhereNewDatetimeRepository : IRepository<TestWhereNewDatetime>
    {

    }
    public class TestWhereNewDatetimeRepository : Repository<TestWhereNewDatetime>, ITestWhereNewDatetimeRepository
    {
        public TestWhereNewDatetimeRepository() : base(new DatabaseUnit(typeof(UnitOfWork), typeof(SqlConnection), ""))
        {

        }
    }
    public class UnitTestExpressionTreeVisitor
    {

        [Fact]
        public async Task TestCount()
        {
            var dogRepository = new DogRepository();
            var r1 = dogRepository.Count(it => it.Name == "sb");
            var r1MiddleResult = dogRepository.GetParsingResult();
            Assert.Equal("select count(*) from [Dog] as [p0] where([p0].[Name] = @y0)", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("sb", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestNullable()
        {
            var dogRepository = new DogRepository();
            var r1 = dogRepository.Where(it => it.Active == 1).ToList();
            var r1MiddleResult = dogRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Active] as [Active] from [Dog] as [p0] where([p0].[Active] = 1)", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.Parameters.GetParamInfos);
        }

        [Fact]
        public void TestIsNull()
        {
            var dogRepository = new DogRepository();
            var r1 = dogRepository.Where(it => it.Name == null).ToList();
            var r1MiddleResult = dogRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Active] as [Active] from [Dog] as [p0] where([p0].[Name] is null)", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.Parameters.GetParamInfos);
        }

        [Fact]
        public void TestIsNull2()
        {
            var dogRepository = new DogRepository();
            var r1 = dogRepository.Where(it => null == it.Name).ToList();
            var r1MiddleResult = dogRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Active] as [Active] from [Dog] as [p0] where([p0].[Name] is null)", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.Parameters.GetParamInfos);
        }
        [Fact]
        public void TestIsNotNull()
        {
            var dogRepository = new DogRepository();
            var r1 = dogRepository.Where(it => it.Name != null).ToList();
            var r1MiddleResult = dogRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Active] as [Active] from [Dog] as [p0] where([p0].[Name] is not null)", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.Parameters.GetParamInfos);
        }
        [Fact]
        public void TestIsNotNull2()
        {
            var dogRepository = new DogRepository();
            var r1 = dogRepository.Where(it => null != it.Name).ToList();
            var r1MiddleResult = dogRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Active] as [Active] from [Dog] as [p0] where([p0].[Name] is not null)", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.Parameters.GetParamInfos);
        }
        [Fact]
        public void TestNullable2()
        {
            var dogRepository = new DogRepository();
            var r1 = dogRepository.Where(it => it.Active == 1 && it.Name == "hzp").ToList();
            var r1MiddleResult = dogRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Active] as [Active] from [Dog] as [p0] where(([p0].[Active] = 1) and([p0].[Name] = @y0))", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }
        [Fact]
        public void TestSelect()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Select(it => it).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select * from [Person] as [p0]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.Parameters.GetParamInfos);
        }

        [Fact]
        public void TestSelect2()
        {
            var personRepository = new PersonRepository();
            var r2 = personRepository.Select(it => it.Name).ToList();
            var r2MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] from [Person] as [p0]", r2MiddleResult.Sql);
            Assert.Empty(r2MiddleResult.Parameters.GetParamInfos);
        }


        [Fact]
        public void TestSelect3()
        {
            var personRepository = new PersonRepository();
            var r3 = personRepository.Select(it => new { it.Name, Address = "福建省" }).ToList();
            var r3MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], '福建省' as [Address] from [Person] as [p0]", r3MiddleResult.Sql);
            Assert.Empty(r3MiddleResult.Parameters.GetParamInfos);
        }

        [Fact]
        public void TestSelect4()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "Dog" };
            var r4 = personRepository.Select(it => new { it.Name, Address = pet.Name }).ToList();
            var r4MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], 'Dog' as [Address] from [Person] as [p0]", r4MiddleResult.Sql);
            Assert.Empty(r4MiddleResult.Parameters.GetParamInfos);


        }

        [Fact]
        public void TestWhere2()
        {

            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.HaveChildren && it.Name == "hzp" && it.Age == 15).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where((([p0].[HaveChildren] = 1) and([p0].[Name] = @y0)) and([p0].[Age] = 15))", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere3()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.HaveChildren == false && it.Name == "hzp" && it.Age == 15).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where((([p0].[HaveChildren] = 0) and([p0].[Name] = @y0)) and([p0].[Age] = 15))", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere4()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp" && it.HaveChildren == false && it.Age == 15).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where((([p0].[Name] = @y0) and([p0].[HaveChildren] = 0)) and([p0].[Age] = 15))", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere5()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp" && it.HaveChildren && it.Age == 15).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where((([p0].[Name] = @y0) and([p0].[HaveChildren] = 1)) and([p0].[Age] = 15))", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere6()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.HaveChildren && it.HaveChildren).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(([p0].[HaveChildren] = 1) and([p0].[HaveChildren] = 1))", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.Parameters.GetParamInfos);
        }
        [Fact]
        public void TestWhere7()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => !it.HaveChildren).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where not(([p0].[HaveChildren] = 1))", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.Parameters.GetParamInfos);
        }
        [Fact]
        public void TestWhere8()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => !!it.HaveChildren).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where not(not(([p0].[HaveChildren] = 1)))", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.Parameters.GetParamInfos);
        }

        [Fact]
        public void TestWhere9()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.Length > 3).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(len([p0].[Name]) > 3)", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.Parameters.GetParamInfos);
        }

        [Fact]
        public void TestWhere10()
        {
            var pet = new Pet() { Name = "cat" };
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == pet.Name).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where([p0].[Name] = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("cat", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere11()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp" && (it.HaveChildren && it.Age == 15)).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(([p0].[Name] = @y0) and(([p0].[HaveChildren] = 1) and([p0].[Age] = 15)))", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere12()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => (it.Name == "hzp" && it.HaveChildren) && it.Age == 15).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where((([p0].[Name] = @y0) and([p0].[HaveChildren] = 1)) and([p0].[Age] = 15))", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);

        }

        [Fact]
        public void TestWhere13()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.Contains("hzp")).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where([p0].[Name] like @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("%hzp%", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere14()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.Contains(pet.Name)).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where([p0].[Name] like @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("%hzp%", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere15()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.StartsWith("hzp")).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where([p0].[Name] like @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp%", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere16()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.StartsWith(pet.Name)).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where([p0].[Name] like @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp%", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere17()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.EndsWith("hzp")).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where([p0].[Name] like @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("%hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere18()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.EndsWith(pet.Name)).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where([p0].[Name] like @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("%hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }
        [Fact]
        public void TestWhere19()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.Trim() == "hzp").ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(trim([p0].[Name]) = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere20()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.Trim() == pet.Name).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(trim([p0].[Name]) = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }
        [Fact]
        public void TestWhere21()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.TrimStart() == "hzp").ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(ltrim([p0].[Name]) = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere22()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.TrimStart() == pet.Name).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(ltrim([p0].[Name]) = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }
        [Fact]
        public void TestWhere23()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.TrimEnd() == "hzp").ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(rtrim([p0].[Name]) = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere24()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.TrimEnd() == pet.Name).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(rtrim([p0].[Name]) = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }
        [Fact]
        public void TestWhere25()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.Equals("hzp")).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where([p0].[Name] = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere26()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.Equals(pet.Name)).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where([p0].[Name] = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere27()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Address = "hzp" };
            var r1 = personRepository.Where(it => it.Name.Equals(pet.Address)).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where([p0].[Name] = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }


        [Fact]
        public void TestWhere29()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.ToLower() == pet.Name).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(lower([p0].[Name]) = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }
        [Fact]
        public void TestWhere30()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.ToLower() == "hzp").ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(lower([p0].[Name]) = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestWhere31()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.ToUpper() == pet.Name).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(upper([p0].[Name]) = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }
        [Fact]
        public void TestWhere32()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.ToUpper() == "hzp").ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(upper([p0].[Name]) = @y0)", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.Parameters.GetParamInfos);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        /// <summary>
        /// colloection
        /// </summary>
        [Fact]
        public void TestWhere33()
        {
            var personRepository = new PersonRepository();
            var nameList = new List<string>() { "hzp", "qy" };
            var r1 = personRepository.Where(it => nameList.Contains(it.Name)).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where [p0].[Name] in(@y0,@y1)", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal("qy", r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        /// <summary>
        /// colloection
        /// </summary>
        [Fact]
        public void TestWhere34()
        {
            var personRepository = new PersonRepository();
            var nameList = new string[] { "hzp", "qy" };
            var r1 = personRepository.Where(it => nameList.Contains(it.Name)).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where [p0].[Name] in(@y0,@y1)", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal("qy", r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        /// <summary>
        /// colloection
        /// </summary>
        [Fact]
        public void TestWhere35()
        {
            var personRepository = new PersonRepository();
            var nameList = Enumerable.Range(0, 1001).Select(it => "hzp" + it).ToList();

            var r1 = personRepository.Where(it => nameList.Contains(it.Name)).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(([p0].[Name] in(@y0,@y1,@y2,@y3,@y4,@y5,@y6,@y7,@y8,@y9,@y10,@y11,@y12,@y13,@y14,@y15,@y16,@y17,@y18,@y19,@y20,@y21,@y22,@y23,@y24,@y25,@y26,@y27,@y28,@y29,@y30,@y31,@y32,@y33,@y34,@y35,@y36,@y37,@y38,@y39,@y40,@y41,@y42,@y43,@y44,@y45,@y46,@y47,@y48,@y49,@y50,@y51,@y52,@y53,@y54,@y55,@y56,@y57,@y58,@y59,@y60,@y61,@y62,@y63,@y64,@y65,@y66,@y67,@y68,@y69,@y70,@y71,@y72,@y73,@y74,@y75,@y76,@y77,@y78,@y79,@y80,@y81,@y82,@y83,@y84,@y85,@y86,@y87,@y88,@y89,@y90,@y91,@y92,@y93,@y94,@y95,@y96,@y97,@y98,@y99,@y100,@y101,@y102,@y103,@y104,@y105,@y106,@y107,@y108,@y109,@y110,@y111,@y112,@y113,@y114,@y115,@y116,@y117,@y118,@y119,@y120,@y121,@y122,@y123,@y124,@y125,@y126,@y127,@y128,@y129,@y130,@y131,@y132,@y133,@y134,@y135,@y136,@y137,@y138,@y139,@y140,@y141,@y142,@y143,@y144,@y145,@y146,@y147,@y148,@y149,@y150,@y151,@y152,@y153,@y154,@y155,@y156,@y157,@y158,@y159,@y160,@y161,@y162,@y163,@y164,@y165,@y166,@y167,@y168,@y169,@y170,@y171,@y172,@y173,@y174,@y175,@y176,@y177,@y178,@y179,@y180,@y181,@y182,@y183,@y184,@y185,@y186,@y187,@y188,@y189,@y190,@y191,@y192,@y193,@y194,@y195,@y196,@y197,@y198,@y199,@y200,@y201,@y202,@y203,@y204,@y205,@y206,@y207,@y208,@y209,@y210,@y211,@y212,@y213,@y214,@y215,@y216,@y217,@y218,@y219,@y220,@y221,@y222,@y223,@y224,@y225,@y226,@y227,@y228,@y229,@y230,@y231,@y232,@y233,@y234,@y235,@y236,@y237,@y238,@y239,@y240,@y241,@y242,@y243,@y244,@y245,@y246,@y247,@y248,@y249,@y250,@y251,@y252,@y253,@y254,@y255,@y256,@y257,@y258,@y259,@y260,@y261,@y262,@y263,@y264,@y265,@y266,@y267,@y268,@y269,@y270,@y271,@y272,@y273,@y274,@y275,@y276,@y277,@y278,@y279,@y280,@y281,@y282,@y283,@y284,@y285,@y286,@y287,@y288,@y289,@y290,@y291,@y292,@y293,@y294,@y295,@y296,@y297,@y298,@y299,@y300,@y301,@y302,@y303,@y304,@y305,@y306,@y307,@y308,@y309,@y310,@y311,@y312,@y313,@y314,@y315,@y316,@y317,@y318,@y319,@y320,@y321,@y322,@y323,@y324,@y325,@y326,@y327,@y328,@y329,@y330,@y331,@y332,@y333,@y334,@y335,@y336,@y337,@y338,@y339,@y340,@y341,@y342,@y343,@y344,@y345,@y346,@y347,@y348,@y349,@y350,@y351,@y352,@y353,@y354,@y355,@y356,@y357,@y358,@y359,@y360,@y361,@y362,@y363,@y364,@y365,@y366,@y367,@y368,@y369,@y370,@y371,@y372,@y373,@y374,@y375,@y376,@y377,@y378,@y379,@y380,@y381,@y382,@y383,@y384,@y385,@y386,@y387,@y388,@y389,@y390,@y391,@y392,@y393,@y394,@y395,@y396,@y397,@y398,@y399,@y400,@y401,@y402,@y403,@y404,@y405,@y406,@y407,@y408,@y409,@y410,@y411,@y412,@y413,@y414,@y415,@y416,@y417,@y418,@y419,@y420,@y421,@y422,@y423,@y424,@y425,@y426,@y427,@y428,@y429,@y430,@y431,@y432,@y433,@y434,@y435,@y436,@y437,@y438,@y439,@y440,@y441,@y442,@y443,@y444,@y445,@y446,@y447,@y448,@y449,@y450,@y451,@y452,@y453,@y454,@y455,@y456,@y457,@y458,@y459,@y460,@y461,@y462,@y463,@y464,@y465,@y466,@y467,@y468,@y469,@y470,@y471,@y472,@y473,@y474,@y475,@y476,@y477,@y478,@y479,@y480,@y481,@y482,@y483,@y484,@y485,@y486,@y487,@y488,@y489,@y490,@y491,@y492,@y493,@y494,@y495,@y496,@y497,@y498,@y499)or [p0].[Name] in(@y500,@y501,@y502,@y503,@y504,@y505,@y506,@y507,@y508,@y509,@y510,@y511,@y512,@y513,@y514,@y515,@y516,@y517,@y518,@y519,@y520,@y521,@y522,@y523,@y524,@y525,@y526,@y527,@y528,@y529,@y530,@y531,@y532,@y533,@y534,@y535,@y536,@y537,@y538,@y539,@y540,@y541,@y542,@y543,@y544,@y545,@y546,@y547,@y548,@y549,@y550,@y551,@y552,@y553,@y554,@y555,@y556,@y557,@y558,@y559,@y560,@y561,@y562,@y563,@y564,@y565,@y566,@y567,@y568,@y569,@y570,@y571,@y572,@y573,@y574,@y575,@y576,@y577,@y578,@y579,@y580,@y581,@y582,@y583,@y584,@y585,@y586,@y587,@y588,@y589,@y590,@y591,@y592,@y593,@y594,@y595,@y596,@y597,@y598,@y599,@y600,@y601,@y602,@y603,@y604,@y605,@y606,@y607,@y608,@y609,@y610,@y611,@y612,@y613,@y614,@y615,@y616,@y617,@y618,@y619,@y620,@y621,@y622,@y623,@y624,@y625,@y626,@y627,@y628,@y629,@y630,@y631,@y632,@y633,@y634,@y635,@y636,@y637,@y638,@y639,@y640,@y641,@y642,@y643,@y644,@y645,@y646,@y647,@y648,@y649,@y650,@y651,@y652,@y653,@y654,@y655,@y656,@y657,@y658,@y659,@y660,@y661,@y662,@y663,@y664,@y665,@y666,@y667,@y668,@y669,@y670,@y671,@y672,@y673,@y674,@y675,@y676,@y677,@y678,@y679,@y680,@y681,@y682,@y683,@y684,@y685,@y686,@y687,@y688,@y689,@y690,@y691,@y692,@y693,@y694,@y695,@y696,@y697,@y698,@y699,@y700,@y701,@y702,@y703,@y704,@y705,@y706,@y707,@y708,@y709,@y710,@y711,@y712,@y713,@y714,@y715,@y716,@y717,@y718,@y719,@y720,@y721,@y722,@y723,@y724,@y725,@y726,@y727,@y728,@y729,@y730,@y731,@y732,@y733,@y734,@y735,@y736,@y737,@y738,@y739,@y740,@y741,@y742,@y743,@y744,@y745,@y746,@y747,@y748,@y749,@y750,@y751,@y752,@y753,@y754,@y755,@y756,@y757,@y758,@y759,@y760,@y761,@y762,@y763,@y764,@y765,@y766,@y767,@y768,@y769,@y770,@y771,@y772,@y773,@y774,@y775,@y776,@y777,@y778,@y779,@y780,@y781,@y782,@y783,@y784,@y785,@y786,@y787,@y788,@y789,@y790,@y791,@y792,@y793,@y794,@y795,@y796,@y797,@y798,@y799,@y800,@y801,@y802,@y803,@y804,@y805,@y806,@y807,@y808,@y809,@y810,@y811,@y812,@y813,@y814,@y815,@y816,@y817,@y818,@y819,@y820,@y821,@y822,@y823,@y824,@y825,@y826,@y827,@y828,@y829,@y830,@y831,@y832,@y833,@y834,@y835,@y836,@y837,@y838,@y839,@y840,@y841,@y842,@y843,@y844,@y845,@y846,@y847,@y848,@y849,@y850,@y851,@y852,@y853,@y854,@y855,@y856,@y857,@y858,@y859,@y860,@y861,@y862,@y863,@y864,@y865,@y866,@y867,@y868,@y869,@y870,@y871,@y872,@y873,@y874,@y875,@y876,@y877,@y878,@y879,@y880,@y881,@y882,@y883,@y884,@y885,@y886,@y887,@y888,@y889,@y890,@y891,@y892,@y893,@y894,@y895,@y896,@y897,@y898,@y899,@y900,@y901,@y902,@y903,@y904,@y905,@y906,@y907,@y908,@y909,@y910,@y911,@y912,@y913,@y914,@y915,@y916,@y917,@y918,@y919,@y920,@y921,@y922,@y923,@y924,@y925,@y926,@y927,@y928,@y929,@y930,@y931,@y932,@y933,@y934,@y935,@y936,@y937,@y938,@y939,@y940,@y941,@y942,@y943,@y944,@y945,@y946,@y947,@y948,@y949,@y950,@y951,@y952,@y953,@y954,@y955,@y956,@y957,@y958,@y959,@y960,@y961,@y962,@y963,@y964,@y965,@y966,@y967,@y968,@y969,@y970,@y971,@y972,@y973,@y974,@y975,@y976,@y977,@y978,@y979,@y980,@y981,@y982,@y983,@y984,@y985,@y986,@y987,@y988,@y989,@y990,@y991,@y992,@y993,@y994,@y995,@y996,@y997,@y998,@y999))or [p0].[Name] in(@y1000))", r1MiddleResult.Sql);
            Assert.Equal(1001, r1MiddleResult.Parameters.GetParamInfos.Count);
            for (int i = 0; i < 1001; i++)
            {
                Assert.Equal("hzp" + i, r1MiddleResult.Parameters.GetParamInfos["y" + i].Value);
            }
        }

        /// <summary>
        /// colloection
        /// </summary>
        [Fact]
        public void TestWhere36()
        {
            var personRepository = new PersonRepository();
            var nameList = Enumerable.Range(0, 1001).Select(it => "hzp" + it).ToList();

            var r1 = personRepository.Where(it => nameList.Contains(it.Name) && it.HaveChildren).ToArray();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where((([p0].[Name] in(@y0,@y1,@y2,@y3,@y4,@y5,@y6,@y7,@y8,@y9,@y10,@y11,@y12,@y13,@y14,@y15,@y16,@y17,@y18,@y19,@y20,@y21,@y22,@y23,@y24,@y25,@y26,@y27,@y28,@y29,@y30,@y31,@y32,@y33,@y34,@y35,@y36,@y37,@y38,@y39,@y40,@y41,@y42,@y43,@y44,@y45,@y46,@y47,@y48,@y49,@y50,@y51,@y52,@y53,@y54,@y55,@y56,@y57,@y58,@y59,@y60,@y61,@y62,@y63,@y64,@y65,@y66,@y67,@y68,@y69,@y70,@y71,@y72,@y73,@y74,@y75,@y76,@y77,@y78,@y79,@y80,@y81,@y82,@y83,@y84,@y85,@y86,@y87,@y88,@y89,@y90,@y91,@y92,@y93,@y94,@y95,@y96,@y97,@y98,@y99,@y100,@y101,@y102,@y103,@y104,@y105,@y106,@y107,@y108,@y109,@y110,@y111,@y112,@y113,@y114,@y115,@y116,@y117,@y118,@y119,@y120,@y121,@y122,@y123,@y124,@y125,@y126,@y127,@y128,@y129,@y130,@y131,@y132,@y133,@y134,@y135,@y136,@y137,@y138,@y139,@y140,@y141,@y142,@y143,@y144,@y145,@y146,@y147,@y148,@y149,@y150,@y151,@y152,@y153,@y154,@y155,@y156,@y157,@y158,@y159,@y160,@y161,@y162,@y163,@y164,@y165,@y166,@y167,@y168,@y169,@y170,@y171,@y172,@y173,@y174,@y175,@y176,@y177,@y178,@y179,@y180,@y181,@y182,@y183,@y184,@y185,@y186,@y187,@y188,@y189,@y190,@y191,@y192,@y193,@y194,@y195,@y196,@y197,@y198,@y199,@y200,@y201,@y202,@y203,@y204,@y205,@y206,@y207,@y208,@y209,@y210,@y211,@y212,@y213,@y214,@y215,@y216,@y217,@y218,@y219,@y220,@y221,@y222,@y223,@y224,@y225,@y226,@y227,@y228,@y229,@y230,@y231,@y232,@y233,@y234,@y235,@y236,@y237,@y238,@y239,@y240,@y241,@y242,@y243,@y244,@y245,@y246,@y247,@y248,@y249,@y250,@y251,@y252,@y253,@y254,@y255,@y256,@y257,@y258,@y259,@y260,@y261,@y262,@y263,@y264,@y265,@y266,@y267,@y268,@y269,@y270,@y271,@y272,@y273,@y274,@y275,@y276,@y277,@y278,@y279,@y280,@y281,@y282,@y283,@y284,@y285,@y286,@y287,@y288,@y289,@y290,@y291,@y292,@y293,@y294,@y295,@y296,@y297,@y298,@y299,@y300,@y301,@y302,@y303,@y304,@y305,@y306,@y307,@y308,@y309,@y310,@y311,@y312,@y313,@y314,@y315,@y316,@y317,@y318,@y319,@y320,@y321,@y322,@y323,@y324,@y325,@y326,@y327,@y328,@y329,@y330,@y331,@y332,@y333,@y334,@y335,@y336,@y337,@y338,@y339,@y340,@y341,@y342,@y343,@y344,@y345,@y346,@y347,@y348,@y349,@y350,@y351,@y352,@y353,@y354,@y355,@y356,@y357,@y358,@y359,@y360,@y361,@y362,@y363,@y364,@y365,@y366,@y367,@y368,@y369,@y370,@y371,@y372,@y373,@y374,@y375,@y376,@y377,@y378,@y379,@y380,@y381,@y382,@y383,@y384,@y385,@y386,@y387,@y388,@y389,@y390,@y391,@y392,@y393,@y394,@y395,@y396,@y397,@y398,@y399,@y400,@y401,@y402,@y403,@y404,@y405,@y406,@y407,@y408,@y409,@y410,@y411,@y412,@y413,@y414,@y415,@y416,@y417,@y418,@y419,@y420,@y421,@y422,@y423,@y424,@y425,@y426,@y427,@y428,@y429,@y430,@y431,@y432,@y433,@y434,@y435,@y436,@y437,@y438,@y439,@y440,@y441,@y442,@y443,@y444,@y445,@y446,@y447,@y448,@y449,@y450,@y451,@y452,@y453,@y454,@y455,@y456,@y457,@y458,@y459,@y460,@y461,@y462,@y463,@y464,@y465,@y466,@y467,@y468,@y469,@y470,@y471,@y472,@y473,@y474,@y475,@y476,@y477,@y478,@y479,@y480,@y481,@y482,@y483,@y484,@y485,@y486,@y487,@y488,@y489,@y490,@y491,@y492,@y493,@y494,@y495,@y496,@y497,@y498,@y499)or [p0].[Name] in(@y500,@y501,@y502,@y503,@y504,@y505,@y506,@y507,@y508,@y509,@y510,@y511,@y512,@y513,@y514,@y515,@y516,@y517,@y518,@y519,@y520,@y521,@y522,@y523,@y524,@y525,@y526,@y527,@y528,@y529,@y530,@y531,@y532,@y533,@y534,@y535,@y536,@y537,@y538,@y539,@y540,@y541,@y542,@y543,@y544,@y545,@y546,@y547,@y548,@y549,@y550,@y551,@y552,@y553,@y554,@y555,@y556,@y557,@y558,@y559,@y560,@y561,@y562,@y563,@y564,@y565,@y566,@y567,@y568,@y569,@y570,@y571,@y572,@y573,@y574,@y575,@y576,@y577,@y578,@y579,@y580,@y581,@y582,@y583,@y584,@y585,@y586,@y587,@y588,@y589,@y590,@y591,@y592,@y593,@y594,@y595,@y596,@y597,@y598,@y599,@y600,@y601,@y602,@y603,@y604,@y605,@y606,@y607,@y608,@y609,@y610,@y611,@y612,@y613,@y614,@y615,@y616,@y617,@y618,@y619,@y620,@y621,@y622,@y623,@y624,@y625,@y626,@y627,@y628,@y629,@y630,@y631,@y632,@y633,@y634,@y635,@y636,@y637,@y638,@y639,@y640,@y641,@y642,@y643,@y644,@y645,@y646,@y647,@y648,@y649,@y650,@y651,@y652,@y653,@y654,@y655,@y656,@y657,@y658,@y659,@y660,@y661,@y662,@y663,@y664,@y665,@y666,@y667,@y668,@y669,@y670,@y671,@y672,@y673,@y674,@y675,@y676,@y677,@y678,@y679,@y680,@y681,@y682,@y683,@y684,@y685,@y686,@y687,@y688,@y689,@y690,@y691,@y692,@y693,@y694,@y695,@y696,@y697,@y698,@y699,@y700,@y701,@y702,@y703,@y704,@y705,@y706,@y707,@y708,@y709,@y710,@y711,@y712,@y713,@y714,@y715,@y716,@y717,@y718,@y719,@y720,@y721,@y722,@y723,@y724,@y725,@y726,@y727,@y728,@y729,@y730,@y731,@y732,@y733,@y734,@y735,@y736,@y737,@y738,@y739,@y740,@y741,@y742,@y743,@y744,@y745,@y746,@y747,@y748,@y749,@y750,@y751,@y752,@y753,@y754,@y755,@y756,@y757,@y758,@y759,@y760,@y761,@y762,@y763,@y764,@y765,@y766,@y767,@y768,@y769,@y770,@y771,@y772,@y773,@y774,@y775,@y776,@y777,@y778,@y779,@y780,@y781,@y782,@y783,@y784,@y785,@y786,@y787,@y788,@y789,@y790,@y791,@y792,@y793,@y794,@y795,@y796,@y797,@y798,@y799,@y800,@y801,@y802,@y803,@y804,@y805,@y806,@y807,@y808,@y809,@y810,@y811,@y812,@y813,@y814,@y815,@y816,@y817,@y818,@y819,@y820,@y821,@y822,@y823,@y824,@y825,@y826,@y827,@y828,@y829,@y830,@y831,@y832,@y833,@y834,@y835,@y836,@y837,@y838,@y839,@y840,@y841,@y842,@y843,@y844,@y845,@y846,@y847,@y848,@y849,@y850,@y851,@y852,@y853,@y854,@y855,@y856,@y857,@y858,@y859,@y860,@y861,@y862,@y863,@y864,@y865,@y866,@y867,@y868,@y869,@y870,@y871,@y872,@y873,@y874,@y875,@y876,@y877,@y878,@y879,@y880,@y881,@y882,@y883,@y884,@y885,@y886,@y887,@y888,@y889,@y890,@y891,@y892,@y893,@y894,@y895,@y896,@y897,@y898,@y899,@y900,@y901,@y902,@y903,@y904,@y905,@y906,@y907,@y908,@y909,@y910,@y911,@y912,@y913,@y914,@y915,@y916,@y917,@y918,@y919,@y920,@y921,@y922,@y923,@y924,@y925,@y926,@y927,@y928,@y929,@y930,@y931,@y932,@y933,@y934,@y935,@y936,@y937,@y938,@y939,@y940,@y941,@y942,@y943,@y944,@y945,@y946,@y947,@y948,@y949,@y950,@y951,@y952,@y953,@y954,@y955,@y956,@y957,@y958,@y959,@y960,@y961,@y962,@y963,@y964,@y965,@y966,@y967,@y968,@y969,@y970,@y971,@y972,@y973,@y974,@y975,@y976,@y977,@y978,@y979,@y980,@y981,@y982,@y983,@y984,@y985,@y986,@y987,@y988,@y989,@y990,@y991,@y992,@y993,@y994,@y995,@y996,@y997,@y998,@y999))or [p0].[Name] in(@y1000))and([p0].[HaveChildren] = 1))", r1MiddleResult.Sql);
            Assert.Equal(1001, r1MiddleResult.Parameters.GetParamInfos.Count);
            for (int i = 0; i < 1001; i++)
            {
                Assert.Equal("hzp" + i, r1MiddleResult.Parameters.GetParamInfos["y" + i].Value);
            }

        }

        [Fact]
        public void TestWhere37()
        {
            var personRepository = new PersonRepository();
            var nameList = Enumerable.Range(0, 1001).Select(it => "hzp" + it).ToList();

            var r1 = personRepository.Where(it => nameList.Contains(it.Name) && it.HaveChildren && it.Age == 15).ToArray();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(((([p0].[Name] in(@y0,@y1,@y2,@y3,@y4,@y5,@y6,@y7,@y8,@y9,@y10,@y11,@y12,@y13,@y14,@y15,@y16,@y17,@y18,@y19,@y20,@y21,@y22,@y23,@y24,@y25,@y26,@y27,@y28,@y29,@y30,@y31,@y32,@y33,@y34,@y35,@y36,@y37,@y38,@y39,@y40,@y41,@y42,@y43,@y44,@y45,@y46,@y47,@y48,@y49,@y50,@y51,@y52,@y53,@y54,@y55,@y56,@y57,@y58,@y59,@y60,@y61,@y62,@y63,@y64,@y65,@y66,@y67,@y68,@y69,@y70,@y71,@y72,@y73,@y74,@y75,@y76,@y77,@y78,@y79,@y80,@y81,@y82,@y83,@y84,@y85,@y86,@y87,@y88,@y89,@y90,@y91,@y92,@y93,@y94,@y95,@y96,@y97,@y98,@y99,@y100,@y101,@y102,@y103,@y104,@y105,@y106,@y107,@y108,@y109,@y110,@y111,@y112,@y113,@y114,@y115,@y116,@y117,@y118,@y119,@y120,@y121,@y122,@y123,@y124,@y125,@y126,@y127,@y128,@y129,@y130,@y131,@y132,@y133,@y134,@y135,@y136,@y137,@y138,@y139,@y140,@y141,@y142,@y143,@y144,@y145,@y146,@y147,@y148,@y149,@y150,@y151,@y152,@y153,@y154,@y155,@y156,@y157,@y158,@y159,@y160,@y161,@y162,@y163,@y164,@y165,@y166,@y167,@y168,@y169,@y170,@y171,@y172,@y173,@y174,@y175,@y176,@y177,@y178,@y179,@y180,@y181,@y182,@y183,@y184,@y185,@y186,@y187,@y188,@y189,@y190,@y191,@y192,@y193,@y194,@y195,@y196,@y197,@y198,@y199,@y200,@y201,@y202,@y203,@y204,@y205,@y206,@y207,@y208,@y209,@y210,@y211,@y212,@y213,@y214,@y215,@y216,@y217,@y218,@y219,@y220,@y221,@y222,@y223,@y224,@y225,@y226,@y227,@y228,@y229,@y230,@y231,@y232,@y233,@y234,@y235,@y236,@y237,@y238,@y239,@y240,@y241,@y242,@y243,@y244,@y245,@y246,@y247,@y248,@y249,@y250,@y251,@y252,@y253,@y254,@y255,@y256,@y257,@y258,@y259,@y260,@y261,@y262,@y263,@y264,@y265,@y266,@y267,@y268,@y269,@y270,@y271,@y272,@y273,@y274,@y275,@y276,@y277,@y278,@y279,@y280,@y281,@y282,@y283,@y284,@y285,@y286,@y287,@y288,@y289,@y290,@y291,@y292,@y293,@y294,@y295,@y296,@y297,@y298,@y299,@y300,@y301,@y302,@y303,@y304,@y305,@y306,@y307,@y308,@y309,@y310,@y311,@y312,@y313,@y314,@y315,@y316,@y317,@y318,@y319,@y320,@y321,@y322,@y323,@y324,@y325,@y326,@y327,@y328,@y329,@y330,@y331,@y332,@y333,@y334,@y335,@y336,@y337,@y338,@y339,@y340,@y341,@y342,@y343,@y344,@y345,@y346,@y347,@y348,@y349,@y350,@y351,@y352,@y353,@y354,@y355,@y356,@y357,@y358,@y359,@y360,@y361,@y362,@y363,@y364,@y365,@y366,@y367,@y368,@y369,@y370,@y371,@y372,@y373,@y374,@y375,@y376,@y377,@y378,@y379,@y380,@y381,@y382,@y383,@y384,@y385,@y386,@y387,@y388,@y389,@y390,@y391,@y392,@y393,@y394,@y395,@y396,@y397,@y398,@y399,@y400,@y401,@y402,@y403,@y404,@y405,@y406,@y407,@y408,@y409,@y410,@y411,@y412,@y413,@y414,@y415,@y416,@y417,@y418,@y419,@y420,@y421,@y422,@y423,@y424,@y425,@y426,@y427,@y428,@y429,@y430,@y431,@y432,@y433,@y434,@y435,@y436,@y437,@y438,@y439,@y440,@y441,@y442,@y443,@y444,@y445,@y446,@y447,@y448,@y449,@y450,@y451,@y452,@y453,@y454,@y455,@y456,@y457,@y458,@y459,@y460,@y461,@y462,@y463,@y464,@y465,@y466,@y467,@y468,@y469,@y470,@y471,@y472,@y473,@y474,@y475,@y476,@y477,@y478,@y479,@y480,@y481,@y482,@y483,@y484,@y485,@y486,@y487,@y488,@y489,@y490,@y491,@y492,@y493,@y494,@y495,@y496,@y497,@y498,@y499)or [p0].[Name] in(@y500,@y501,@y502,@y503,@y504,@y505,@y506,@y507,@y508,@y509,@y510,@y511,@y512,@y513,@y514,@y515,@y516,@y517,@y518,@y519,@y520,@y521,@y522,@y523,@y524,@y525,@y526,@y527,@y528,@y529,@y530,@y531,@y532,@y533,@y534,@y535,@y536,@y537,@y538,@y539,@y540,@y541,@y542,@y543,@y544,@y545,@y546,@y547,@y548,@y549,@y550,@y551,@y552,@y553,@y554,@y555,@y556,@y557,@y558,@y559,@y560,@y561,@y562,@y563,@y564,@y565,@y566,@y567,@y568,@y569,@y570,@y571,@y572,@y573,@y574,@y575,@y576,@y577,@y578,@y579,@y580,@y581,@y582,@y583,@y584,@y585,@y586,@y587,@y588,@y589,@y590,@y591,@y592,@y593,@y594,@y595,@y596,@y597,@y598,@y599,@y600,@y601,@y602,@y603,@y604,@y605,@y606,@y607,@y608,@y609,@y610,@y611,@y612,@y613,@y614,@y615,@y616,@y617,@y618,@y619,@y620,@y621,@y622,@y623,@y624,@y625,@y626,@y627,@y628,@y629,@y630,@y631,@y632,@y633,@y634,@y635,@y636,@y637,@y638,@y639,@y640,@y641,@y642,@y643,@y644,@y645,@y646,@y647,@y648,@y649,@y650,@y651,@y652,@y653,@y654,@y655,@y656,@y657,@y658,@y659,@y660,@y661,@y662,@y663,@y664,@y665,@y666,@y667,@y668,@y669,@y670,@y671,@y672,@y673,@y674,@y675,@y676,@y677,@y678,@y679,@y680,@y681,@y682,@y683,@y684,@y685,@y686,@y687,@y688,@y689,@y690,@y691,@y692,@y693,@y694,@y695,@y696,@y697,@y698,@y699,@y700,@y701,@y702,@y703,@y704,@y705,@y706,@y707,@y708,@y709,@y710,@y711,@y712,@y713,@y714,@y715,@y716,@y717,@y718,@y719,@y720,@y721,@y722,@y723,@y724,@y725,@y726,@y727,@y728,@y729,@y730,@y731,@y732,@y733,@y734,@y735,@y736,@y737,@y738,@y739,@y740,@y741,@y742,@y743,@y744,@y745,@y746,@y747,@y748,@y749,@y750,@y751,@y752,@y753,@y754,@y755,@y756,@y757,@y758,@y759,@y760,@y761,@y762,@y763,@y764,@y765,@y766,@y767,@y768,@y769,@y770,@y771,@y772,@y773,@y774,@y775,@y776,@y777,@y778,@y779,@y780,@y781,@y782,@y783,@y784,@y785,@y786,@y787,@y788,@y789,@y790,@y791,@y792,@y793,@y794,@y795,@y796,@y797,@y798,@y799,@y800,@y801,@y802,@y803,@y804,@y805,@y806,@y807,@y808,@y809,@y810,@y811,@y812,@y813,@y814,@y815,@y816,@y817,@y818,@y819,@y820,@y821,@y822,@y823,@y824,@y825,@y826,@y827,@y828,@y829,@y830,@y831,@y832,@y833,@y834,@y835,@y836,@y837,@y838,@y839,@y840,@y841,@y842,@y843,@y844,@y845,@y846,@y847,@y848,@y849,@y850,@y851,@y852,@y853,@y854,@y855,@y856,@y857,@y858,@y859,@y860,@y861,@y862,@y863,@y864,@y865,@y866,@y867,@y868,@y869,@y870,@y871,@y872,@y873,@y874,@y875,@y876,@y877,@y878,@y879,@y880,@y881,@y882,@y883,@y884,@y885,@y886,@y887,@y888,@y889,@y890,@y891,@y892,@y893,@y894,@y895,@y896,@y897,@y898,@y899,@y900,@y901,@y902,@y903,@y904,@y905,@y906,@y907,@y908,@y909,@y910,@y911,@y912,@y913,@y914,@y915,@y916,@y917,@y918,@y919,@y920,@y921,@y922,@y923,@y924,@y925,@y926,@y927,@y928,@y929,@y930,@y931,@y932,@y933,@y934,@y935,@y936,@y937,@y938,@y939,@y940,@y941,@y942,@y943,@y944,@y945,@y946,@y947,@y948,@y949,@y950,@y951,@y952,@y953,@y954,@y955,@y956,@y957,@y958,@y959,@y960,@y961,@y962,@y963,@y964,@y965,@y966,@y967,@y968,@y969,@y970,@y971,@y972,@y973,@y974,@y975,@y976,@y977,@y978,@y979,@y980,@y981,@y982,@y983,@y984,@y985,@y986,@y987,@y988,@y989,@y990,@y991,@y992,@y993,@y994,@y995,@y996,@y997,@y998,@y999))or [p0].[Name] in(@y1000))and([p0].[HaveChildren] = 1)) and([p0].[Age] = 15))", r1MiddleResult.Sql);
            Assert.Equal(1001, r1MiddleResult.Parameters.GetParamInfos.Count);
            for (int i = 0; i < 1001; i++)
            {
                Assert.Equal("hzp" + i, r1MiddleResult.Parameters.GetParamInfos["y" + i].Value);
            }
        }

        [Fact]
        public void TestWhere38()
        {
            var testWhereNewDatetimeRepository = new TestWhereNewDatetimeRepository();
            var p0 = new DateTime(2024, 7, 23);
            var result = testWhereNewDatetimeRepository.Where(it => it.Time == new DateTime(2024, 7, 23)).ToList();
            var r1MiddleResult = testWhereNewDatetimeRepository.GetParsingResult();
            Assert.Equal("select [p0].[Time] as [Time] from [TestWhereNewDatetime] as [p0] where([p0].[Time] = @y0)", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(p0, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);

        }

        [Fact]
        public void TestWhere39()
        {
            var testWhereNewDatetimeRepository = new TestWhereNewDatetimeRepository();
            var p0 = new DateTime();
            var result = testWhereNewDatetimeRepository.Where(it => it.Time == new TestWhereNewDatetime().Time).ToList();
            var r1MiddleResult = testWhereNewDatetimeRepository.GetParsingResult();
            Assert.Equal("select [p0].[Time] as [Time] from [TestWhereNewDatetime] as [p0] where([p0].[Time] = @y0)", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(p0, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestCombineSelectAndWhere()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp").Select(it => it).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select * from [Person] as [p0] where([p0].[Name] = @y0)", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestCombineSelectAndWhere2()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp").Select(it => new { it.Age, Address = "fujian" }).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();


            Assert.Equal("select [p0].[Age] as [Age], 'fujian' as [Address] from [Person] as [p0] where([p0].[Name] = @y0)", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestCombineSelectAndWhere3()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp" && it.HaveChildren).Select(it => new { it.Age, it.HaveChildren }).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(([p0].[Name] = @y0) and([p0].[HaveChildren] = 1))", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestCombineSelectAndWhere4()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => true && it.Name == "hzp" && it.HaveChildren).Select(it => new { it.Age, it.HaveChildren }).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(((1 = 1) and([p0].[Name] = @y0)) and([p0].[HaveChildren] = 1))", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
        }

        [Fact]
        public void TestCombineSelectAndWhere5()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp" && true && it.HaveChildren).Select(it => new { it.Age, it.HaveChildren }).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where((([p0].[Name] = @y0) and(1 = 1)) and([p0].[HaveChildren] = 1))", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);

        }

        [Fact]
        public void TestOrderBy()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderBy(it => it.Age).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] order by [p0].[Age] asc", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }

        [Fact]
        public void TestOrderBy2()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderByDescending(it => it.Age).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] order by [p0].[Age] desc", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }
        [Fact]
        public void TestOrderBy3()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderBy(it => it.Age).ThenBy(it => it.Name).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] order by [p0].[Age] asc, [p0].[Name] asc", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }

        [Fact]
        public void TestOrderBy4()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderBy(it => it.Age).ThenByDescending(it => it.Name).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] order by [p0].[Age] asc, [p0].[Name] desc", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }

        [Fact]
        public void TestOrderBy5()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderByDescending(it => it.Age).ThenByDescending(it => it.Name).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] order by [p0].[Age] desc, [p0].[Name] desc", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }

        [Fact]
        public void TestOrderBy6()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderByDescending(it => it.Age).ThenBy(it => it.Name).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] order by [p0].[Age] desc, [p0].[Name] asc", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }


        [Fact]
        public void TestGroupBy()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.GroupBy(it => it.Name).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] order by [p0].[Name]", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }

        [Fact]
        public void TestGroupBy2()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.GroupBy(it => new { it.Name, it.Age }).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] order by [p0].[Name], [p0].[Age]", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }
        [Fact]
        public void TestGroupBy3()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.GroupBy(it => it.Name).Select(g => new { Key = g.Key, Count = g.Count(), Uv = g.Sum(it => it.Age) }).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p0].[Name] as [Key], Count([p0].[Name]) as [Count], Sum([p0].[Age]) as [Uv] from [Person] as [p0] group by [p0].[Name]", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }

        [Fact]
        public void TestGroupBy4()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.GroupBy(it => it.Name).Select(g => new { Key = g.Key, Count = g.Count(), Uv = g.Sum(it => it.Age) }).ToDictionary(it => it.Key);
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p0].[Name] as [Key], Count([p0].[Name]) as [Count], Sum([p0].[Age]) as [Uv] from [Person] as [p0] group by [p0].[Name]", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }

        [Fact]
        public void TestGroupBy5()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.GroupBy(it => new { it.Name, it.Age }).Select(g => new { Key = g.Key, Count = g.Count(), Uv = g.Sum(it => it.Age) }).ToDictionary(it => it.Key);
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p0].[Age] as [Key], Count([p0].[Age]) as [Count], Sum([p0].[Age]) as [Uv] from [Person] as [p0] group by [p0].[Name], [p0].[Age]", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }

        [Fact]
        public void TestGroupBy6()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.GroupBy(it => new { it.Name, it.Age }).Select(g => new { Key = g.Key, Count = g.Count(), Uv = g.Sum(it => it.Age), Um = g.Min(it => it.Age), uu = g.Average(it => it.Age), umx = g.Max(it => it.Age) }).ToDictionary(it => it.Key);
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select [p0].[Age] as [Key], Count([p0].[Age]) as [Count], Sum([p0].[Age]) as [Uv], Min([p0].[Age]) as [Um], Avg([p0].[Age]) as [uu], Max([p0].[Age]) as [umx] from [Person] as [p0] group by [p0].[Name], [p0].[Age]", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestBool(bool bv)
        {
            var personRepository = new PersonRepository();

            var r1 = personRepository.Where(x => bv).ToList();

            var r1MiddleResult = personRepository.GetParsingResult();
            var t = bv ? 1 : 0;
            Assert.Equal($"select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(1 = {t})", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }

        [Fact]
        public void TestBool2()
        {
            var personRepository = new PersonRepository();

            var r1 = personRepository.Where(x => true).ToList();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal($"select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0] where(1 = 1)", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }


        [Fact]
        public async Task TestFirstOrDefault()
        {
            var personRepository = new PersonRepository();
            //var r12 =await personRepository.FirstOrDefaultAsync();
            var r1 = personRepository.FirstOrDefault();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p1].[Name], [p1].[Age], [p1].[HaveChildren] from(select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren], ROW_NUMBER()over(order by [p0].[Age]) as [sbRowNo] from [Person] as [p0]) as [p1] where(([p1].[sbRowNo] > @y0) and([p1].[sbRowNo] <= @y1))", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestFirstOrDefault2()
        {
            var personRepository = new PersonRepository();

            var r1 = personRepository.First();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p1].[Name], [p1].[Age], [p1].[HaveChildren] from(select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren], ROW_NUMBER()over(order by [p0].[Age]) as [sbRowNo] from [Person] as [p0]) as [p1] where(([p1].[sbRowNo] > @y0) and([p1].[sbRowNo] <= @y1))", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestFirstOrDefault3()
        {
            var personRepository = new PersonRepository();

            var r1 = personRepository.Distinct().FirstOrDefault();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p1].[Name], [p1].[Age], [p1].[HaveChildren] from(select distinct [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren], ROW_NUMBER()over(order by [p0].[Age]) as [sbRowNo] from [Person] as [p0]) as [p1] where(([p1].[sbRowNo] > @y0) and([p1].[sbRowNo] <= @y1))", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestMysqlFirstOrDefault()
        {
            var personRepository = new MysqlPersonRepository();

            var r1 = personRepository.FirstOrDefault();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select `p0`.`Name` as `Name`, `p0`.`Age` as `Age`, `p0`.`HaveChildren` as `HaveChildren` from `Person` as `p0` limit @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestOracleFirstOrDefault()
        {
            var personRepository = new OraclePersonRepository(new DatabaseUnit(typeof(UnitOfWork), typeof(OracleConnection), ""));

            var r1 = personRepository.FirstOrDefault();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select \"p1\".\"Name\", \"p1\".\"Age\", \"p1\".\"HaveChildren\" from(select \"p0\".\"NAME\" as \"Name\", \"p0\".\"AGE\" as \"Age\", \"p0\".\"HAVECHILDREN\" as \"HaveChildren\", ROW_NUMBER()over(order by \"p0\".\"AGE\") as \"sbRowNo\" from \"PERSON\" \"p0\") \"p1\" where((\"p1\".\"sbRowNo\" > :y0) and(\"p1\".\"sbRowNo\" <= :y1))", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestMysqlFirstOrDefault2()
        {
            var personRepository = new MysqlPersonRepository();

            var r1 = personRepository.Distinct().FirstOrDefault();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select distinct `p0`.`Name` as `Name`, `p0`.`Age` as `Age`, `p0`.`HaveChildren` as `HaveChildren` from `Person` as `p0` limit @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestDistinct()
        {
            var personRepository = new PersonRepository();

            var r1 = personRepository.Distinct().ToList();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select distinct [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren] from [Person] as [p0]", r1MiddleResult.Sql);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos.Count);
        }

        [Fact]
        public void TestSkipAndTake()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Skip(1).Take(1).ToList();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p1].[Name], [p1].[Age], [p1].[HaveChildren] from(select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren], ROW_NUMBER()over(order by [p0].[Age]) as [sbRowNo] from [Person] as [p0]) as [p1] where(([p1].[sbRowNo] > @y0) and([p1].[sbRowNo] <= @y1))", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void OracleTestSkipAndTake()
        {
            var personRepository = new OraclePersonRepository(new DatabaseUnit(typeof(UnitOfWork), typeof(OracleConnection), ""));
            var r1 = personRepository.Skip(1).Take(1).ToList();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select \"p1\".\"Name\", \"p1\".\"Age\", \"p1\".\"HaveChildren\" from(select \"p0\".\"NAME\" as \"Name\", \"p0\".\"AGE\" as \"Age\", \"p0\".\"HAVECHILDREN\" as \"HaveChildren\", ROW_NUMBER()over(order by \"p0\".\"AGE\") as \"sbRowNo\" from \"PERSON\" \"p0\") \"p1\" where((\"p1\".\"sbRowNo\" > :y0) and(\"p1\".\"sbRowNo\" <= :y1))", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestSkipAndTake2()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Take(1).ToList();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p1].[Name], [p1].[Age], [p1].[HaveChildren] from(select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren], ROW_NUMBER()over(order by [p0].[Age]) as [sbRowNo] from [Person] as [p0]) as [p1] where(([p1].[sbRowNo] > @y0) and([p1].[sbRowNo] <= @y1))", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);

        }
        [Fact]
        public void TestSkipAndTake3()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp").OrderBy(it => it.Age).Select(it => it.HaveChildren).Skip(1).Take(1).ToList();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p1].[HaveChildren] from(select [p0].[HaveChildren], ROW_NUMBER()over(order by [p0].[Age] asc) as [sbRowNo] from [Person] as [p0] where([p0].[Name] = @y0)) as [p1] where(([p1].[sbRowNo] > @y1) and([p1].[sbRowNo] <= @y2))", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos["y2"].Value);
        }

        [Fact]
        public void TestSkipAndTake4()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderBy(it => it.Age).Take(1).ToList();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p1].[Name], [p1].[Age], [p1].[HaveChildren] from(select [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren], ROW_NUMBER()over(order by [p0].[Age] asc) as [sbRowNo] from [Person] as [p0]) as [p1] where(([p1].[sbRowNo] > @y0) and([p1].[sbRowNo] <= @y1))", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);

        }

        [Fact]
        public void TestSkipAndTake5()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp").OrderBy(it => it.Age).Select(it => it.HaveChildren).Skip(1).Take(1).Distinct().ToList();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select distinct [p1].[HaveChildren] from(select [p0].[HaveChildren], ROW_NUMBER()over(order by [p0].[Age] asc) as [sbRowNo] from [Person] as [p0] where([p0].[Name] = @y0)) as [p1] where(([p1].[sbRowNo] > @y1) and([p1].[sbRowNo] <= @y2))", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos["y2"].Value);
        }

        [Fact]
        public void TestSkipAndTake6()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp").OrderBy(it => it.Age).Distinct().Select(it => it.HaveChildren).Skip(1).Take(1).ToList();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select [p1].[HaveChildren] from(select distinct [p0].[HaveChildren], ROW_NUMBER()over(order by [p0].[Age] asc) as [sbRowNo] from [Person] as [p0] where([p0].[Name] = @y0)) as [p1] where(([p1].[sbRowNo] > @y1) and([p1].[sbRowNo] <= @y2))", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos["y2"].Value);
        }

        [Fact]
        public void TestMysqlSkipAndTake()
        {
            var personRepository = new MysqlPersonRepository();
            var r1 = personRepository.Skip(1).Take(1).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();
            Assert.Equal("select `p0`.`Name` as `Name`, `p0`.`Age` as `Age`, `p0`.`HaveChildren` as `HaveChildren` from `Person` as `p0` limit @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestMysqlSkipAndTake2()
        {
            var personRepository = new MysqlPersonRepository();
            var r1 = personRepository.Take(5).ToList();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select `p0`.`Name` as `Name`, `p0`.`Age` as `Age`, `p0`.`HaveChildren` as `HaveChildren` from `Person` as `p0` limit @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(5, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestMysqlSkipAndTake3()
        {
            var personRepository = new MysqlPersonRepository();
            var r1 = personRepository.Skip(5).ToList();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select `p0`.`Name` as `Name`, `p0`.`Age` as `Age`, `p0`.`HaveChildren` as `HaveChildren` from `Person` as `p0` limit @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(5, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(int.MaxValue, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestMysqlSkipAndTake4()
        {
            var personRepository = new MysqlPersonRepository();
            var r1 = personRepository.GroupBy(it => it.Name).Select(it => new { it.Key, Count = it.Sum(x => x.Age) }).Skip(1).Take(1).ToList();
            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select `p0`.`Name` as `Key`, Sum(`p0`.`Age`) as `Count` from `Person` as `p0` group by `p0`.`Name` limit @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestMysqlSkipAndTake5()
        {
            var personRepository = new MysqlPersonRepository();
            var r1 = personRepository.GroupBy(it => it.Name).Select(it => new { it.Key, Count = it.Sum(x => x.Age) }).Distinct().Skip(1).Take(1).ToList();

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select distinct `p0`.`Name` as `Key`, Sum(`p0`.`Age`) as `Count` from `Person` as `p0` group by `p0`.`Name` limit @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);

            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestMysqlSkipAndTake6()
        {
            var personRepository = new MysqlPersonRepository();
            var r1 = personRepository.GroupBy(it => it.Name).Select(it => new { it.Key, Count = it.Sum(x => x.Age) })
                .Distinct().Skip(1).Take(1).ToDictionary(it => it.Key, it => it.Count);

            var r1MiddleResult = personRepository.GetParsingResult();

            Assert.Equal("select distinct `p0`.`Name` as `Key`, Sum(`p0`.`Age`) as `Count` from `Person` as `p0` group by `p0`.`Name` limit @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.Parameters.GetParamInfos.Count);

            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
        }

        [Fact]
        public void TestDelete()
        {
            var personRepository = new PersonRepository();
            var persion = new Person() { Age = 5, HaveChildren = false, Name = "????" };
            personRepository.Delete(persion);
            var r1MiddleResult = personRepository.GetParsingResult();

            //Assert.Equal("delete from [Person] where [Name]=@Name and [Age]=@Age and [HaveChildren]=@HaveChildren", r1MiddleResult.Sql);
        }

        [Fact]
        public void TestUpdate()
        {
            var employeeRepository = new EmployeeRepository();
            var persion = new Employee() { Age = 5, HaveChildren = false, Name = "????" };
            employeeRepository.Update(persion);
            var r1MiddleResult = employeeRepository.GetParsingResult();

            //Assert.Equal("update [Employee] set [Name]=@Name,[Age]=@Age,[HaveChildren]=@HaveChildren where [Id]=@Id", r1MiddleResult.Sql);
        }

        [Fact]
        public void TestUpdate2()
        {
            var hrRepository = new HrRepository();
            var persion = new Hr() { Age = 5, HaveChildren = false, Name = "test", EmployeNo = "666" };
            hrRepository.Update(persion);
            var r1MiddleResult = hrRepository.GetParsingResult();

            //Assert.Equal("update [Hr2] set [Age]=@Age,[HaveChildren]=@HaveChildren where [hrEmployeeNo]=@hrEmployeeNo and [Name]=@Name", r1MiddleResult.Sql);
        }

        [Fact]
        public void TestIgnoreWhenUpdate()
        {
            var hrRepository = new HrRepository();
            var persion = new Hr() { Age = 5, HaveChildren = false, Name = "test", EmployeNo = "666", CreateOn = DateTime.Now };

            hrRepository.Insert(persion);

            var r1MiddleResult = hrRepository.GetParsingResult();

            //Assert.Equal("insert into [Hr2] ([hrEmployeeNo],[Name],[Age],[HaveChildren],[CreateOn]) values (@hrEmployeeNo,@Name,@Age,@HaveChildren,@CreateOn)", r1MiddleResult.Sql);

            hrRepository.Update(persion);

            var r2MiddleResult = hrRepository.GetParsingResult();

            //Assert.Equal("update [Hr2] set [Age]=@Age,[HaveChildren]=@HaveChildren where [hrEmployeeNo]=@hrEmployeeNo and [Name]=@Name", r2MiddleResult.Sql);
        }

        [Fact]
        public void TestInsert()
        {
            var personRepository = new PersonRepository();
            var persion = new Person() { Age = 5, HaveChildren = false, Name = "????" };
            personRepository.Insert(persion);
            var r1MiddleResult = personRepository.GetParsingResult();

            //Assert.Equal("insert into [Person] ([Name],[Age],[HaveChildren]) values (@Name,@Age,@HaveChildren)", r1MiddleResult.Sql);
        }

        [Fact]
        public void TestGet()
        {
            var employeeRepository = new EmployeeRepository();
            employeeRepository.InternalGet(1);
            var r1MiddleResult = employeeRepository.GetParsingResult();

            //Assert.Equal("select [Id],[Name],[Age],[HaveChildren] from [Employee] where [Id]=@y0", r1MiddleResult.Sql);
            //Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos.Count);

            //Assert.Equal("@y0", r1MiddleResult.Parameters[0].ParameterName);
            //Assert.Equal(1, r1MiddleResult.Parameters[0].Value);
        }

        [Fact]
        public void TestGetAll()
        {
            var employeeRepository = new EmployeeRepository();
            employeeRepository.InternalGetAll();
            var r1MiddleResult = employeeRepository.GetParsingResult();

            //Assert.Equal("select [Id],[Name],[Age],[HaveChildren] from [Employee]", r1MiddleResult.Sql);

        }

        [Fact]
        public void TestTrimOrUpperOrLowerProperty()
        {

            var employeeRepository = new EmployeeRepository();
            var newEmployee = new Employee()
            {
                Name = " Hzp "
            };
            var expected =
                "select [p1].[Id], [p1].[Name], [p1].[Age], [p1].[HaveChildren] from(select [p0].[Id] as [Id], [p0].[Name] as [Name], [p0].[Age] as [Age], [p0].[HaveChildren] as [HaveChildren], ROW_NUMBER()over(order by [p0].[Id]) as [sbRowNo] from [Employee] as [p0] where([p0].[Name] = @y0)) as [p1] where(([p1].[sbRowNo] > @y1) and([p1].[sbRowNo] <= @y2))";
            var d = employeeRepository.FirstOrDefault(it => it.Name == newEmployee.Name.Trim());
            var r1MiddleResult = employeeRepository.GetParsingResult();
            Assert.Equal(expected, r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("Hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y2"].Value);
            d = employeeRepository.FirstOrDefault(it => it.Name == newEmployee.Name.TrimStart());
            r1MiddleResult = employeeRepository.GetParsingResult();
            Assert.Equal(expected, r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal("Hzp ", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y2"].Value);
            d = employeeRepository.FirstOrDefault(it => it.Name == newEmployee.Name.TrimEnd());
            r1MiddleResult = employeeRepository.GetParsingResult();
            Assert.Equal(expected, r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(" Hzp", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y2"].Value);
            d = employeeRepository.FirstOrDefault(it => it.Name == newEmployee.Name.ToLower());
            r1MiddleResult = employeeRepository.GetParsingResult();
            Assert.Equal(expected, r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(" hzp ", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y2"].Value);
            d = employeeRepository.FirstOrDefault(it => it.Name == newEmployee.Name.ToUpper());
            r1MiddleResult = employeeRepository.GetParsingResult();
            Assert.Equal(expected, r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.Parameters.GetParamInfos.Count);
            Assert.Equal(" HZP ", r1MiddleResult.Parameters.GetParamInfos["y0"].Value);
            Assert.Equal(0, r1MiddleResult.Parameters.GetParamInfos["y1"].Value);
            Assert.Equal(1, r1MiddleResult.Parameters.GetParamInfos["y2"].Value);
        }
    }

    public class MySqlDbContext : DbContext
    {
        [Obsolete]
        public static readonly LoggerFactory LoggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
        public DbSet<Person> person { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = MyConfiguration.GetConfiguration("mysqlDbConnectionString");
            optionsBuilder.UseLoggerFactory(LoggerFactory);
            optionsBuilder.UseMySQL(connectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().HasNoKey();
            base.OnModelCreating(modelBuilder);
        }
    }


    public class SqlServerDbContext : DbContext
    {
        public static readonly LoggerFactory LoggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
        public DbSet<Person> person { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(LoggerFactory);
            //optionsBuilder.UseSqlServer(@"server=LAPTOP-S1BRVUT1\SQLEXPRESS;User ID=SA;Password=Aa123456;database=master;");
            optionsBuilder.UseSqlServer("server=172.16.189.245;User ID=SA;Password=Aa123456;database=Test;");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().HasNoKey();
            base.OnModelCreating(modelBuilder);
        }
    }
}


