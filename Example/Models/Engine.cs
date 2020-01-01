using SummerBoot.Cache;
using SummerBoot.Core;
using System;
using Microsoft.Extensions.Logging;

namespace Example.Models
{
    /// <summary>
    /// 汽车引擎
    /// </summary>
    public class Engine
    {
        [Value("HelpNumber")]
        public string HelpNumber { set; get; }


        [Autowired]
        public ILogger<Car> Logger { set; get; }
        //[Cacheable("db1", "wheel5", cacheManager: "redis")]
        //public virtual int GetWheelAs()
        //{
        //    //var result = new List<WheelA>() { new WheelA(), new WheelA() };
        //    Console.WriteLine("真实获得所有轮胎");
        //    var result = 1;
        //    return result;
        //}

        public virtual void Start()
        {
            Logger.LogDebug("发动机启动");
            Stop();
        }

        public virtual void Stop()
        {
           Logger.LogDebug("发动机熄火,拨打求救电话" + HelpNumber);
        }
    }
}