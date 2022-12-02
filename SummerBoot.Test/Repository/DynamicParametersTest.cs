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

            var actualParameters = param.GetParamInfos;
            Assert.Equal(3, actualParameters.Count);
            Assert.Equal("何pp", actualParameters["Name"].Value);
            Assert.Equal(10, actualParameters["Age"].Value);
            Assert.Equal(null, actualParameters["Kind"].Value);
        }

        [Fact]
        public void TestAddValueParameter()
        {
            var param = new DynamicParameters();
            Assert.Throws<NotSupportedException>(() =>
            {
                param.AddEntity(123);
            });
            Assert.Throws<NotSupportedException>(() =>
            {
                param.AddEntity("123");
            });
            var structDog = new DogStruct3()
            {
                Name = "何pp",
                Age = 10
            };
            param.AddEntity(structDog);

            var para = param.GetParamInfos;
            Assert.Equal(3,para.Count);
            Assert.Equal("何pp", para[nameof(DogStruct3.Name)].Value);
            Assert.Equal(10, para[nameof(DogStruct3.Age)].Value);
            Assert.Equal(null, para[nameof(DogStruct3.Kind)].Value);
        }

    }
}