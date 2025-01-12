using System;

namespace SummerBoot.Feign.Nacos.Dto;

public class FeignCacheEntity
{
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpirationTime { get; set; }

    public object Data { get; set; }

    public bool IsEffective => (DateTime.Now - ExpirationTime).TotalSeconds < 0;
}