using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Pgsql.Models
{
    [Table("guidmodel")]
    public class GuidModel
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        protected bool Equals(GuidModel other)
        {
            return Id.Equals(other.Id) && Name == other.Name && Address == other.Address;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GuidModel)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Address);
        }

        [Column("name")]
        public string Name { get; set; }

        [Column("address")]
        public string Address { get; set; }
    }
}