using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Oracle.Models
{
    /// <summary>
    /// 测试TypeHandler
    /// </summary>
    [Table("TESTTYPEHANDLERTABLE")]
    public class TestTypeHandlerTable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("ID")]
        public int Id { set; get; }
        [Column("BOOLCOLUMN")]
        public bool BoolColumn { get; set; }
        [Column("NULLABLEBOOLCOLUMN")]
        public bool? NullableBoolColumn { get; set; }
        [Column("NAME")]
        public string Name { get; set; }
        [Column("GUIDCOLUMN")]
        public Guid GuidColumn { get; set; }
        [Column("NULLABLEGUIDCOLUMN")]
        public Guid? NullableGuidColumn { get; set; }
    }
}