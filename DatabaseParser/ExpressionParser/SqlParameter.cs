﻿using System;

namespace DatabaseParser.ExpressionParser
{
    public class SqlParameter
    {
        public string ParameterName { get; set; }
        public object Value { get; set; }
        /// <summary>
        /// 参数的类型
        /// </summary>
        public Type ParameterType { get; set; }
    }
}