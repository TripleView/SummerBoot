using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace SummerBoot.Repository.Druid
{
    /// <summary>
    /// 判断connection是否有效的校验器接口
    /// </summary>
    public interface IValidConnectionChecker
    {
        /// <summary>
        /// 判断是否有效
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="querySql"></param>
        /// <param name="validationQueryTimeout"></param>
        /// <returns></returns>
        bool IsValidConnection(IDbConnection dbConnection, string querySql, int validationQueryTimeout);
        /// <summary>
        /// 从字典获得校验数据
        /// </summary>
        /// <param name="dictionary"></param>
        void ConfigFromDictionary(IDictionary<string,string> dictionary);
    }
}