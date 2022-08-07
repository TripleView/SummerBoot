using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        public static long CalculateTime(string actionName, Action action)
        {
            var sw = new Stopwatch();
            sw.Start();
            action();
            sw.Stop();
            var time = sw.ElapsedMilliseconds;
            Debug.WriteLine(actionName + ":" + time);
            return time;
        }

        public static async Task<long> CalculateTimeAsync(string actionName, Func<Task> action)
        {
            var sw = new Stopwatch();
            sw.Start();
            await action();
            sw.Stop();
            var time = sw.ElapsedMilliseconds;
            Debug.WriteLine(actionName + ":" + time);
            return time;
        }
        /// <summary>
        /// 1970的起始时间
        /// </summary>
        private static readonly DateTime Jan1st1970 = new DateTime
            (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        /// <summary>
        /// 判断时间差是否为0
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsZero(this TimeSpan time)
        {
            return time == TimeSpan.Zero;
        }

        /// <summary>
        /// 检查时间差是否为负值
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsNegative(this TimeSpan time)
        {
            return time.TotalSeconds < 0;
        }

        /// <summary>
        /// 返回自1970年以来以毫秒为单位的UTC时间
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        /// <summary>
        /// 获得毫微秒
        /// </summary>
        /// <returns></returns>
        public static long NanoTime()
        {
            return (DateTime.UtcNow - Jan1st1970).Ticks/10;
        }

        /// <summary>
        /// 返回自1970年以来以毫秒为单位的UTC时间
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static long CurrentSeconds()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalSeconds;
        }
    }
}