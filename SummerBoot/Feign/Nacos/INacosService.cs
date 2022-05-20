using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Feign.Attributes;
using SummerBoot.Feign.Nacos.Dto;

namespace SummerBoot.Feign.Nacos
{
    [FeignClient(Url = "${nacos:serviceAddress}")]
    public interface INacosService
    {
        /// <summary>
        /// 注册实例
        /// </summary>
        /// <returns></returns>
        [PostMapping("/nacos/v1/ns/instance")]
        Task<string> RegisterInstance([Query]NacosRegisterInstanceDto dto);
        /// <summary>
        /// 注销实例
        /// </summary>
        /// <returns></returns>
        [DeleteMapping("/nacos/v1/ns/instance")]
        Task<string> UnRegisterInstance([Query] NacosRegisterInstanceDto dto);
        /// <summary>
        /// 发送实例心跳
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [PutMapping("/nacos/v1/ns/instance/beat")]
        Task<SendInstanceHeartBeatOutputDto> SendInstanceHeartBeat([Query] SendInstanceHeartBeatDto dto);

        /// <summary>
        /// 查询实例列表
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [GetMapping("/nacos/v1/ns/instance/list")]
        Task<QueryInstanceListOutputDto> QueryInstanceList([Query] QueryInstanceListInputDto dto);
    }
}