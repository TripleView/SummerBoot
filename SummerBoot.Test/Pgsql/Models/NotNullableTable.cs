using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository.Attributes;

namespace SummerBoot.Test.Pgsql.Models
{
    [Table("notnullabletable")]
    public class NotNullableTable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Description("这是一个测试")]
        [Column("int2")]
        public int Int2 { get; set; }
        [Column("long2")]
        public long Long2 { get; set; }
        [Column("float2")]
        public float Float2 { get; set; }
        [Column("double2")]
        public double Double2 { get; set; }
        [Column("decimal2")]
        public decimal Decimal2 { get; set; }
        [Column("decimal3")]
        [DecimalPrecision(20, 4)]
        public decimal Decimal3 { get; set; }
        [Column("guid2")]
        public Guid Guid2 { get; set; }
        [Column("short2")]
        public short Short2 { get; set; }
        [Column("datetime2")]
        public DateTime DateTime2 { get; set; }
        [Column("bool2")]
        public bool Bool2 { get; set; }
        [Column("timespan2")]
        public TimeSpan TimeSpan2 { get; set; }
        [Column("byte2")]
        public byte Byte2 { get; set; }

        [Required]
        [StringLength(100)]
        [Column("string2")]
        public string String2 { get; set; }
        [Required]
        [Column("string3")]
        public string String3 { get; set; }
    }

}