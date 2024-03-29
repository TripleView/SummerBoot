﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository.Attributes;

namespace SummerBoot.Test.Oracle.Models
{
    [Table("NOTNULLABLETABLE")]
    public class NotNullableTable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Description("这是一个测试")]
        [Column("INT2")]
        public int Int2 { get; set; }
        [Column("LONG2")]
        public long Long2 { get; set; }
        [Column("FLOAT2")]
        public float Float2 { get; set; }
        [Column("DOUBLE2")]
        public double Double2 { get; set; }
        [Column("DECIMAL2")]
        public decimal Decimal2 { get; set; }
        [Column("DECIMAL3")]
        [DecimalPrecision(20, 4)]
        public decimal Decimal3 { get; set; }
        [Column("GUID2")]
        public Guid Guid2 { get; set; }
        [Column("SHORT2")]
        public short Short2 { get; set; }
        [Column("DATETIME2")]
        public DateTime DateTime2 { get; set; }
        [Column("BOOL2")]
        public bool Bool2 { get; set; }
        [Column("TIMESPAN2")]
        public TimeSpan TimeSpan2 { get; set; }
        [Column("BYTE2")]
        public byte Byte2 { get; set; }

        [Required]
        [StringLength(100)]
        [Column("STRING2")]
        public string String2 { get; set; }
        [Required]
        [Column("STRING3")]
        public string String3 { get; set; }
    }

}