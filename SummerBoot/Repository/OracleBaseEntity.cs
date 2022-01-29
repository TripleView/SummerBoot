using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository.Attributes;

namespace SummerBoot.Repository
{
    public abstract class OracleBaseEntity
    {
        public OracleBaseEntity()
        {
        }
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity),Column("ID")]
        
        public int Id { set; get; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Column("LASTUPDATEON")]
        public DateTime? LastUpdateOn { get; set; }
        /// <summary>
        /// 最后更新人
        /// </summary>
        [Column("LASTUPDATEBY")]
        public string LastUpdateBy { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [IgnoreWhenUpdate]
        [Column("CREATEON")]
        public DateTime? CreateOn { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [IgnoreWhenUpdate]
        [Column("CREATEBY")]
        public string CreateBy { get; set; }
        /// <summary>
        /// 软删除标记
        /// </summary>
        [Column("ACTIVE")]
        public int? Active { get; set; }

    }
}
