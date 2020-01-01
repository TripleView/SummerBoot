using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Example.Models
{
    [Table("address")]
    public class Address
    {
        [Key]
        public int Id { set; get; }

        /// <summary>
        /// 省
        /// </summary>
        public string Province { set; get; }

        /// <summary>
        /// 市
        /// </summary>
        public string City { set; get; }

        /// <summary>
        /// 县城
        /// </summary>
        public string County { set; get; }
    }
}