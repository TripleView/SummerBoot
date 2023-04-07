using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Pgsql.Models
{
    /// <summary>
    /// 测试TypeHandler
    /// </summary>
    [Table("testtypehandlertable")]
    public class TestTypeHandlerTable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id")]
        public int Id { set; get; }
        [Column("boolcolumn")]
        public bool BoolColumn { get; set; }
        [Column("nullableboolcolumn")]
        public bool? NullableBoolColumn { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("guidcolumn")]
        public Guid GuidColumn { get; set; }
        [Column("nullableguidcolumn")]
        public Guid? NullableGuidColumn { get; set; }
    }
}