using SummerBoot.Cache;
using SummerBoot.Core;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Example.Models
{
    public class Car : ICar
    {
        [Autowired]
        public Engine Engine { set; get; }

        [Value("oilNo")]
        public int OilNo { set; get; }

        [Autowired]
        [Qualifier("A轮胎")]
        private IWheel Wheel { set; get; }

        [Autowired]
        private ILogger<Car> Logger { set; get; }

        [Transactional]
        public void Fire()
        {
            Logger.LogDebug("加满" + OilNo + "号汽油,点火");

            Wheel.Scroll();

            Engine.Start();
        }

        [Cacheable("db1", "GetWheelAs", cacheManager:"redis",condition:"1+1=2")]
        public IEnumerable<WheelA> GetWheelAs()
        {
            var result=new List<WheelA>(){new WheelA(),new WheelA()};
            //Console.WriteLine("真实获得所有轮胎");
            Logger.LogWarning("真实获得所有轮胎");
            return result;
        }

        [CachePut("db1", "log2", cacheManager: "redis",condition:"1+1==2")]
        [CachePut("db1","log",cacheManager:"redis")]
        //[Cacheable("db1", "GetWheelNum", cacheManager: "redis", condition: "1+1==2")]
        public int GetWheelNum(int requestNum)
        {
            Logger.LogDebug("cachePut方法执行");
            return requestNum;
        }
    }
}