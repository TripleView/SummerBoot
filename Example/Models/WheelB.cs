using System;
using Microsoft.Extensions.Logging;
using SummerBoot.Core;

namespace Example.Models
{
    [Serializable]
    public class WheelB : IWheel
    {
        public string Name { get; set; } = "B";
        [Autowired]
        private ILogger<Car> Logger { set; get; }
        public void Scroll()
        {
            Logger.LogDebug("我是B轮胎，我正在滚");
        }
    }
}