
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

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
        public DogRepository() : base(DatabaseType.SqlServer)
        {

        }
    }


    public class EmployeeRepository : Repository<Employee>
    {
        public EmployeeRepository() : base(DatabaseType.SqlServer)
        {

        }
    }

    public class HrRepository : Repository<Hr>
    {
        public HrRepository() : base(DatabaseType.SqlServer)
        {

        }
    }

    public class PersonRepository : Repository<Person>, IPersonRepository
    {
        public PersonRepository() : base(DatabaseType.SqlServer)
        {

        }
        public void ExecuteDelete()
        {

        }
    }

    public class MysqlPersonRepository : Repository<Person>, IPersonRepository
    {
        public MysqlPersonRepository() : base(DatabaseType.Mysql)
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
        public OraclePersonRepository() : base(DatabaseType.Oracle)
        {

        }
        public void ExecuteDelete()
        {

        }
    }
    public interface IPersonRepository : IRepository<Person>
    {
        void ExecuteDelete();
    }
    public class UnitTestExpressionTreeVisitor
    {
        [Fact]
        public async Task TestCount()
        {
            var dogRepository = new DogRepository();
            var r1 = dogRepository.Count(it=>it.Name=="sb");
            var r1MiddleResult = dogRepository.GetDbQueryDetail();
            Assert.Equal("SELECT Count(*) FROM [Dog] [p0] WHERE  ([p0].[Name] = @y0 )", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.SqlParameters.Count);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("sb", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestNullable()
        {
            var dogRepository = new DogRepository();
            var r1 = dogRepository.Where(it => it.Active == 1).ToList();
            var r1MiddleResult = dogRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Active] FROM [Dog] [p0] WHERE  ([p0].[Active] = @y0 )", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.SqlParameters.Count);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);
        }
        [Fact]
        public void TestNullable2()
        {
            var dogRepository = new DogRepository();
            var r1 = dogRepository.Where(it => it.Active == 1 && it.Name == "hzp").ToList();
            var r1MiddleResult = dogRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Active] FROM [Dog] [p0] WHERE  ( ([p0].[Active] = @y0 ) AND  ([p0].[Name] = @y1 )  )", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);
            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[1].Value);
        }
        [Fact]
        public void TestSelect()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Select(it => it).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestSelect2()
        {
            var personRepository = new PersonRepository();
            var r2 = personRepository.Select(it => it.Name).ToList();
            var r2MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name] FROM [Person] [p0]", r2MiddleResult.Sql);
            Assert.Empty(r2MiddleResult.SqlParameters);
        }


        [Fact]
        public void TestSelect3()
        {
            var personRepository = new PersonRepository();
            var r3 = personRepository.Select(it => new { it.Name, Address = "福建省" }).ToList();
            var r3MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], '福建省' As [Address] FROM [Person] [p0]", r3MiddleResult.Sql);
            Assert.Empty(r3MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestSelect4()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "Dog" };
            var r4 = personRepository.Select(it => new { it.Name, Address = pet.Name }).ToList();
            var r4MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], 'Dog' As [Address] FROM [Person] [p0]", r4MiddleResult.Sql);
            Assert.Empty(r4MiddleResult.SqlParameters);


        }

        [Fact]
        public void TestWhere2()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.HaveChildren && it.Name == "hzp" && it.Age == 15).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ( ([p0].[HaveChildren] = @y0 ) AND  ([p0].[Name] = @y1 )  ) AND  ([p0].[Age] = @y2 )  )", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.SqlParameters.Count);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);
            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[1].Value);
            Assert.Equal("@y2", r1MiddleResult.SqlParameters[2].ParameterName);
            Assert.Equal(15, r1MiddleResult.SqlParameters[2].Value);
        }

        [Fact]
        public void TestWhere3()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.HaveChildren == false && it.Name == "hzp" && it.Age == 15).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ( ([p0].[HaveChildren] = @y0 ) AND  ([p0].[Name] = @y1 )  ) AND  ([p0].[Age] = @y2 )  )", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.SqlParameters.Count);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(0, r1MiddleResult.SqlParameters[0].Value);
            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[1].Value);
            Assert.Equal("@y2", r1MiddleResult.SqlParameters[2].ParameterName);
            Assert.Equal(15, r1MiddleResult.SqlParameters[2].Value);
        }

        [Fact]
        public void TestWhere4()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp" && it.HaveChildren == false && it.Age == 15).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ( ([p0].[Name] = @y0 ) AND  ([p0].[HaveChildren] = @y1 )  ) AND  ([p0].[Age] = @y2 )  )", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.SqlParameters.Count);
            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(0, r1MiddleResult.SqlParameters[1].Value);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
            Assert.Equal("@y2", r1MiddleResult.SqlParameters[2].ParameterName);
            Assert.Equal(15, r1MiddleResult.SqlParameters[2].Value);
        }

        [Fact]
        public void TestWhere5()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp" && it.HaveChildren && it.Age == 15).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ( ([p0].[Name] = @y0 ) AND  ([p0].[HaveChildren] = @y1 )  ) AND  ([p0].[Age] = @y2 )  )", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.SqlParameters.Count);
            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
            Assert.Equal("@y2", r1MiddleResult.SqlParameters[2].ParameterName);
            Assert.Equal(15, r1MiddleResult.SqlParameters[2].Value);
        }

        [Fact]
        public void TestWhere6()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.HaveChildren && it.HaveChildren).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ([p0].[HaveChildren] = @y0 ) AND  ([p0].[HaveChildren] = @y1 )  )", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);
            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);

        }
        [Fact]
        public void TestWhere7()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => !it.HaveChildren).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (NOT  ( ([p0].[HaveChildren] = @y0 ) ) )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);
        }
        [Fact]
        public void TestWhere8()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => !!it.HaveChildren).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (NOT  ( (NOT  ( ([p0].[HaveChildren] = @y0 ) ) ) ) )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere9()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.Length > 3).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (LEN([p0].[Name]) > @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(3, r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere10()
        {
            var pet = new Pet() { Name = "С��" };
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == pet.Name).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("С��", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere11()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp" && (it.HaveChildren && it.Age == 15)).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ([p0].[Name] = @y0 ) AND  ( ([p0].[HaveChildren] = @y1 ) AND  ([p0].[Age] = @y2 )  )  )", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.SqlParameters.Count);
            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
            Assert.Equal("@y2", r1MiddleResult.SqlParameters[2].ParameterName);
            Assert.Equal(15, r1MiddleResult.SqlParameters[2].Value);
        }

        [Fact]
        public void TestWhere12()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => (it.Name == "hzp" && it.HaveChildren) && it.Age == 15).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ( ([p0].[Name] = @y0 ) AND  ([p0].[HaveChildren] = @y1 )  ) AND  ([p0].[Age] = @y2 )  )", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.SqlParameters.Count);
            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
            Assert.Equal("@y2", r1MiddleResult.SqlParameters[2].ParameterName);
            Assert.Equal(15, r1MiddleResult.SqlParameters[2].Value);
        }

        [Fact]
        public void TestWhere13()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.Contains("hzp")).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] like @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("%hzp%", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere14()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.Contains(pet.Name)).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] like @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("%hzp%", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere15()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.StartsWith("hzp")).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] like @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp%", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere16()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.StartsWith(pet.Name)).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] like @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp%", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere17()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.EndsWith("hzp")).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] like @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("%hzp", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere18()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.EndsWith(pet.Name)).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] like @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("%hzp", r1MiddleResult.SqlParameters[0].Value);
        }
        [Fact]
        public void TestWhere19()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.Trim() == "hzp").ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (TRIM([p0].[Name]) = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere20()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.Trim() == pet.Name).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (TRIM([p0].[Name]) = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
        }
        [Fact]
        public void TestWhere21()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.TrimStart() == "hzp").ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (LTRIM([p0].[Name]) = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere22()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.TrimStart() == pet.Name).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (LTRIM([p0].[Name]) = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
        }
        [Fact]
        public void TestWhere23()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.TrimEnd() == "hzp").ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (RTRIM([p0].[Name]) = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere24()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.TrimEnd() == pet.Name).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (RTRIM([p0].[Name]) = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
        }
        [Fact]
        public void TestWhere25()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.Equals("hzp")).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere26()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.Equals(pet.Name)).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere27()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Address = "ND" };
            var r1 = personRepository.Where(it => it.Name.Equals(pet.Address)).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("ND", r1MiddleResult.SqlParameters[0].Value);
        }


        [Fact]
        public void TestWhere29()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "hzp" };
            var r1 = personRepository.Where(it => it.Name.ToLower() == pet.Name).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (LOWER([p0].[Name]) = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
        }
        [Fact]
        public void TestWhere30()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.ToLower() == "hzp").ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (LOWER([p0].[Name]) = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestWhere31()
        {
            var personRepository = new PersonRepository();
            var pet = new Pet() { Name = "HZP" };
            var r1 = personRepository.Where(it => it.Name.ToUpper() == pet.Name).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (UPPER([p0].[Name]) = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("HZP", r1MiddleResult.SqlParameters[0].Value);
        }
        [Fact]
        public void TestWhere32()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name.ToUpper() == "hzp").ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (UPPER([p0].[Name]) = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
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
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] in @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(nameList, r1MiddleResult.SqlParameters[0].Value);
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
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] in @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(nameList, r1MiddleResult.SqlParameters[0].Value);
        }

        /// <summary>
        /// colloection
        /// </summary>
        [Fact]
        public void TestWhere35()
        {
            var personRepository = new PersonRepository();
            var nameList = Enumerable.Range(0, 1001).Select(it => "hzp" + it).ToList();

            var p0 = nameList.Take(500).ToList();
            var p1 = nameList.Skip(500).Take(500).ToList();
            var p2 = nameList.Skip(1000).Take(500).ToList();

            var r1 = personRepository.Where(it => nameList.Contains(it.Name)).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ( ([p0].[Name] in @y0 ) or  ([p0].[Name] in @y1 )  ) or  ([p0].[Name] in @y2 )  )", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(p0, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(p1, r1MiddleResult.SqlParameters[1].Value);

            Assert.Equal("@y2", r1MiddleResult.SqlParameters[2].ParameterName);
            Assert.Equal(p2, r1MiddleResult.SqlParameters[2].Value);
        }

        /// <summary>
        /// colloection
        /// </summary>
        [Fact]
        public void TestWhere36()
        {
            var personRepository = new PersonRepository();
            var nameList = Enumerable.Range(0, 1001).Select(it => "hzp" + it).ToList();

            var p0 = nameList.Take(500).ToList();
            var p1 = nameList.Skip(500).Take(500).ToList();
            var p2 = nameList.Skip(1000).Take(500).ToList();

            var r1 = personRepository.Where(it => nameList.Contains(it.Name)).ToArray();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ( ([p0].[Name] in @y0 ) or  ([p0].[Name] in @y1 )  ) or  ([p0].[Name] in @y2 )  )", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(p0, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(p1, r1MiddleResult.SqlParameters[1].Value);

            Assert.Equal("@y2", r1MiddleResult.SqlParameters[2].ParameterName);
            Assert.Equal(p2, r1MiddleResult.SqlParameters[2].Value);
        }

        [Fact]
        public void TestWhere37()
        {
            var personRepository = new PersonRepository();
            var nameList = Enumerable.Range(0, 1001).Select(it => "hzp" + it).ToList();

            var p0 = nameList.Take(500).ToList();
            var p1 = nameList.Skip(500).Take(500).ToList();
            var p2 = nameList.Skip(1000).Take(500).ToList();

            var r1 = personRepository.Where(it => nameList.Contains(it.Name) && it.HaveChildren && it.Age == 15).ToArray();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ( ( ( ([p0].[Name] in @y0 ) or  ([p0].[Name] in @y1 )  ) or  ([p0].[Name] in @y2 )  ) AND  ([p0].[HaveChildren] = @y3 )  ) AND  ([p0].[Age] = @y4 )  )", r1MiddleResult.Sql);
            Assert.Equal(5, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(p0, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(p1, r1MiddleResult.SqlParameters[1].Value);

            Assert.Equal("@y2", r1MiddleResult.SqlParameters[2].ParameterName);
            Assert.Equal(p2, r1MiddleResult.SqlParameters[2].Value);

            Assert.Equal("@y3", r1MiddleResult.SqlParameters[3].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[3].Value);
            Assert.Equal("@y4", r1MiddleResult.SqlParameters[4].ParameterName);
            Assert.Equal(15, r1MiddleResult.SqlParameters[4].Value);
        }


        [Fact]
        public void TestCombineSelectAndWhere()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp").Select(it => it).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ([p0].[Name] = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestCombineSelectAndWhere2()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp").Select(it => new { it.Age, Address = "����" }).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();


            Assert.Equal("SELECT [p0].[Age], '����' As [Address] FROM [Person] [p0] WHERE  ([p0].[Name] = @y0 )", r1MiddleResult.Sql);
            Assert.Single(r1MiddleResult.SqlParameters);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestCombineSelectAndWhere3()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp" && it.HaveChildren).Select(it => new { it.Age, it.HaveChildren }).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ([p0].[Name] = @y0 ) AND  ([p0].[HaveChildren] = @y1 )  )", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestCombineSelectAndWhere4()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => true && it.Name == "hzp" && it.HaveChildren).Select(it => new { it.Age, it.HaveChildren }).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ( (1=1 ) AND  ([p0].[Name] = @y0 )  ) AND  ([p0].[HaveChildren] = @y1 )  )", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestCombineSelectAndWhere5()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp" && true && it.HaveChildren).Select(it => new { it.Age, it.HaveChildren }).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  ( ( ([p0].[Name] = @y0 ) AND  (1=1 )  ) AND  ([p0].[HaveChildren] = @y1 )  )", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestOrderBy()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderBy(it => it.Age).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] ORDER BY [p0].[Age]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestOrderBy2()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderByDescending(it => it.Age).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] ORDER BY [p0].[Age] DESC", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }
        [Fact]
        public void TestOrderBy3()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderBy(it => it.Age).ThenBy(it => it.Name).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] ORDER BY [p0].[Age],[p0].[Name]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestOrderBy4()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderBy(it => it.Age).ThenByDescending(it => it.Name).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] ORDER BY [p0].[Age],[p0].[Name] DESC", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestOrderBy5()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderByDescending(it => it.Age).ThenByDescending(it => it.Name).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] ORDER BY [p0].[Age] DESC,[p0].[Name] DESC", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestOrderBy6()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderByDescending(it => it.Age).ThenBy(it => it.Name).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] ORDER BY [p0].[Age] DESC,[p0].[Name]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }


        [Fact]
        public void TestGroupBy()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.GroupBy(it => it.Name).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] GROUP BY [p0].[Name]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestGroupBy2()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.GroupBy(it => new { it.Name, it.Age }).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] GROUP BY [p0].[Name],[p0].[Age]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }
        [Fact]
        public void TestGroupBy3()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.GroupBy(it => it.Name).Select(g => new { Key = g.Key, Count = g.Count(), Uv = g.Sum(it => it.Age) }).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name] As [Key], COUNT(*) As [Count], SUM([p0].[Age]) As [Uv] FROM [Person] [p0] GROUP BY [p0].[Name]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestGroupBy4()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.GroupBy(it => it.Name).Select(g => new { Key = g.Key, Count = g.Count(), Uv = g.Sum(it => it.Age) }).ToDictionary(it => it.Key);
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name] As [Key], COUNT(*) As [Count], SUM([p0].[Age]) As [Uv] FROM [Person] [p0] GROUP BY [p0].[Name]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestGroupBy5()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.GroupBy(it => new { it.Name, it.Age }).Select(g => new { Key = g.Key, Count = g.Count(), Uv = g.Sum(it => it.Age) }).ToDictionary(it => it.Key);
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name], [p0].[Age], COUNT(*) As [Count], SUM([p0].[Age]) As [Uv] FROM [Person] [p0] GROUP BY [p0].[Name],[p0].[Age]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestGroupBy6()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.GroupBy(it => new { it.Name, it.Age }).Select(g => new { Key = g.Key, Count = g.Count(), Uv = g.Sum(it => it.Age), Um = g.Min(it => it.Age), uu = g.Average(it => it.Age), umx = g.Max(it => it.Age) }).ToDictionary(it => it.Key);
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name], [p0].[Age], COUNT(*) As [Count], SUM([p0].[Age]) As [Uv], MIN([p0].[Age]) As [Um], AVG([p0].[Age]) As [uu], MAX([p0].[Age]) As [umx] FROM [Person] [p0] GROUP BY [p0].[Name],[p0].[Age]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestTrue()
        {
            var personRepository = new PersonRepository();

            var r1 = personRepository.Where(x => true).ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] WHERE  (1=1 )", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestFirstOrDefault()
        {
            var personRepository = new PersonRepository();

            var r1 = personRepository.FirstOrDefault();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT TOP(1) [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestFirstOrDefault2()
        {
            var personRepository = new PersonRepository();

            var r1 = personRepository.First();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT TOP(1) [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestFirstOrDefault3()
        {
            var personRepository = new PersonRepository();

            var r1 = personRepository.Distinct().FirstOrDefault();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT TOP(1) [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM (SELECT DISTINCT [p1].[Name], [p1].[Age], [p1].[HaveChildren] FROM [Person] [p1]) [p0]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestMysqlFirstOrDefault()
        {
            var personRepository = new MysqlPersonRepository();

            var r1 = personRepository.FirstOrDefault();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT `p0`.`Name`, `p0`.`Age`, `p0`.`HaveChildren` FROM `Person` `p0` LIMIT @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(0, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestOracleFirstOrDefault()
        {
            var personRepository = new OraclePersonRepository();

            var r1 = personRepository.FirstOrDefault();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT \"p1\".\"NAME\", \"p1\".\"AGE\", \"p1\".\"HAVECHILDREN\" FROM (SELECT \"p0\".\"NAME\", \"p0\".\"AGE\", \"p0\".\"HAVECHILDREN\",ROW_NUMBER() OVER( ORDER BY  null ) AS pageNo FROM \"PERSON\" \"p0\") \"p1\" WHERE \"p1\".pageNo>:y0 AND \"p1\".pageNo<=:y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal(":y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(0, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal(":y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestMysqlFirstOrDefault2()
        {
            var personRepository = new MysqlPersonRepository();

            var r1 = personRepository.Distinct().FirstOrDefault();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT `p0`.`Name`, `p0`.`Age`, `p0`.`HaveChildren` FROM (SELECT DISTINCT `p1`.`Name`, `p1`.`Age`, `p1`.`HaveChildren` FROM `Person` `p1`) `p0` LIMIT @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(0, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestDistinct()
        {
            var personRepository = new PersonRepository();

            var r1 = personRepository.Distinct().ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT DISTINCT [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);
        }

        [Fact]
        public void TestSkipAndTake()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Skip(1).Take(1).ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p1].[Name], [p1].[Age], [p1].[HaveChildren] FROM (SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren],ROW_NUMBER() OVER( ORDER BY (SELECT 1)) AS [ROW] FROM [Person] [p0]) [p1] WHERE [p1].[ROW]>@y0 AND [p1].[ROW]<=@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(2, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void OracleTestSkipAndTake()
        {
            var personRepository = new OraclePersonRepository();
            var r1 = personRepository.Skip(1).Take(1).ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT \"p1\".\"NAME\", \"p1\".\"AGE\", \"p1\".\"HAVECHILDREN\" FROM (SELECT \"p0\".\"NAME\", \"p0\".\"AGE\", \"p0\".\"HAVECHILDREN\",ROW_NUMBER() OVER( ORDER BY  null ) AS pageNo FROM \"PERSON\" \"p0\") \"p1\" WHERE \"p1\".pageNo>:y0 AND \"p1\".pageNo<=:y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal(":y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal(":y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(2, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestSkipAndTake2()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Take(1).ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT TOP(1) [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);

        }
        [Fact]
        public void TestSkipAndTake3()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp").OrderBy(it => it.Age).Select(it => it.HaveChildren).Skip(1).Take(1).ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p1].[HaveChildren] FROM (SELECT [p0].[Age], [p0].[HaveChildren],ROW_NUMBER() OVER( ORDER BY [p0].[Age]) AS [ROW] FROM [Person] [p0] WHERE  ([p0].[Name] = @y0 )) [p1] WHERE [p1].[ROW]>@y1 AND [p1].[ROW]<=@y2", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);

            Assert.Equal("@y2", r1MiddleResult.SqlParameters[2].ParameterName);
            Assert.Equal(2, r1MiddleResult.SqlParameters[2].Value);
        }

        [Fact]
        public void TestSkipAndTake4()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.OrderBy(it => it.Age).Take(1).ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT TOP(1) [p0].[Name], [p0].[Age], [p0].[HaveChildren] FROM [Person] [p0] ORDER BY [p0].[Age]", r1MiddleResult.Sql);
            Assert.Empty(r1MiddleResult.SqlParameters);

        }

        [Fact]
        public void TestSkipAndTake5()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp").OrderBy(it => it.Age).Select(it => it.HaveChildren).Skip(1).Take(1).Distinct().ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT DISTINCT [p1].[HaveChildren] FROM (SELECT [p0].[Age], [p0].[HaveChildren],ROW_NUMBER() OVER( ORDER BY [p0].[Age]) AS [ROW] FROM [Person] [p0] WHERE  ([p0].[Name] = @y0 )) [p1] WHERE [p1].[ROW]>@y1 AND [p1].[ROW]<=@y2", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);

            Assert.Equal("@y2", r1MiddleResult.SqlParameters[2].ParameterName);
            Assert.Equal(2, r1MiddleResult.SqlParameters[2].Value);
        }

        [Fact]
        public void TestSkipAndTake6()
        {
            var personRepository = new PersonRepository();
            var r1 = personRepository.Where(it => it.Name == "hzp").OrderBy(it => it.Age).Distinct().Select(it => it.HaveChildren).Skip(1).Take(1).ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT [p1].[HaveChildren] FROM (SELECT [p0].[Name], [p0].[Age], [p0].[HaveChildren],ROW_NUMBER() OVER( ORDER BY (SELECT 1)) AS [ROW] FROM (SELECT DISTINCT [p2].[Name], [p2].[Age], [p2].[HaveChildren] FROM [Person] [p2] WHERE  ([p2].[Name] = @y0 )) [p0]) [p1] WHERE [p1].[ROW]>@y1 AND [p1].[ROW]<=@y2", r1MiddleResult.Sql);
            Assert.Equal(3, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal("hzp", r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);

            Assert.Equal("@y2", r1MiddleResult.SqlParameters[2].ParameterName);
            Assert.Equal(2, r1MiddleResult.SqlParameters[2].Value);
        }

        [Fact]
        public void TestMysqlSkipAndTake()
        {
            var personRepository = new MysqlPersonRepository();
            var r1 = personRepository.Skip(1).Take(1).ToList();
            var r1MiddleResult = personRepository.GetDbQueryDetail();
            Assert.Equal("SELECT `p0`.`Name`, `p0`.`Age`, `p0`.`HaveChildren` FROM `Person` `p0` LIMIT @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestMysqlSkipAndTake2()
        {
            var personRepository = new MysqlPersonRepository();
            var r1 = personRepository.Take(5).ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT `p0`.`Name`, `p0`.`Age`, `p0`.`HaveChildren` FROM `Person` `p0` LIMIT @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(0, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(5, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestMysqlSkipAndTake3()
        {
            var personRepository = new MysqlPersonRepository();
            var r1 = personRepository.Skip(5).ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT `p0`.`Name`, `p0`.`Age`, `p0`.`HaveChildren` FROM `Person` `p0` LIMIT @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(5, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(int.MaxValue, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestMysqlSkipAndTake4()
        {
            var personRepository = new MysqlPersonRepository();
            var r1 = personRepository.GroupBy(it => it.Name).Select(it => new { it.Key, Count = it.Sum(x => x.Age) }).Skip(1).Take(1).ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT `p0`.`Name` As `Key`, SUM(`p0`.`Age`) As `Count` FROM `Person` `p0` GROUP BY `p0`.`Name` LIMIT @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestMysqlSkipAndTake5()
        {
            var personRepository = new MysqlPersonRepository();
            var r1 = personRepository.GroupBy(it => it.Name).Select(it => new { it.Key, Count = it.Sum(x => x.Age) }).Distinct().Skip(1).Take(1).ToList();

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT DISTINCT `p0`.`Name` As `Key`, SUM(`p0`.`Age`) As `Count` FROM `Person` `p0` GROUP BY `p0`.`Name` LIMIT @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestMysqlSkipAndTake6()
        {
            var personRepository = new MysqlPersonRepository();
            var r1 = personRepository.GroupBy(it => it.Name).Select(it => new { it.Key, Count = it.Sum(x => x.Age) })
                .Distinct().Skip(1).Take(1).ToDictionary(it => it.Key, it => it.Count);

            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("SELECT DISTINCT `p0`.`Name` As `Key`, SUM(`p0`.`Age`) As `Count` FROM `Person` `p0` GROUP BY `p0`.`Name` LIMIT @y0,@y1", r1MiddleResult.Sql);
            Assert.Equal(2, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);

            Assert.Equal("@y1", r1MiddleResult.SqlParameters[1].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[1].Value);
        }

        [Fact]
        public void TestDelete()
        {
            var personRepository = new PersonRepository();
            var persion = new Person() { Age = 5, HaveChildren = false, Name = "����" };
            personRepository.InternalDelete(persion);
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("delete from [Person] where [Name]=@Name and [Age]=@Age and [HaveChildren]=@HaveChildren", r1MiddleResult.Sql);
        }

        [Fact]
        public void TestUpdate()
        {
            var employeeRepository = new EmployeeRepository();
            var persion = new Employee() { Age = 5, HaveChildren = false, Name = "����" };
            employeeRepository.InternalUpdate(persion);
            var r1MiddleResult = employeeRepository.GetDbQueryDetail();

            Assert.Equal("update [Employee] set [Name]=@Name,[Age]=@Age,[HaveChildren]=@HaveChildren where [Id]=@Id", r1MiddleResult.Sql);
        }

        [Fact]
        public void TestUpdate2()
        {
            var hrRepository = new HrRepository();
            var persion = new Hr() { Age = 5, HaveChildren = false, Name = "����", EmployeNo = "666" };
            hrRepository.InternalUpdate(persion);
            var r1MiddleResult = hrRepository.GetDbQueryDetail();

            Assert.Equal("update [Hr2] set [Age]=@Age,[HaveChildren]=@HaveChildren where [hrEmployeeNo]=@EmployeNo and [Name]=@Name", r1MiddleResult.Sql);
        }

        [Fact]
        public void TestIgnoreWhenUpdate()
        {
            var hrRepository = new HrRepository();
            var persion = new Hr() { Age = 5, HaveChildren = false, Name = "����", EmployeNo = "666", CreateOn = DateTime.Now };

            hrRepository.InternalInsert(persion);

            var r1MiddleResult = hrRepository.GetDbQueryDetail();

            Assert.Equal("insert into [Hr2] ([hrEmployeeNo],[Name],[Age],[HaveChildren],[CreateOn]) values (@hrEmployeeNo,@Name,@Age,@HaveChildren,@CreateOn)", r1MiddleResult.Sql);

            hrRepository.InternalUpdate(persion);

            var r2MiddleResult = hrRepository.GetDbQueryDetail();

            Assert.Equal("update [Hr2] set [Age]=@Age,[HaveChildren]=@HaveChildren where [hrEmployeeNo]=@EmployeNo and [Name]=@Name", r2MiddleResult.Sql);
        }

        [Fact]
        public void TestInsert()
        {
            var personRepository = new PersonRepository();
            var persion = new Person() { Age = 5, HaveChildren = false, Name = "����" };
            personRepository.InternalInsert(persion);
            var r1MiddleResult = personRepository.GetDbQueryDetail();

            Assert.Equal("insert into [Person] ([Name],[Age],[HaveChildren]) values (@Name,@Age,@HaveChildren)", r1MiddleResult.Sql);
        }

        [Fact]
        public void TestGet()
        {
            var employeeRepository = new EmployeeRepository();
            employeeRepository.InternalGet(1);
            var r1MiddleResult = employeeRepository.GetDbQueryDetail();

            Assert.Equal("select [Id],[Name],[Age],[HaveChildren] from [Employee] where [Id]=@y0", r1MiddleResult.Sql);
            Assert.Equal(1, r1MiddleResult.SqlParameters.Count);

            Assert.Equal("@y0", r1MiddleResult.SqlParameters[0].ParameterName);
            Assert.Equal(1, r1MiddleResult.SqlParameters[0].Value);
        }

        [Fact]
        public void TestGetAll()
        {
            var employeeRepository = new EmployeeRepository();
            employeeRepository.InternalGetAll();
            var r1MiddleResult = employeeRepository.GetDbQueryDetail();

            Assert.Equal("select [Id],[Name],[Age],[HaveChildren] from [Employee]", r1MiddleResult.Sql);

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


