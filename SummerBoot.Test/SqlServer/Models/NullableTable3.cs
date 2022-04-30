using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository.Attributes;

namespace SummerBoot.Test.SqlServer.Models
{
    [Table("NullableTable")]
    [Description("NullableTable test add column")]
    public class NullableTable3 : NullableTable
    {
        [Description("test add column")]
        public int? int3 { get; set; }
    }

}