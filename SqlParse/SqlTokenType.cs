using System;
using System.Collections.Generic;
using System.Text;

namespace SqlParse
{
    /// <summary>
    /// 词类型
    /// </summary>
    public enum SqlTokenType
    {
        /// <summary>
        /// 关键词，比如select，order by
        /// </summary>
        KeyWord=1 ,
        /// <summary>
        /// 符号，比如空格，逗号等
        /// </summary>
        Symbol=2 ,
     
        /// <summary>
        /// 标识符，如表名,列名
        /// </summary>
        Identifier=3,
        /// <summary>
        /// 变量
        /// </summary>
        Variable=4,
        /// <summary>
        /// 函数
        /// </summary>
        Function=5,
       
    }
}
