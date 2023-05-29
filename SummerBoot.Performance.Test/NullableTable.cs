using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository.Attributes;

namespace SummerBoot.Performance.Test
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
        //[Column(name: "Guid2", TypeName = "varbinary(200)")]
        public Guid? Guid2 { get; set; }

        public short? Short2 { get; set; }

        public DateTime? DateTime2 { get; set; }

        public bool? Bool2 { get; set; }

        public TimeSpan? TimeSpan2 { get; set; }

        public byte? Byte2 { get; set; }


        [StringLength(100)]
        public string String2 { get; set; }
        public string String3 { get; set; }

        public Enum2? Enum2 { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column("TestInt3")]
        [Description("Int2")]
        public int? Int3 { get; set; }

        protected bool Equals(NullableTable other)
        {
            return Int2 == other.Int2 && Long2 == other.Long2 && Nullable.Equals(Float2, other.Float2) && Nullable.Equals(Double2, other.Double2) && Decimal2 == other.Decimal2 && Decimal3 == other.Decimal3 && Nullable.Equals(Guid2, other.Guid2) && Short2 == other.Short2 && Nullable.Equals(DateTime2, other.DateTime2) && Bool2 == other.Bool2 && Nullable.Equals(TimeSpan2, other.TimeSpan2) && Byte2 == other.Byte2 && String2 == other.String2 && String3 == other.String3 && Enum2 == other.Enum2 && Int3 == other.Int3;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NullableTable)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Int2);
            hashCode.Add(Long2);
            hashCode.Add(Float2);
            hashCode.Add(Double2);
            hashCode.Add(Decimal2);
            hashCode.Add(Decimal3);
            hashCode.Add(Guid2);
            hashCode.Add(Short2);
            hashCode.Add(DateTime2);
            hashCode.Add(Bool2);
            hashCode.Add(TimeSpan2);
            hashCode.Add(Byte2);
            hashCode.Add(String2);
            hashCode.Add(String3);
            hashCode.Add(Enum2);
            hashCode.Add(Int3);
            return hashCode.ToHashCode();
        }
    }


    [FreeSql.DataAnnotations.Table(Name = "NullableTableForFreeSql")]
    public class NullableTableForFreeSql
    {
        [FreeSql.DataAnnotations.Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        [Description("Int2")]
        public int? Int2 { get; set; }
        [Description("Long2")]
        public long? Long2 { get; set; }

        public float? Float2 { get; set; }
        public double? Double2 { get; set; }

        public decimal? Decimal2 { get; set; }

        [DecimalPrecision(20, 4)]
        public decimal? Decimal3 { get; set; }
        //[Column(name: "Guid2", TypeName = "varbinary(200)")]
        public Guid? Guid2 { get; set; }

        public short? Short2 { get; set; }

        public DateTime? DateTime2 { get; set; }

        public bool? Bool2 { get; set; }

        public TimeSpan? TimeSpan2 { get; set; }

        public byte? Byte2 { get; set; }



        public string String2 { get; set; }
        public string String3 { get; set; }

        public Enum2? Enum2 { get; set; }

        [FreeSql.DataAnnotations.Column(Name = "TestInt3")]
        public int? Int3 { get; set; }

        protected bool Equals(NullableTable other)
        {
            return Int2 == other.Int2 && Long2 == other.Long2 && Nullable.Equals(Float2, other.Float2) && Nullable.Equals(Double2, other.Double2) && Decimal2 == other.Decimal2 && Decimal3 == other.Decimal3 && Nullable.Equals(Guid2, other.Guid2) && Short2 == other.Short2 && Nullable.Equals(DateTime2, other.DateTime2) && Bool2 == other.Bool2 && Nullable.Equals(TimeSpan2, other.TimeSpan2) && Byte2 == other.Byte2 && String2 == other.String2 && String3 == other.String3 && Enum2 == other.Enum2 && Int3 == other.Int3;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NullableTable)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Int2);
            hashCode.Add(Long2);
            hashCode.Add(Float2);
            hashCode.Add(Double2);
            hashCode.Add(Decimal2);
            hashCode.Add(Decimal3);
            hashCode.Add(Guid2);
            hashCode.Add(Short2);
            hashCode.Add(DateTime2);
            hashCode.Add(Bool2);
            hashCode.Add(TimeSpan2);
            hashCode.Add(Byte2);
            hashCode.Add(String2);
            hashCode.Add(String3);
            hashCode.Add(Enum2);
            hashCode.Add(Int3);
            return hashCode.ToHashCode();
        }
    }

    public enum Enum2
    {
        x,
        y
    }

}