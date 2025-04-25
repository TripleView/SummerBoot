using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Model;

public struct TestReturnStruct
{
    public TestReturnStruct()
    {

    }
    public TestReturnStruct(Guid? guid, string name)
    {
        Guid = guid;
        Name = name;
    }
    public Guid? Guid { get; set; }
    public string Name { get; set; }

}

public struct TestReturnStruct2
{
    public TestReturnStruct2()
    {

    }
    public TestReturnStruct2(string name)
    {

        Name = name;
    }

    public string Name { get; set; }

}