using System;
using System.ComponentModel.DataAnnotations;

namespace SummerBoot.Test.SqlServer.Models
{
    public class GuidModel
    {
        [Key]
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

        public string Name { get; set; }

        public string Address { get; set; }

    }
}