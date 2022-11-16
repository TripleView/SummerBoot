using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Cache;
using SummerBoot.Core;
using SummerBoot.Repository.Core;
using SummerBoot.Test.Feign;
using SummerBoot.Test.Model;
using Xunit;

namespace SummerBoot.Test.Cache
{
    public class DynamicParametersTest
    {
        [Fact]
        public void TestAddClassParameter()
        {
            var param= new DynamicParameters();
            var dog = new DogClass3()
            {
                Name = "何pp",
                Age = 10

            };
            param.AddEntity(dog);
        }

        [Fact]
        public void TestAddValueParameter()
        {
            var param = new DynamicParameters();
            //Assert.Throws<NotSupportedException>(() =>
            //{
            //    param.AddEntity(123);
            //});
            //Assert.Throws<NotSupportedException>(() =>
            //{
            //    param.AddEntity("123");
            //});
            var structDog = new DogStruct3()
            {
                Name = "何pp",
                Age = 10
            };

            if (structDog is DogStruct3 dd)
            {

            }
            //var a1 = structDog.GetPropertyValueByEmit(nameof(DogStruct3.Name));
            var a = ((object)structDog).GetPropertyValueByEmit(nameof(DogStruct3.Name));
            //var b = structDog.GetPropertyValueByEmit(nameof(DogStruct3.Age));
            param.AddEntity(structDog);
        }

    }
}