using Microsoft.Extensions.DependencyInjection;
using SummerBoot.Cache;
using SummerBoot.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SummerBoot.Test.Common.Dto;
using Xunit;

namespace SummerBoot.Test.CoreTest
{
    public class CoreTest
    {

        [Fact]
        public async Task TestListIsNullOrEmpty()
        {
            var listTest = new ListTest();
            var t1 = listTest.Dtos.IsNullOrEmpty();
            Assert.True(t1);
            var listTest2 = new ListTest()
            {
                Dtos = new List<ListItemTestDto>()
            };
            var t2 = listTest2.Dtos.IsNullOrEmpty();
            Assert.True(t2);
        }


    }
}