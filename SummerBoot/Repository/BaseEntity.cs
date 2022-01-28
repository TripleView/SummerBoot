using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository.Attributes;

namespace SummerBoot.Repository
{
    public abstract class BaseEntity
    {
        public BaseEntity()
        {
        }
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        
        public int Id { set; get; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime? LastUpdateOn { get; set; }
        /// <summary>
        /// 最后更新人
        /// </summary>
        public string LastUpdateBy { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [IgnoreWhenUpdate]
        public DateTime? CreateOn { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [IgnoreWhenUpdate]
        public string CreateBy { get; set; }
        /// <summary>
        /// 软删除标记
        /// </summary>
        public int? Active { get; set; }

    }
}
