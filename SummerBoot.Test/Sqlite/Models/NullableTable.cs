using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository.Attributes;

namespace SummerBoot.Test.Sqlite.Models
{
    [Description("NullableTable")]
    public class NullableTable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        [Description("Int2")]
        public int? Int2 { get; set; }
        [Description("Long2")]
        public long? Long2 { get; set; }

        public float? Float2 { get; set; }
        public double? Double2 { get; set; }

        public decimal? Decimal2 { get; set; }

        [DecimalPrecision(20,4)]
        public decimal? Decimal3 { get; set; }
        public Guid? Guid2 { get; set; }

        public short? Short2 { get; set; }

        public DateTime? DateTime2 { get; set; }

        public bool? Bool2 { get; set; }

        public TimeSpan? TimeSpan2 { get; set; }

        public byte? Byte2 { get; set; }


        [StringLength(100)]
        public string String2 { get; set; }
        public string String3 { get; set; }
    }

}