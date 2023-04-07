using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiExample.Model
{
    public class OrderHistory
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        /// <summary>
        /// 会员Id
        /// </summary>
        [Description("会员Id")]
        public int CustomerId { set; get; }
        /// <summary>
        /// 购买商品名称
        /// </summary>
        [Description("购买商品名称")]
        public string ProductName { set; get; }
    
    }
}