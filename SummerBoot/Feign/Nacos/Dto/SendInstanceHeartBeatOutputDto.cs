using System;
using System.Collections.Generic;
using SummerBoot.Feign.Attributes;

namespace SummerBoot.Feign.Nacos.Dto
{
    /// <summary>
    /// 发送心跳的返回值dto
    /// </summary>
    public class SendInstanceHeartBeatOutputDto
    {
        /// <summary>
        /// 心跳时间间隔
        /// </summary>
   
        public int ClientBeatInterval { get; set; }

        public bool LightBeatEnabled { get; set; }
    }

}