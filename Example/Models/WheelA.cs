using System;
using Microsoft.Extensions.Logging;
using SummerBoot.Core;

namespace Example.Models
{
  [Serializable]
    public class WheelA : IWheel
    {
        public string Name { get; set; } = "A";
        [Autowired]
        private ILogger<Car> Logger { set; get; }
        public void Scroll()
        {
            Logger.LogDebug("我是A轮胎，我正在滚");
        }
    }
}