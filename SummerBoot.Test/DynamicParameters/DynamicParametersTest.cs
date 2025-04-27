using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SummerBoot.Cache;
using SummerBoot.Test.Model;
using Xunit;

namespace SummerBoot.Test.DynamicParameters;

public class DynamicParametersTest
{
    /// <summary>
    /// 测试值类型作为参数
    /// </summary>
    [Fact]
    public static void TestDynamicParametersWithStruct()
    {
        var p = new Repository.Core.DynamicParameters(new TestReturnStruct()
        {
            Name = "a"
        });
        var r= p.GetParamInfos;
        Assert.Equal(2,r.Count);
        Assert.Equal(null, r["Guid"].Value);
        Assert.Equal("a", r["Name"].Value);
    }
    /// <summary>
    /// 测试class类型作为参数
    /// </summary>
    [Fact]
    public static void TestDynamicParametersWithClass()
    {
        var p = new Repository.Core.DynamicParameters(new TestReturnClass()
        {
            Name = "a"
        });
        var r = p.GetParamInfos;
        Assert.Equal(2, r.Count);
        Assert.Equal(null, r["Guid"].Value);
        Assert.Equal("a", r["Name"].Value);
    }
    /// <summary>
    /// 测试匿名类型作为参数
    /// </summary>
    [Fact]
    public static void TestDynamicParametersWithAnonymousType()
    {
        var guid = Guid.NewGuid();
        var p = new Repository.Core.DynamicParameters(new 
        {
            Guid= guid,
            Name = "a"
        });
        var r = p.GetParamInfos;
        Assert.Equal(2, r.Count);
        Assert.Equal(guid, r["Guid"].Value);
        Assert.Equal("a", r["Name"].Value);
    }
    /// <summary>
    /// 测试字典类型作为参数
    /// </summary>
    [Fact]
    public static void TestDynamicParametersWithDictionary()
    {
        var guid = Guid.NewGuid();
        var p = new Repository.Core.DynamicParameters(new
        Dictionary<string,object>(){
            { "Guid" , guid},
            {"Name" , "a"}
        });
        var r = p.GetParamInfos;
        Assert.Equal(2, r.Count);
        Assert.Equal(guid, r["Guid"].Value);
        Assert.Equal("a", r["Name"].Value);
    }
}