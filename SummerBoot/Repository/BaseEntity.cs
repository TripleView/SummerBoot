using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository.Attributes;

namespace SummerBoot.Repository
{
    public abstract class BaseEntity : IBaseEntity
    {
        public BaseEntity()
        {
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Description("Id")]
        public int Id { set; get; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Description("最后更新时间")]
        public DateTime? LastUpdateOn { get; set; }
        /// <summary>
        /// 最后更新人
        /// </summary>
        [Description("最后更新人")]
        public string LastUpdateBy { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [IgnoreWhenUpdate]
        [Description("创建时间")]
        public DateTime? CreateOn { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [IgnoreWhenUpdate]
        [Description("创建人")]
        public string CreateBy { get; set; }
        /// <summary>
        /// 是否有效
        /// </summary>
        [Description("是否有效")]
        public int? Active { get; set; }
    }
}
