using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Model;

public class TestReturnClass
{
    public TestReturnClass()
    {

    }
    public TestReturnClass(Guid? guid, string name)
    {
        Guid = guid;
        Name = name;
    }
    public Guid? Guid { get; set; }
    public string Name { get; set; }

}